using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nucs.SystemCore.Boolean {
    internal static class BooleanTools {
        internal static bool Not(this bool sample) {
            return !sample;
        }

        internal static Bool Not(this Bool sample) {
            sample.value = !sample.value;
            return sample;
        }
    }
}
