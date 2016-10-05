using System;
using System.Text;

namespace nucs.Startup.Internal {
    internal static class StringGenerator {
        private static Random rand = null;

        internal static string Generate(int len = 10) {
            return Generate(rand ?? (rand = new Random()), len);
        }


        internal static string Generate(Random rand, int len = 10) {
            if (len <= 0) return "";
            char ch;
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < len; i++) {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26*rand.NextDouble() + 65)));
                builder.Append(ch);
            }
            for (int i = 0; i < len; i++) {
                if (rand.Next(1, 3) == 1)
                    builder[i] = char.ToLowerInvariant(builder[i]);
            }
            return builder.ToString();
        }
    }
}