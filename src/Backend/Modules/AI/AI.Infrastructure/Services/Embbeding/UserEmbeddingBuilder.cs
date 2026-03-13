using AI.Application.Abstractions;
using AI.Domain.Entities;

namespace AI.Infrastructure.Services
{
    public class UserEmbeddingBuilder : IUserEmbeddingBuilder
    {
        private readonly IEmbeddingModel _embeddingModel;
        private const int Dimension = 384; // Matching standard Sentence-Transformers

        public UserEmbeddingBuilder(IEmbeddingModel embeddingModel)
        {
            _embeddingModel = embeddingModel;
        }

        public float[] Build(List<UserInterestScore> scores)
        {
            var userVector = new float[Dimension];
            double totalWeight = 0;

            foreach (var score in scores)
            {
                // Generate embedding for the category
                var categoryEmbedding = _embeddingModel.GenerateEmbeddingAsync(score.Category).Result;

                if (categoryEmbedding == null || categoryEmbedding.Length != Dimension)
                    continue;

                // Weight the vector (Score represents intensity of interest)
                float weight = (float)score.Score;
                totalWeight += weight;

                for (int i = 0; i < Dimension; i++)
                {
                    userVector[i] += categoryEmbedding[i] * weight;
                }
            }

            // Normalize the result (Vector must have a length of 1 for Cosine Similarity)
            return Normalize(userVector, totalWeight);
        }

        private float[] Normalize(float[] vector, double totalWeight)
        {
            if (totalWeight <= 0) return vector;

            float sumOfSquares = 0;
            for (int i = 0; i < vector.Length; i++)
            {
                // Average based on weight
                vector[i] /= (float)totalWeight;
                sumOfSquares += vector[i] * vector[i];
            }

            // Standard Unit Normalization
            float magnitude = MathF.Sqrt(sumOfSquares);
            if (magnitude > 0)
            {
                for (int i = 0; i < vector.Length; i++)
                {
                    vector[i] /= magnitude;
                }
            }

            return vector;
        }
    }
}