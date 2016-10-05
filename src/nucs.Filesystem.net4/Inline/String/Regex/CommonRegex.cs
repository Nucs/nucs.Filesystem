namespace nucs.SystemCore.String.Regex {
    /// <summary>
    ///     This class is used to fake an enum. You'll be able to use it as an enum.
    ///     Note: type save enum, found on stackoverflow: http://stackoverflow.com/a/424414/603309
    /// </summary>
    internal sealed class CommonRegex {
        #region Private Members

        private readonly System.String name;
        private readonly int value;

        #endregion Private Members

        #region Public Properties

        internal string Name {
            get { return name; }
        }

        internal int Value {
            get { return value; }
        }

        #endregion Public Properties

        #region Statics

        internal static readonly CommonRegex Url = new CommonRegex(1,
            @"((([A-Za-z]{3,9}:(?:\/\/)?)(?:[^-;:&=\+\$,\w]+@)?[A-Za-z0-9.-]+(:[0-9]+)?|(?:www.|[^-;:&=\+\$,\w]+@)[A-Za-z0-9.-]+)((?:\/[\+~%\/.\w_]*)?\??(?:[-\+=&;%@.\w-_]*)#?‌​(?:[\w]*))?)");

        internal static readonly CommonRegex Email = new CommonRegex(2, @"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}");

        #endregion Statics

        #region Constructors

        static CommonRegex() {
        }

        private CommonRegex(int value, System.String name) {
            this.name = name;
            this.value = value;
        }

        #endregion Constructors
    }
}