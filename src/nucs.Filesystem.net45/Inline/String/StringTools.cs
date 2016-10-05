using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using StringManipulator;

namespace nucs.SystemCore.String {
    /*[DebuggerStepThrough]*/
    internal static class StringTools {

        internal static readonly string EnglishAbc = "abcdefghijklmnopqrstuvwxyz";
        internal static readonly string EnglishABC = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        internal delegate bool Comparison(string o);

        #region Searching
        internal static int[] IndexBetween(this string str, string signatureA, string signatureB, string endSignature, int startIndex = 0) {
            if (string.IsNullOrEmpty(signatureA + signatureB))
                return new[] { 0, str.Length - 1 };
            bool foundA = false, foundB = false;
            int a = -1, b = -1;
            for (var i = startIndex; i <= str.Length; i++) { //main iteration
                if (foundA && foundB)
                    break;
                for (var j = 0; j < endSignature.Length; j++) {
                    if (str[i + j] != endSignature[j])
                        goto _continue1;
                }
                goto _exit;
                _continue1:
                if (!foundA)
                    for (var j = 0; j < signatureA.Length; j++) {
                        if (str[i+j] != signatureA[j])
                            goto end;
                    }
                else goto searchB;
                a = i = i + signatureA.Length;
                foundA = true;
                searchB:
                if (!foundB)
                    for (var j = 0; j < signatureB.Length; j++) {
                        try {
                            if (str[i + j] != signatureB[j])
                                goto end;
                        }
                        catch {
                            b = -1;
                            foundB = true;
                            goto end;
                        }
                    }
                b = i;
                foundB = true;
                end:
                continue;
            }
            _exit:
            return new [] {a, b};
        }

        /// <summary>
        /// Example: (signA = ABCC, signB = BBCA => astweuitysulvkjcnvbjkABCCaiultasiojaBBCA) returns index of letter C in first word and first B at last word
        /// </summary>
        internal static int[] IndexBetween(this string str, string signatureA, string signatureB, int startIndex = 0, int endIndex = -1) {
            if (endIndex == -1)
                endIndex = str.Length - 1;
            if (string.IsNullOrEmpty(signatureA + signatureB))
                return new[] { 0, str.Length - 1 };
            bool foundA = false, foundB = false;
            int a = -1, b = -1;
            for (var i = startIndex; i <= endIndex; i++) {
                if (foundA && foundB)
                    break;
                if (!foundA)
                    for (var j = 0; j < signatureA.Length; j++) {
                        if (str[i+j] != signatureA[j])
                            goto end;
                    }
                else goto searchB;
                a = i = i + signatureA.Length;
                foundA = true;
                searchB:
// ReSharper disable ConditionIsAlwaysTrueOrFalse
                if (!foundB)
// ReSharper restore ConditionIsAlwaysTrueOrFalse
                    for (var j = 0; j < signatureB.Length; j++) {
                        try {
                            if (str[i + j] != signatureB[j])
                                goto end;
                        }
                        catch {
                            b = -1;
                            foundB = true;
                            goto end;
                        }
                    }
                b = i;
                foundB = true;
                end:
                continue;
            }
            return new [] {a, b};
        }

        internal static int[] IndexOfBetween(this string str, string signatureA, string signatureB, int startIndex = 0, int endIndex = -1, StringComparison comparison = StringComparison.OrdinalIgnoreCase) {
            if (endIndex == -1)
                endIndex = str.Length-1;
            if (string.IsNullOrEmpty(signatureA+signatureB))
                return new [] {0, str.Length-1};
            int a /*= -1*/ /*= -1*/;
            if (string.IsNullOrEmpty(signatureA)) {
                a = 0;
                goto srb;
            }

            a = str.IndexOf(signatureA, startIndex, comparison) + signatureA.Length;
            srb:
            var b = str.IndexOf(signatureB, a, endIndex-a+1,  comparison);
            return new[] {a, b};
        }

        internal static string StringBetween(this string str, string signatureA, string signatureB, int startIndex = 0, int endIndex = -1) {
            try {
                var i = IndexBetween(str, signatureA, signatureB, startIndex, endIndex);
                if (i[0] == -1)
                    return string.Empty;
                if (i[1] == -1)
                    return str.Substring(i[0], str.Length - i[0]);
                return str.Substring(i[0], i[1] - i[0]);
            }
            catch { return string.Empty; }
        }

        internal static string StringOfBetween(this string str, string signatureA, string signatureB, int startIndex = 0, int endIndex = -1, StringComparison comparison = StringComparison.OrdinalIgnoreCase) {
            var i = IndexOfBetween(str, signatureA, signatureB, startIndex, endIndex, comparison);
            if (i[0] == -1)
                return string.Empty;
            if (i[1] == -1)
                return str.Substring(i[0], str.Length - i[0]);
            return str.Substring(i[0], i[1] - i[0]);
        }
        
        /// <summary>
        /// example: sdfgsdfgsdfLOLxxxLOLhgfhLOLbbbbLOLssd - will return xxx, bbbb
        /// <returns></returns>
        /// </summary>
        [DebuggerStepThrough]
        internal static IEnumerable<string> StringsBetween(this string str, string signatureA, string signatureB, bool AddWhenNoClosureFoundAtEnd = false, int startIndex = 0, int endIndex = -1) {
            if (endIndex == -1)
                endIndex = str.Length - 1;
            int a = startIndex, b = endIndex;
            while (true) {
                var res = IndexBetween(str, signatureA, signatureB, a, b);
                if (res[0] == -1 || res[1] >= endIndex)
                    break;
                a = res[1];
                if (res[1] == -1) {
                    if (AddWhenNoClosureFoundAtEnd) {
                        string s = null;
                        try {
                        s = str.Substring(res[0], (endIndex + 1 - res[0]));
                        } catch{}
                        if (s!=null)
                        yield return s;
                    }
                    yield break;
                }
                yield return str.Substring(res[0], res[1] - res[0]);
            }
        }
        
         /// <summary>
        /// example: sdfgsdfgsdfLOLxxxLOLhgfhLOLbbbbLOLssd - will return xxx, bbbb
        /// <returns></returns>
        /// </summary>
        [DebuggerStepThrough]
        internal static IEnumerable<string> StringsBetween(this string str, string signatureA, string signatureB, Comparison comparer, bool BreakOnFirstYield = false , int startIndex = 0, int endIndex = -1) {
            if (endIndex == -1)
                endIndex = str.Length - 1;
            int a = startIndex, b = endIndex;
            while (true) {
                var res = IndexBetween(str, signatureA, signatureB, a, b);
                if (res[0] == -1 || res[1] >= endIndex)
                    break;
                a = res[1];
                if (res[1] == -1) {
                    yield break;
                }
                var s = str.Substring(res[0], res[1] - res[0]);
                if (comparer.Invoke(s)) {
                    yield return s;
                    if (BreakOnFirstYield)
                        break;
                }
            }
        }

        /// <summary>
        /// retrieves a sentence between signatureA and signatureB and returns the last letter of signatureB.
        /// </summary>
        internal static List<int> IndexesOfCompared(this string str, string signatureA, string signatureB, Comparison comparer, bool BreakOnFirstYield = false , int startIndex = 0, int endIndex = -1) {
            if (endIndex == -1)
                endIndex = str.Length - 1;
            int a = startIndex, b = endIndex;
            var items = new List<int>();
            while (true) {
                var res = IndexBetween(str, signatureA, signatureB, a, b);
                if (res[0] == -1 || res[1] >= endIndex)
                    break;
                a = res[1];
                if (res[1] == -1) {
                    break;
                }

                var s = str.Substring(res[0], res[1] - res[0]);
                if (comparer.Invoke(s)) {
                    items.Add(res[1] + signatureB.Length - 1);
                    if (BreakOnFirstYield)
                        break;
                }
            }
            return items;
        }

        internal static string StringTill(this string str, string signature, int startIndex = 0, int endIndex = -1) {
            if (endIndex == -1)
                endIndex = str.Length - 1;
            for (var i = startIndex; i < endIndex+1; i++) {
                for (var j = 0; j < signature.Length; j++) {
                    try {
                        if (str[i + j] != signature[j])
                            goto @false;
                    } catch {goto returnfalse;}

                }
                return str.Substring(startIndex, i - startIndex);
                @false:
                continue;
            }
            returnfalse:
            return null;
        }
        
        [DebuggerStepThrough]
        internal static IEnumerable<string> StringsTill(this string str, string signature, bool includeAfterLastSignature = false, int startIndex = 0, int endIndex = -1) {
            if (endIndex == -1)
                endIndex = str.Length - 1;
            int lastdetected = -1;
            for (var i = startIndex; i < endIndex+1; i++) {
                for (var j = 0; j < signature.Length; j++) {
                    try {
                        if (str[i + j] != signature[j])
                            goto @false;
                    } catch {goto @out;}
                }
                var r = str.Substring(startIndex, i - startIndex);
                lastdetected = (i += signature.Length - 1) + 1;
                startIndex += r.Length + signature.Length;
                yield return r;
                @false:
                continue;
                @out:
                break;
            }
            if (includeAfterLastSignature) {
                var s = string.Empty;
                try {
                    if (lastdetected > -1)
                    s = str.Substring(lastdetected, str.Length - lastdetected);
                } catch {}
                if (!string.IsNullOrEmpty(s))
                    yield return s;
            }
        }

        /// <summary>
        /// Also knows as explode in php.. 
        /// returns the object before first signature, the one between the next, and what comes after the last one.
        /// </summary>
        internal static IEnumerable<string> Split(this string str, string signature, int startIndex = 0, int endIndex = -1) {
            return StringsTill(str, signature, true, startIndex, endIndex);
        }

        /* //todo internal static StringBuilder SeekForSignatureTill(this StringBuilder str, string signature, string exit, int startIndex = 0) {
            if (str.Length < startIndex + 1)
                return null;

            for (int i = startIndex; i < str.Length; i++) {
                
            }

            return str;
        }*/

        /*internal static IEnumerable<string> Split(string str, string signature) {
            int lastseen = 0, seenAt = -1, k=0, alreadyTaken=0;
            for (var i = alreadyTaken; i < str.Length; i++, k=i) {
                for (var j = 0; j < signature.Length; j++) {
                    try {
                        if (str[i + j] != signature[j])
                            goto @false;
                        seenAt = i;
                    } catch {
                        goto @out;
                    }
                }
                var r = str.Substring(lastseen, i - alreadyTaken);
                i += signature.Length - 1;

                lastseen = seenAt + signature.Length;
                alreadyTaken += r.Length + signature.Length;
                yield return r;
                @false:
                continue;
                @out:
                break;


            }
            yield return str.Substring(lastseen, k - alreadyTaken);
        }*/

        #endregion

        #region Deleting and replacing

        /// <summary>
        /// Given "01234lol" as target and indexes in an array {0, 1, 3} will return "24lol".
        /// </summary>
        /// <param name="target"></param>
        /// <param name="indexes">Which indexes to delete</param>
        /// <returns></returns>
        internal static string DeleteIndexes(this string target, ICollection<long> indexes) {
            if (string.IsNullOrEmpty(target) || indexes.Count == 0)
                return target;
            var tar = target.ToCharArray();
            const char deletechar = '♣';
            foreach (var i in indexes)
                if (i < tar.Length)
                    tar[i] = deletechar;
            return tar.ConvertToString().Replace(deletechar.ToString(CultureInfo.InvariantCulture), string.Empty);
        }

        /// <summary>
        /// Works same as "sas".Replace("a",""), but uses multiple samples to delete. if you pass in samples "abc" and use delete on "abcdefac", it will return "def".
        /// </summary>
        /// <param name="target">The string to be edited</param>
        /// <param name="samples">Samples that define what to remove: if you pass in samples "abc" and use delete on "abcdefac", it will return "def".</param>
        internal static string Delete(this string target, string samples) {
            if (string.IsNullOrEmpty(target) || string.IsNullOrEmpty(samples))
                return target;
            var tar = target.ToCharArray();
            const char deletechar = '♣'; //a char that most likely never to be used an the input
            for (var i = 0; i < tar.Length; i++) {
                for (var j = 0; j < samples.Length; j++) {
                    if (tar[i] == samples[j]) {
                        tar[i] = deletechar;
                        break;
                    }
                }
            }
            return tar.ConvertToString().Replace(deletechar.ToString(CultureInfo.InvariantCulture), string.Empty);
        }

        /// <summary>
        /// Give in sample " " (whitespace), and if the target is "eli is  the pro   !!), returns: "eli is the pro !!"
        /// </summary>
        /// <param name="target">The string to be edited</param>
        /// <param name="sample">Give in sample " " (whitespace), and if the target is "eli is  the pro   !!), returns: "eli is the pro !!"</param>
        internal static string DeleteDuplicateChars(this string target, char sample) {
            if (string.IsNullOrEmpty(target))
                return target;
            var indexes = new List<long>();
            for (var i = 0; i < target.Length; i++) {
                if (target[i] == sample)
                    for (var j = 1;/*true*/; j++) {
                        if (i + j < target.Length && target[i + j] == sample) {
                            indexes.Add(i+j);
                        } else
                            break;
                    }
            }
            return target.DeleteIndexes(indexes);
        }

        internal static string DeleteDuplicateCharsMultiple(this string target, string samples) {
            if (string.IsNullOrEmpty(target) || string.IsNullOrEmpty(samples))
                return target;
            var indexes = new List<long>();
            for (var i = 0; i < target.Length; i++) {
                for (int j = 0; j < samples.Length; j++) {
                    if (target[i] == samples[j])
                        for (var l = 1;/*true*/; l++) {
                            if (i + l < target.Length && target[i + l] == samples[j]) {
                                indexes.Add(i+l);
                            } else
                                break;
                        }
                }
            }
            return target.DeleteIndexes(indexes);
        }

        internal static string ReplaceMultiple(this string target, string samples, char replaceWith) {
            if (string.IsNullOrEmpty(target) || string.IsNullOrEmpty(samples))
                return target;
            var tar = target.ToCharArray();
            for (var i = 0; i < tar.Length; i++) {
                for (var j = 0; j < samples.Length; j++) {
                    if (tar[i] == samples[j]) {
                        tar[i] = replaceWith;
                        break;
                    }
                }
            }
            return tar.ConvertToString();
        }

        internal static string Truncate(this string s, int maxLength) {
            if (string.IsNullOrEmpty(s) || maxLength <= 0)
                return string.Empty;
            else if (s.Length > maxLength)
                return s.Substring(0, maxLength) + "...";
            else
                return s;
        }

        #endregion

        #region Comparison

        internal static bool Compare(this string Sample, string CompareTo) {
            try {
                return string.CompareOrdinal(Sample, CompareTo) == 0;
            } catch {
                return false;
            }
        }

        internal static bool StartsWithAny(this string Sample, params string[] CompareTo) {
            try {
                return CompareTo.Any(Sample.StartsWith);
            } catch {
                return false;
            }
        }

        internal static bool CompareAny(this string Sample, params string[] CompareTo) {
            try {
                return CompareTo.Any(s => string.CompareOrdinal(Sample, s) == 0);
            } catch {
                return false;
            }
        }

        internal static bool CompareAll(this string Sample, params string[] CompareTo) {
            try {
                return CompareTo.All(s => string.CompareOrdinal(Sample, s) == 0);
            } catch {
                return false;
            }
        }

        internal static string FirstOrDefault(this string testunit, params string[] tocompare) {
            return tocompare.FirstOrDefault(testunit.Contains);
        }

        internal static bool ContainsAny(this string testunit, string charsamps) {
            return testunit.Any(ch => charsamps.Any(nch => nch.Equals(ch)));
        }
        [DebuggerStepThrough]
        internal static bool IsNullOrEmpty(this string testunit) {
            return string.IsNullOrEmpty(testunit);
        }

        internal static bool IsNumeric(this string Expression) {
            double retNum;
            return Double.TryParse(Expression, System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
        }
        /// <summary>
        /// Gives a content between a clause
        /// </summary>
        /// <param name="item">the string to modify</param>
        /// <param name="clauseName">The name of the clause without the arrow marks</param>
        internal static string BetweenClause(this string item, string clauseName) {
            return item.StringBetween("<" + clauseName + ">", "</" + clauseName + ">");
        }

        internal static bool IsBetweenClause(this string message, string clauseName) {
            return message.Contains("<" + clauseName + ">") && message.Contains("</" + clauseName + ">");

        }

        internal static string BetweenQuotation(this string text) {
            return text.Between("\"", "\"");
        }
        #endregion
    }
}