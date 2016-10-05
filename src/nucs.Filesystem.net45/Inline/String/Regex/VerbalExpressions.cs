/*!
 * CSharpVerbalExpressions v0.1
 * https://github.com/VerbalExpressions/CSharpVerbalExpressions
 * 
 * @psoholt
 * 
 * Date: 2013-07-26
 * 
 * Additions and Refactoring
 * @alexpeta
 * 
 * Date: 2013-08-06
 */

using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace nucs.SystemCore.String.Regex {
    internal class VerbalExpressions {
        #region Statics

        /// <summary>
        ///     Returns a default instance of VerbalExpressions
        ///     having the Multiline option enabled
        /// </summary>
        internal static VerbalExpressions DefaultExpression {
            get { return new VerbalExpressions(); }
        }

        #endregion Statics

        #region Private Members

        private RegexOptions _modifiers = RegexOptions.Multiline;
        private string _prefixes = "";
        private string _source = "";
        private string _suffixes = "";

        #endregion Private Members

        #region Private Properties

        private string RegexString {
            get { return _prefixes + _source + _suffixes; }
        }

        private System.Text.RegularExpressions.Regex PatternRegex {
            get { return new System.Text.RegularExpressions.Regex(RegexString, _modifiers); }
        }

        #endregion Private Properties

        #region Constructors

        #endregion Constructors

        #region Public Methods

        #region Helpers

        internal string Sanitize(string value) {
            if (string.IsNullOrEmpty(value)) {
                throw new ArgumentNullException("value");
            }

            return System.Text.RegularExpressions.Regex.Escape(value);
        }

        internal bool Test(string toTest) {
            return IsMatch(toTest);
        }

        internal bool IsMatch(string toTest) {
            return PatternRegex.IsMatch(toTest);
        }

        internal System.Text.RegularExpressions.Regex ToRegex() {
            return PatternRegex;
        }

        public override string ToString() {
            return PatternRegex.ToString();
        }

        #endregion Helpers

        #region Expression Modifiers

        internal VerbalExpressions Add(string value) {
            if (ReferenceEquals(value, null)) {
                throw new ArgumentNullException("value");
            }

            return Add(value, true);
        }

        internal VerbalExpressions Add(CommonRegex commonRegex) {
            return Add(commonRegex.Name, false);
        }

        internal VerbalExpressions Add(string value, bool sanitize = true) {
            if (value == null)
                throw new ArgumentNullException("value must be provided");

            value = sanitize ? Sanitize(value) : value;
            _source += value;
            return this;
        }

        internal VerbalExpressions StartOfLine(bool enable = true) {
            _prefixes = enable ? "^" : string.Empty;
            return this;
        }

        internal VerbalExpressions EndOfLine(bool enable = true) {
            _suffixes = enable ? "$" : string.Empty;
            return this;
        }

        internal VerbalExpressions Then(string value, bool sanitize = true) {
            string sanitizedValue = sanitize ? Sanitize(value) : value;
            value = string.Format("({0})", sanitizedValue);
            return Add(value, false);
        }

        internal VerbalExpressions Then(CommonRegex commonRegex) {
            return Then(commonRegex.Name, false);
        }

        internal VerbalExpressions Find(string value) {
            return Then(value);
        }

        internal VerbalExpressions Maybe(string value, bool sanitize = true) {
            value = sanitize ? Sanitize(value) : value;
            value = string.Format("({0})?", value);
            return Add(value, false);
        }

        internal VerbalExpressions Maybe(CommonRegex commonRegex) {
            return Maybe(commonRegex.Name, false);
        }

        internal VerbalExpressions Anything() {
            return Add("(.*)", false);
        }

        internal VerbalExpressions AnythingBut(string value, bool sanitize = true) {
            value = sanitize ? Sanitize(value) : value;
            value = string.Format("([^{0}]*)", value);
            return Add(value, false);
        }

        internal VerbalExpressions Something() {
            return Add("(.+)", false);
        }

        internal VerbalExpressions SomethingBut(string value, bool sanitize = true) {
            value = sanitize ? Sanitize(value) : value;
            value = string.Format("([^" + value + "]+)");
            return Add(value, false);
        }

        internal VerbalExpressions Replace(string value) {
            string whereToReplace = PatternRegex.ToString();

            if (whereToReplace.Length != 0) {
                _source.Replace(whereToReplace, value);
            }

            return this;
        }

        internal VerbalExpressions LineBreak() {
            return Add(@"(\n|(\r\n))", false);
        }

        internal VerbalExpressions Br() {
            return LineBreak();
        }

        internal VerbalExpressions Tab() {
            return Add(@"\t");
        }

        internal VerbalExpressions Word() {
            return Add(@"\w+", false);
        }

        internal VerbalExpressions AnyOf(string value, bool sanitize = true) {
            if (string.IsNullOrEmpty(value)) {
                throw new ArgumentNullException("value");
            }

            value = sanitize ? Sanitize(value) : value;
            value = string.Format("[{0}]", Sanitize(value));
            return Add(value, false);
        }

        internal VerbalExpressions Any(string value) {
            return AnyOf(value);
        }

        internal VerbalExpressions Range(params object[] arguments) {
            if (ReferenceEquals(arguments, null)) {
                throw new ArgumentNullException("arguments");
            }

            if (arguments.Length == 1) {
                throw new ArgumentOutOfRangeException("arguments");
            }

            string[] sanitizedStrings = arguments.Select(argument => {
                if (ReferenceEquals(argument, null)) {
                    return string.Empty;
                }

                string casted = argument.ToString();
                if (string.IsNullOrEmpty(casted)) {
                    return string.Empty;
                }
                return Sanitize(casted);
            })
                .Where(sanitizedString =>
                    !string.IsNullOrEmpty(sanitizedString))
                .OrderBy(s => s)
                .ToArray();

            if (sanitizedStrings.Length > 3) {
                throw new ArgumentOutOfRangeException("arguments");
            }

            if (!sanitizedStrings.Any()) {
                return this;
            }

            bool hasOddNumberOfParams = (sanitizedStrings.Length%2) > 0;

            var sb = new StringBuilder("[");
            for (int _from = 0; _from < sanitizedStrings.Length; _from += 2) {
                int _to = _from + 1;
                if (sanitizedStrings.Length <= _to) {
                    break;
                }
                sb.AppendFormat("{0}-{1}", sanitizedStrings[_from], sanitizedStrings[_to]);
            }
            sb.Append("]");

            if (hasOddNumberOfParams) {
                sb.AppendFormat("|{0}", sanitizedStrings.Last());
            }

            return Add(sb.ToString(), false);
        }

        internal VerbalExpressions Multiple(string value, bool sanitize = true) {
            if (string.IsNullOrEmpty(value)) {
                throw new ArgumentNullException("value");
            }

            value = sanitize ? Sanitize(value) : value;
            value = string.Format(@"({0})+", value);

            return Add(value, false);
        }

        internal VerbalExpressions Or(CommonRegex commonRegex) {
            return Or(commonRegex.Name, false);
        }

        internal VerbalExpressions Or(string value, bool sanitize = true) {
            _prefixes += "(";
            _suffixes = ")" + _suffixes;

            _source += ")|(";

            return Add(value, sanitize);
        }

        internal VerbalExpressions BeginCapture() {
            return Add("(", false);
        }

        internal VerbalExpressions EndCapture() {
            return Add(")", false);
        }

        internal VerbalExpressions RepeatPrevious(int n) {
            return Add("{" + n + "}", false);
        }

        internal VerbalExpressions RepeatPrevious(int n, int m) {
            return Add("{" + n + "," + m + "}", false);
        }

        #endregion Expression Modifiers

        #region Expression Options Modifiers

        internal VerbalExpressions AddModifier(char modifier) {
            switch (modifier) {
                case 'i':
                    _modifiers |= RegexOptions.IgnoreCase;
                    break;
                case 'x':
                    _modifiers |= RegexOptions.IgnorePatternWhitespace;
                    break;
                case 'm':
                    _modifiers |= RegexOptions.Multiline;
                    break;
                case 's':
                    _modifiers |= RegexOptions.Singleline;
                    break;
            }

            return this;
        }

        internal VerbalExpressions RemoveModifier(char modifier) {
            switch (modifier) {
                case 'i':
                    _modifiers &= ~RegexOptions.IgnoreCase;
                    break;
                case 'x':
                    _modifiers &= ~RegexOptions.IgnorePatternWhitespace;
                    break;
                case 'm':
                    _modifiers &= ~RegexOptions.Multiline;
                    break;
                case 's':
                    _modifiers &= ~RegexOptions.Singleline;
                    break;
            }

            return this;
        }

        internal VerbalExpressions WithAnyCase(bool enable = true) {
            if (enable) {
                AddModifier('i');
            }
            else {
                RemoveModifier('i');
            }
            return this;
        }

        internal VerbalExpressions UseOneLineSearchOption(bool enable) {
            if (enable) {
                RemoveModifier('m');
            }
            else {
                AddModifier('m');
            }

            return this;
        }

        internal VerbalExpressions WithOptions(RegexOptions options) {
            _modifiers = options;
            return this;
        }

        #endregion Expression Options Modifiers

        #endregion Public Methods
    }
}