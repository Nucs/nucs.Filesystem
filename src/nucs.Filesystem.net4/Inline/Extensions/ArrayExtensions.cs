using System;
using System.Collections.Generic;

namespace nucs.Collections {
    internal static class ByteArrayRocks {
        /// <summary>
        ///     Locates the position of an array inside array.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="candidate"></param>
        /// <returns></returns>

        internal static IEnumerable<int> Locate(this byte[] self, byte[] candidate) {
            if (IsEmptyLocate(self, candidate))
                yield break;
            for (var i = 0; i < self.Length; i++) {
                if (!IsMatch(self, i, candidate))
                    continue;
                yield return i;
            }
        }

        /// <summary>
        ///     Locates the position of an array inside array and returns the postion after the needle.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="candidate"></param>
        /// <returns></returns>

        internal static IEnumerable<int> LocatePast(this byte[] self, byte[] candidate) {
            if (IsEmptyLocate(self, candidate))
                yield break;
            for (var i = 0; i < self.Length; i++) {
                if (!IsMatch(self, i, candidate))
                    continue;
                if (i + candidate.Length>=self.Length) yield break;
                yield return i+candidate.Length;
            }
        }


        private static bool IsMatch(byte[] array, int position, byte[] candidate) {
            if (candidate.Length > (array.Length - position))
                return false;

            for (int i = 0; i < candidate.Length; i++)
                if (array[position + i] != candidate[i])
                    return false;

            return true;
        }

        private static bool IsEmptyLocate(byte[] array, byte[] candidate) {
            return array == null
                   || candidate == null
                   || array.Length == 0
                   || candidate.Length == 0
                   || candidate.Length > array.Length;
        }
    }
}