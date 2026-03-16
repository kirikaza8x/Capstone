using AI.Application.Abstractions;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using FastBertTokenizer;  // ✅ Correct namespace for FastBertTokenizer v1.0.28
using Microsoft.Extensions.Logging;

namespace AI.Infrastructure.Services.Embedding
{
    /// <summary>
    /// Generates embeddings using ONNX Runtime with all-MiniLM-L6-v2.
    /// Uses FastBertTokenizer for efficient tokenization.
    /// </summary>
    public sealed class EmbeddingService : IEmbeddingService, IDisposable
    {
        private readonly InferenceSession _session;
        private readonly BertTokenizer _tokenizer;  
        private readonly ILogger<EmbeddingService> _logger;

        private const int MaxTokens = 128;
        private const int Dimension = 384;
        private bool _disposed;

        public string ModelName => "all-MiniLM-L6-v2";
        public int ModelDimension => Dimension;

        public EmbeddingService(
            string modelPath,
            string vocabPath,
            ILogger<EmbeddingService> logger)
        {
            _logger = logger;

            try
            {
                // Configure ONNX Runtime session
                var sessionOptions = new SessionOptions
                {
                    EnableMemoryPattern = true,
                    ExecutionMode = ExecutionMode.ORT_SEQUENTIAL,
                    InterOpNumThreads = 1,
                    IntraOpNumThreads = Environment.ProcessorCount,
                    GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL
                };

                // Try CUDA, fall back to CPU
                try
                {
                    sessionOptions.AppendExecutionProvider_CUDA(0);
                    _logger.LogInformation("ONNX: Using CUDA execution provider");
                }
                catch
                {
                    _logger.LogInformation("ONNX: CUDA not available, using CPU");
                }

                _session = new InferenceSession(modelPath, sessionOptions);
                _logger.LogInformation("ONNX session created: {ModelPath}", modelPath);
                _tokenizer = new BertTokenizer();
                _tokenizer.LoadVocabularyAsync(vocabPath, true).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize EmbeddingService");
                throw;
            }
        }

        public async Task<float[]> GenerateAsync(string text, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Array.Empty<float>();

            return await Task.Run(() =>
            {
                try
                {
                    ct.ThrowIfCancellationRequested();

                    var encoding = _tokenizer.Encode(text, MaxTokens);
                    int seqLen = encoding.InputIds.Length;

                    if (seqLen == 0)
                        return Array.Empty<float>();


                    var inputIds = new DenseTensor<long>(encoding.InputIds, new[] { 1, seqLen });
                    var attentionMask = new DenseTensor<long>(encoding.AttentionMask, new[] { 1, seqLen });
                    var tokenTypeIds = new DenseTensor<long>(encoding.TokenTypeIds, new[] { 1, seqLen });

                    // Prepare ONNX inputs
                    var inputs = new List<NamedOnnxValue>
                    {
                        NamedOnnxValue.CreateFromTensor("input_ids", inputIds),
                        NamedOnnxValue.CreateFromTensor("attention_mask", attentionMask),
                        NamedOnnxValue.CreateFromTensor("token_type_ids", tokenTypeIds),
                    };

                    // Run inference
                    using var results = _session.Run(inputs);

                    // Extract last_hidden_state: shape [1, seqLen, 384]
                    var output = results.FirstOrDefault(r => r.Name == "last_hidden_state")
                        ?? throw new InvalidOperationException("Output 'last_hidden_state' not found");

                    var tensor = output.AsTensor<float>();

                    // Mean pooling over sequence dimension
                    var pooled = MeanPool(tensor, seqLen);

                    // L2 normalize for cosine similarity
                    return L2Normalize(pooled);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Embedding generation cancelled for text: {Text}", text);
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Embedding generation failed for text: {Text}", text);
                    throw new InvalidOperationException("Failed to generate embedding", ex);
                }
            }, ct);
        }

        /// <summary>
        /// Mean pooling over sequence dimension of last_hidden_state.
        /// Input tensor shape: [batch=1, seqLen, hiddenSize=384]
        /// </summary>
        private static float[] MeanPool(Microsoft.ML.OnnxRuntime.Tensors.Tensor<float> tensor, int seqLen)
        {
            var result = new float[Dimension];

            // tensor[batch, sequence_position, hidden_dimension]
            for (int t = 0; t < seqLen; t++)
            {
                for (int d = 0; d < Dimension; d++)
                {
                    result[d] += tensor[0, t, d];
                }
            }

            // Average
            for (int d = 0; d < Dimension; d++)
            {
                result[d] /= seqLen;
            }

            return result;
        }

        /// <summary>
        /// L2 normalize vector to unit length (enables cosine similarity via dot product)
        /// </summary>
        private static float[] L2Normalize(float[] vector)
        {
            double norm = Math.Sqrt(vector.Sum(x => (double)x * x));

            if (norm < 1e-8)
                return vector;  

            var result = new float[Dimension];
            for (int d = 0; d < Dimension; d++)
            {
                result[d] = (float)(vector[d] / norm);
            }
            return result;
        }

        /// <summary>
        /// Generate embeddings for multiple texts (sequential for now)
        /// </summary>
        public async Task<List<float[]>> GenerateBatchAsync(
            IEnumerable<string> texts,
            CancellationToken ct = default)
        {
            var textList = texts.Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
            if (!textList.Any())
                return new List<float[]>();

            var results = new List<float[]>(textList.Count);
            foreach (var text in textList)
            {
                results.Add(await GenerateAsync(text, ct));
            }
            return results;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _session?.Dispose();
                _disposed = true;
            }
        }
    }
}