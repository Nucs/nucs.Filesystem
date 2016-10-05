using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StringManipulator {
    internal class ManipulatedString { 
        private string _src;
        private int cursor = -1;
        private int endcursor = -1;
        private StringComparison comparison;

        /// <summary>
        /// has the cursor ever change?
        /// </summary>
        private bool reachedend;
        internal ManipulatedString(string str, StringComparison comparison = StringComparison.InvariantCultureIgnoreCase)
        {
            _src = str;
            this.comparison = comparison;
        }


        /// <summary>
        ///     Sets cursor till the first occurence, if not found - stays the last occurence.
        /// </summary>
        /// <returns></returns>
        internal ManipulatedString SkipUntill(string str) {
            if (reachedend) return this;
            var index = _src.IndexOf(str, cursor == -1 ? 0 : cursor+1, comparison);
            if (index == -1)
            {
                reachedend = true;
                return this;

            }
            
            cursor = index;
            return this;
        }

        /// <summary>
        ///     Sets cursor till the first occurence, if not found - stays the last occurence.
        /// </summary>
        /// <returns></returns>
        internal ManipulatedString Skip(int chars) {
            if (reachedend) return this;
            if (chars == 0)
                return this;
            if ((cursor ==-1 ? 0 : cursor) + chars + 1 > _src.Length) {//does exceed max
                reachedend = true;
                return this;
            }
            if (cursor == -1)
                cursor = 0;
            cursor += chars;
            return this;
        }

        /// <summary>
        ///     Takes the End Cursor n chars back. 
        /// </summary>
        /// <returns></returns>
        internal ManipulatedString EndBack(int chars) {
            var newec = endcursor - chars;
            if (newec < 0)
                throw new IndexOutOfRangeException("Cannot move the end cursor below index 0.");

            if (newec < cursor)
                throw new IndexOutOfRangeException("Cannot move the end cursor behind the cursor.");

            endcursor = newec;
            return this;
        }
        /// <summary>
        ///     Takes the End Cursor n chars forward. 
        /// </summary>
        /// <returns></returns>
        internal ManipulatedString EndForward(int chars) {
            var newec = endcursor + chars;
            if (newec+1 > _src.Length)
                throw new IndexOutOfRangeException("Cannot move the end cursor above length of the string.");

            endcursor = newec;
            return this;
        }

        /// <summary>
        ///     Takes the Cursor n chars forward. 
        /// </summary>
        /// <returns></returns>
        internal ManipulatedString Forward(int chars) {
            var newc = cursor + chars;
            if (newc + 1 > _src.Length)
                throw new IndexOutOfRangeException("Cannot move the cursor above length of the string.");
            if (newc > endcursor)
                throw new IndexOutOfRangeException("Cannot move the cursor above end cursor.");

            cursor = newc;
            return this;
        }

        /// <summary>
        ///     Takes the Cursor n chars back. 
        /// </summary>
        /// <returns></returns>
        internal ManipulatedString Back(int chars) {
            var newec = cursor - chars;
            if (newec < 0)
                throw new IndexOutOfRangeException("Cannot move the cursor below index 0.");

            cursor = newec;
            return this;
        }
        /// <summary>
        ///     Takes the Cursor and End Cursor and moves both to the right.
        /// </summary>
        /// <returns></returns>
        internal ManipulatedString ShiftRight(int chars) {
            return this.EndForward(chars).Forward(chars);
        }

        /// <summary>
        ///     Takes the Cursor and End Cursor and moves both to the left.
        /// </summary>
        /// <returns></returns>
        internal ManipulatedString ShiftLeft(int chars) {
            return this.Back(chars).EndBack(chars);
        }

        /// <summary>
        ///     Sets the End Cursor to the end.
        /// </summary>
        /// <returns></returns>
        internal ManipulatedString TillEnd() {
            if (reachedend) return this;
            endcursor = _src.Length-1;
            return this;
        }



        /// <summary>
        ///     Sets the End Cursor to the first occurence of str. (cursor is set on the first letter of the occurence)
        /// </summary>
        /// <returns></returns>
        /// <param name="endifnotfound">If the str is not found, it will set the End Cursor as last char</param>
        internal ManipulatedString Till(string str, bool endifnotfound = false) {
            if (reachedend) return this;
            if (cursor >= _src.Length - 1) //exxceeded end
                return TillEnd();
            var index = _src.IndexOf(str, cursor == -1 ? 0 : cursor+1, comparison);
            if (index == -1) {
                if (endifnotfound)
                {
                    endcursor = _src.Length - 1;
                } else
                {
                    reachedend = true;
                }
                return this;
            }

            endcursor = index;

            if (cursor == -1)
                cursor = 0;
            return this;
        }


        /// <summary>
        /// Finalizes the query and returns the result.
        /// </summary>
        /// <returns></returns>
        internal string Finallize() {
            if (reachedend)
                return null;
            
            if (cursor == -1 && endcursor == -1)
                return null;
            else if(cursor == -1 && endcursor != -1)
                return _src.Substring(0, endcursor+1);

            return _src.Substring(cursor,endcursor + 1 - cursor);
        }

    }

    internal static class ManipulatedStringExtensions {
        internal static ManipulatedString Manipulate(this string str) {
            return new ManipulatedString(str);
        }
        /// <summary>
        ///     Gives the string 
        /// </summary>
        /// <param name="src"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        internal static string Between(this string src, string start, string end) {
            return src.Manipulate().SkipUntill(start).Skip(start.Length).Till(end).EndBack(end.Length).Finallize();
        }

        /// <summary>
        ///     Gives the string 
        /// </summary>
        /// <param name="src"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        internal static IEnumerable<string> AllBetween(this string src, string start, string end) {
            var str = src.Manipulate();
            while (true) { 
                string ret;
                try {
                    ret = str.SkipUntill(start).Skip(start.Length).Till(end).EndBack(end.Length).Finallize();
                    if (ret == null)
                        yield break;
                } catch (IndexOutOfRangeException) {
                    yield break;
                }

                yield return ret;
            }
        }
    }
}
