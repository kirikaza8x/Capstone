using AI.Application.Abstractions;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Security.Cryptography;
using System.Text;

namespace AI.Infrastructure.Embeddings
{
    /// <summary>
    /// Local ONNX embedding model using SentenceTransformers (e.g., all-MiniLM-L6-v2).
    /// Produces 384-dimensional embeddings compatible with the pgvector schema.
    /// </summary>
    public class EmbeddingModel : IEmbeddingModel, IDisposable
    {
        private readonly InferenceSession _session;
        private readonly string _inputIdsName;
        private readonly string _attentionMaskName;
        private readonly string _outputName;
        private bool _disposed;

        public EmbeddingModel(string modelPath = "Models/model.onnx")
        {
            var fullPath = Path.IsPathRooted(modelPath)
                ? modelPath
                : Path.Combine(AppContext.BaseDirectory, modelPath);

            var sessionOptions = new SessionOptions
            {
                InterOpNumThreads = 1,
                IntraOpNumThreads = 1,
                EnableMemoryPattern = true
            };

            // Add CPU execution provider (or use CUDA for GPU acceleration)
            // sessionOptions.AppendExecutionProvider_CUDA();

            _session = new InferenceSession(fullPath, sessionOptions);

            // Discover input/output names from the ONNX model
            var inputNames = new List<string>();
            foreach (var input in _session.InputMetadata)
            {
                inputNames.Add(input.Key);
            }

            _inputIdsName = inputNames.FirstOrDefault(n => n.Contains("input_ids", StringComparison.OrdinalIgnoreCase))
                ?? inputNames.FirstOrDefault()
                ?? throw new InvalidOperationException("Could not find input_ids in ONNX model");

            _attentionMaskName = inputNames.FirstOrDefault(n => n.Contains("attention_mask", StringComparison.OrdinalIgnoreCase))
                ?? inputNames.FirstOrDefault(n => n != _inputIdsName)
                ?? _inputIdsName;

            var outputNames = new List<string>();
            foreach (var output in _session.OutputMetadata)
            {
                outputNames.Add(output.Key);
            }

            // Prefer token_embeddings or sentence_embedding output
            _outputName = outputNames.FirstOrDefault(n => n.Contains("sentence_embedding", StringComparison.OrdinalIgnoreCase))
                ?? outputNames.FirstOrDefault(n => n.Contains("token_embeddings", StringComparison.OrdinalIgnoreCase))
                ?? outputNames.FirstOrDefault()
                ?? throw new InvalidOperationException("Could not find output in ONNX model");
        }

        public Task<float[]> GenerateEmbeddingAsync(
            string input,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Input cannot be empty", nameof(input));

            var embedding = GenerateEmbedding(input);
            return Task.FromResult(embedding);
        }

        private float[] GenerateEmbedding(string text)
        {
            // Tokenize input (simple wordpiece-style tokenization for SentenceTransformers)
            var tokens = Tokenize(text);
            var inputIds = tokens.InputIds;
            var attentionMask = tokens.AttentionMask;

            // Create input tensors
            var inputIdsTensor = new DenseTensor<long>(inputIds, [1, inputIds.Length]);
            var attentionMaskTensor = new DenseTensor<long>(attentionMask, [1, attentionMask.Length]);

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor(_inputIdsName, inputIdsTensor),
                NamedOnnxValue.CreateFromTensor(_attentionMaskName, attentionMaskTensor)
            };

            using var results = _session.Run(inputs);
            var output = results.FirstOrDefault(r => r.Name == _outputName)
                ?? results.First();

            var tensor = output.AsTensor<float>();
            var embedding = GetPoolingResult(tensor, attentionMask);

            // L2 normalize for cosine similarity
            return Normalize(embedding);
        }

        private float[] GetPoolingResult(Tensor<float> tensor, long[] attentionMask)
        {
            // For sentence_embedding output (already pooled), return as-is
            if (tensor.Dimensions.Length == 2)
            {
                return tensor.ToArray();
            }

            // For token_embeddings, apply mean pooling with attention mask
            var batchSize = tensor.Dimensions[0];
            var seqLength = tensor.Dimensions[1];
            var hiddenSize = tensor.Dimensions[2];

            var sum = new float[hiddenSize];
            float count = 0;

            for (int t = 0; t < seqLength; t++)
            {
                if (attentionMask[t] == 1)
                {
                    for (int h = 0; h < hiddenSize; h++)
                    {
                        sum[h] += tensor[0, t, h];
                    }
                    count++;
                }
            }

            if (count == 0) return sum;

            for (int h = 0; h < hiddenSize; h++)
            {
                sum[h] /= count;
            }

            return sum;
        }

        private (long[] InputIds, long[] AttentionMask) Tokenize(string text, int maxLength = 384)
        {
            // Simple tokenization: split on whitespace and convert to pseudo-token IDs
            // For production, use a proper tokenizer (e.g., BERT tokenizer)
            var words = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var inputIds = new List<long> { 101 }; // [CLS] token
            var attentionMask = new List<long> { 1 };

            foreach (var word in words.Take(maxLength - 2))
            {
                // Simple hash-based tokenization (replace with real tokenizer for production)
                var tokenId = HashToken(word) % 30522 + 100; // Map to vocab range
                inputIds.Add(tokenId);
                attentionMask.Add(1);
            }

            // Pad to maxLength
            while (inputIds.Count < maxLength)
            {
                inputIds.Add(0); // [PAD] token
                attentionMask.Add(0);
            }

            inputIds.Add(102); // [SEP] token
            attentionMask.Add(0);

            return (inputIds.ToArray(), attentionMask.ToArray());
        }

        private long HashToken(string token)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            return BitConverter.ToInt64(hash, 0);
        }

        private float[] Normalize(float[] vector)
        {
            float sum = 0;
            foreach (var v in vector)
                sum += v * v;

            var magnitude = MathF.Sqrt(sum);
            if (magnitude == 0) return vector;

            for (int i = 0; i < vector.Length; i++)
                vector[i] /= magnitude;

            return vector;
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