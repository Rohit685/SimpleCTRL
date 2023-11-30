using System;

namespace SimpleCTRL.Utils
{
    internal static class Calc
    {
		public static float Clamp(float value, float min, float max)
		{
			if (value < min)
			{
				return min;
			}
			if (value > max)
			{
				return max;
			}
			return value;
		}

		public static int Clamp(int value, int min, int max)
		{
			return Math.Min(max, Math.Max(min, value));
		}
	}
}
