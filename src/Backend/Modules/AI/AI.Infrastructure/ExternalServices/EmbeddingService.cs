// AI.Infrastructure/Services/Embedding/EmbeddingService.cs
using AI.Application.Abstractions;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.Tokenizers;

namespace AI.Infrastructure.Services.Embedding
{
    public sealed class EmbeddingService : IEmbeddingService, IDisposable
    {
        private readonly InferenceSession _session;
        private readonly BertTokenizer _tokenizer;
        private const int MaxTokens = 128;
        private const int Dimension = 384;

        public EmbeddingService(string modelPath, string vocabPath)
        {
            _session = new InferenceSession(modelPath, new SessionOptions
            {
                EnableMemoryPattern = true,
                ExecutionMode = ExecutionMode.ORT_SEQUENTIAL
            });

            _tokenizer = BertTokenizer.Create(vocabPath);
        }

        public Task<float[]> GenerateAsync(string text, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Task.FromResult(new float[Dimension]);

            // Encode and truncate to MaxTokens
            var encoded = _tokenizer.EncodeToIds(text, MaxTokens, out _, out _);
            int seqLen = encoded.Count;

            var inputIds      = new DenseTensor<long>(new[] { 1, seqLen });
            var attentionMask = new DenseTensor<long>(new[] { 1, seqLen });
            var tokenTypeIds  = new DenseTensor<long>(new[] { 1, seqLen });

            for (int i = 0; i < seqLen; i++)
            {
                inputIds[0, i]      = encoded[i];
                attentionMask[0, i] = 1;
                tokenTypeIds[0, i]  = 0;
            }

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input_ids",      inputIds),
                NamedOnnxValue.CreateFromTensor("attention_mask", attentionMask),
                NamedOnnxValue.CreateFromTensor("token_type_ids", tokenTypeIds),
            };

            using var results = _session.Run(inputs);

            var lastHiddenState = results
                .First(r => r.Name == "last_hidden_state")
                .AsEnumerable<float>()
                .ToArray();

            var pooled = MeanPool(lastHiddenState, seqLen);
            return Task.FromResult(L2Normalise(pooled));
        }

        private static float[] MeanPool(float[] hiddenState, int seqLen)
        {
            var result = new float[Dimension];

            for (int t = 0; t < seqLen; t++)
            {
                int offset = t * Dimension;
                for (int d = 0; d < Dimension; d++)
                    result[d] += hiddenState[offset + d];
            }

            for (int d = 0; d < Dimension; d++)
                result[d] /= seqLen;

            return result;
        }

        private static float[] L2Normalise(float[] vector)
        {
            double norm = Math.Sqrt(vector.Sum(x => (double)x * x));
            if (norm == 0) return vector;

            var result = new float[Dimension];
            for (int d = 0; d < Dimension; d++)
                result[d] = (float)(vector[d] / norm);
            return result;
        }

        public void Dispose() => _session.Dispose();
    }
}