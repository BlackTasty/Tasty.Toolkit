namespace Tasty.Patcher.Core
{
    public static class Parser
    {
        public static bool TryParse(string value, bool defaultValue)
        {
            if (bool.TryParse(value, out bool result))
                return result;
            else
                return defaultValue;
        }

        public static int TryParse(string value, int defaultValue)
        {
            double temp = TryParse(value, (double)defaultValue);
            if (temp > int.MaxValue)
            {
                return int.MaxValue;
            }
            else if (temp < int.MinValue)
            {
                return int.MinValue;
            }
            else
            {
                if (int.TryParse(value, out int result))
                    return result;
                else
                    return defaultValue;
            }
        }

        public static double TryParse(string value, double defaultValue)
        {
            if (double.TryParse(value, out double result))
                return result;
            else
                return defaultValue;
        }

        public static float TryParse(string value, float defaultValue)
        {
            if (float.TryParse(value, out float result))
                return result;
            else
                return defaultValue;
        }
    }
}
