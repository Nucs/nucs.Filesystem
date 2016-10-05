using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nucs.SystemCore {
    internal static class MathTools {
        internal static bool PrecentageDifferences(double up, double down, double deviation) {
            if (deviation < 0)
                throw new InvalidOperationException("Deviation cannot be below 0%. Devision affects both on negetive size and positive side");
            double res = up/down; //say its 1.025 and deviation 5/100 = 0.05
            //1.05 is bigger than 1.025 AND 0.95 is smaller than 1.025
            return (1 + deviation/100 >= res && 1 - deviation/100 <= res );
        }
    }
}
