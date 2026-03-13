using System.Security.Cryptography;
using System.Text;
using AI.Application.Abstractions;

namespace AI.Infrastructure.Embeddings
{
    public class LocalVectorBuilder : ILocalVectorBuilder
    {
        private const int Dimension = 128;

        public float[] BuildVector(string key)
        {
            var vector = new float[Dimension];

            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));

            for (int i = 0; i < Dimension; i++)
            {
                vector[i] = hash[i % hash.Length] / 255f;
            }

            return Normalize(vector);
        }

        private float[] Normalize(float[] vector)
        {
            float sum = 0;

            foreach (var v in vector)
                sum += v * v;

            var magnitude = MathF.Sqrt(sum);

            if (magnitude == 0)
                return vector;

            for (int i = 0; i < vector.Length; i++)
                vector[i] /= magnitude;

            return vector;
        }
    }
}