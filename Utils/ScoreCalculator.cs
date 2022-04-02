using UnityEngine;

namespace Toggle.Utils
{
    public static class ScoreCalculator
    {
        public static int Calculate(int actualClickCount, int minClickCount, float Q)
        {
            float score = 100.0f - Q * ((actualClickCount - minClickCount) / (float) minClickCount);
            score = Mathf.Clamp(score, 10, 100);

            return Mathf.FloorToInt(score);
        }

        public static int CalculateStarCount(int score)
        {
            if (score == 100) return 3;
            else if (score >= 73) return 2;
            else return 1;
        }
    }
}