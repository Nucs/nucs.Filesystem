using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using nucs.Collections.Extensions;
using nucs.SystemCore;
using nucs.SystemCore.Boolean;
using nucs.SystemCore.String;

namespace nucs.Filesystem.Distribution {
    public static class FileNameGenerator {
        private static readonly string[] datastorageextensions = {".bin", ".mui", ".ini", ".cfg", ".dll", ".dat", ".iso", ".vcd", ".cue", ".dmp", ".cab"};
        private static readonly string[] winfilenames = {"unins000.dat", "en-GB.bin", "he-IL.bin", "en-GB.dat", "he-IL.dat", "cgi.bin", ".thumbnail"};

        /// <summary>
        ///     Filenames for empty directories
        /// </summary>
        private static readonly string[] emptyfilenames = {"LICENSE.txt", "en-GB.bin", "en-US", "he-IL.bin", "en-GB.dat", "he-IL.dat", "cgi.bin"};

        private static readonly string[] blacklisted_extension = {
            ".exe", ".EXE",
            ".doc", ".docx", ".log", ".msg", ".odt", ".pages", ".rtf",
            ".tex", ".txt", ".wpd", ".wps", ".csv", ".dat", ".gbr", ".ged",
            ".key", ".keychain", ".pps", ".ppt", ".pptx", ".sdf", ".tar",
            ".tax2012", ".tax2014", ".vcf", ".xml", ".aif", ".iff", ".m3u",
            ".m4a", ".mid", ".mp3", ".mpa", ".ra", ".wav", ".wma", ".3g2",
            ".3gp", ".asf", ".asx", ".avi", ".flv", ".m4v", ".mov", ".mp4",
            ".mpg", ".rm", ".srt", ".swf", ".vob", ".wmv", ".3dm", ".3ds",
            ".max", ".obj", ".bmp", ".dds", ".gif", ".jpg", ".png", ".psd",
            ".pspimage", ".tga", ".thm", ".tif", ".tiff", ".yuv", ".ai",
            ".eps", ".ps", ".svg", ".indd", ".pct", ".pdf", ".xlr", ".xls",
            ".xlsx", ".asp", ".aspx", ".cer", ".cfm", ".csr", ".css", ".htm",
            ".html", ".js", ".jsp", ".php", ".rss", ".xhtml", ".crx", ".plugin",
            ".c", ".class", ".cpp", ".cs", ".dtd", ".fla", ".h", ".java",
            ".lua", ".m", ".pl", ".py", ".sh", ".sln", ".swift", ".vcxproj",
            ".xcodeproj", ".crdownload", ".ics", ".msi", ".part", ".vsx",
            ".lnk"
        };

        private static readonly Random rand = new Random();

        /// <summary>
        ///       Generates a random logical filename based on the given directory
        /// </summary>
        /// <param name="base">The directory to sample for generation</param>
        public static FileInfo Generate(DirectoryInfo @base) {
            if (@base.Exists == false) { //for nonexisting
                @base.EnsureDirectoryExists();
                return Generate(@base); //create and retry
            }

            DirectoryInfo[] dirs;
            FileInfo[] files;

            try {
                dirs = @base.GetDirectories();
                files = @base.GetFiles();
            } catch (UnauthorizedAccessException) {
                return null;
            }

            if (@base.Root.FullName.Equals(@base.FullName)) { //is root dir (c:/)
                var fn = winfilenames.TakeRandomNonExisting(@base);
                return new FileInfo(Path.Combine(@base.FullName, fn));
            }

            if (files.Length == 0 | dirs.Length == 0 && files.Length == 0) { //for empty directory
                //take the directory name and parse it into a file
                var realbase = @base;
                _invalidname:
                var fn = @base.Name.ToLower().RemoveNumber().Trim('.', ',').Trim();
                var spaces = fn.Count(c => c == ' ');
                if (spaces >= 3) {
                    var splet = fn.Split(' ');
                    fn = rand.Chance(50, splet.Skip(1), splet.Reverse().Skip(1).Reverse())
                        .StringJoin(rand.Chance(50, "-", ""));
                    fn = fn.DeleteDuplicateCharsMultiple("-");
                } else {
                    fn = rand.Chance(50, () => fn.Replace(" ", "-"), () => fn);
                }
                if (string.IsNullOrEmpty(fn)) {
                    @base = @base.Parent;
                    goto _invalidname;
                }


                return new FileInfo(Path.Combine(realbase.FullName, fn + rand.CoinToss(rand.CoinToss("-") + RandomVersion()) + datastorageextensions.TakeRandom()));
            }

            //file with number in its end
            if (files.Where(f => f.HasExtension() && blacklisted_extension.Any(ext => ext.Equals(f.Extension)) == false).Where(f => !string.IsNullOrEmpty(f.GetFileNameWithoutExtension())).Any(f => char.IsDigit(f.GetFileNameWithoutExtension().Reverse().First()))) {
                var potent = files.Where(f => f.HasExtension() && blacklisted_extension.Any(ext => ext.Equals(f.Extension)) == false).Where(f => !string.IsNullOrEmpty(f.GetFileNameWithoutExtension()) && char.IsDigit(f.GetFileNameWithoutExtension().Reverse().First())).OrderBy(f => f.LastAccessTimeUtc).FirstOrDefault(); //get least touched file with number at end.
                if (potent != null) {
                    var n = potent.GetFileNameWithoutExtension()
                        .Reverse()
                        .TakeWhile(char.IsDigit)
                        .ToArray()
                        .Take(10)
                        .Reverse()
                        .StringJoin("")
                        .ToDecimal();
                    bool tried = false;
                    var filewonum = potent.GetFileNameWithoutExtension().Replace(n.ToString(), "");
                    _retry:
                    if (tried | n < 1 | new Random().CoinToss())
                        n++;
                    else
                        n--;
                    tried = true;
                    var t = new FileInfo(Path.Combine(@base.FullName, filewonum + n + potent.Extension));
                    if (t.Exists)
                        goto _retry;
                    return t;
                }
            }


            //based on file with all lowcase letters and extension
            var potential = files.Where(f => f.GetFileNameWithoutExtension().All(c => char.IsLetter(c) && char.IsLower(c))).OrderBy(f => f.Extension == ".exe").ThenByDescending(f => f.LastAccessTimeUtc).FirstOrDefault();
            if (potential != null) {
                var favext = FavoriteExtension(files, blacklisted_extension.Concat(potential.Extension.ToEnumerable()).ToArray());
                return potential.ChangeExtension(favext);
            } else {
                potential = files.OrderBy(f => f.Extension == ".exe").ThenByDescending(f => f.LastAccessTimeUtc).FirstOrDefault();
                var favext = FavoriteExtension(files, blacklisted_extension.Concat(potential.Extension.ToEnumerable()).ToArray());
                return potential.ChangeExtension(favext);
            }
        }

        private static string FavoriteExtension(DirectoryInfo dir, params string[] except) {
            return FavoriteExtension(dir.GetFiles(), except);
        }

        private static string FavoriteExtension(IEnumerable<FileInfo> filessample, params string[] except) {
            if (except == null)
                except = new string[0];
            var files = filessample.ToArray();
            if (files.Length <= 1)
                goto _rand; //empty or one file

            //select where the extension occurs the most
            var logical = files.Where(f => except.Any(ext => ext == f.Extension) == false)
                .Select(f => f.Extension)
                .GroupBy(ext => ext)
                .OrderByDescending(g => g.Count()).Select(g => g.Key)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(logical) == false)
                return logical;

            _rand:
            return datastorageextensions.Except(except).TakeRandom();
        }

        private static T TakeRandom<T>(this IEnumerable<T> list) {
            return TakeRandom(list.ToList());
        }

        private static T TakeRandom<T>(this IList<T> list) {
            return list[rand.Next(0, list.Count - 1)];
        }

        /// <summary>
        ///     Gets random objects that doesnt exist
        /// </summary>
        /// <param name="filenames"></param>
        /// <param name="base"></param>
        /// <returns></returns>
        private static string TakeRandomNonExisting(this IList<string> filenames, DirectoryInfo @base) {
            var nonexisting =
                filenames.Select(f => new FileInfo(Path.Combine(@base.FullName, f)))
                    .Where(fn => fn.Exists == false)
                    .ToList();

            return nonexisting.TakeRandom().Name;
        }

        public static string RandomVersion() {
            var dots = rand.Next(1, 4);

            switch (dots) {
                case 3:
                    return rand.Next(0, 3) + "." + rand.Next(0, 8) + "." + rand.Next(0, 5) + "." + rand.Next(0, 10);
                case 2:
                    return rand.Next(0, 3) + "." + rand.Next(0, 5) + "." + rand.Next(0, 10);
                case 1:
                    return rand.Next(0, 3) + rand.Chance(70, ".") + rand.Next(0, 10);
                default:
                    throw new Exception("CHAOS IS IN THE HOUSE!");
            }
        }

        #region Inline
        private static string RemoveNumber(this string @this) {
            return new string(@this.ToCharArray().Where(x => !Char.IsNumber(x)).ToArray());
        }

        internal static DirectoryInfo EnsureDirectoryExists(this DirectoryInfo @this)
        {
            if (Directory.Exists(@this.FullName) == false)
                return Directory.CreateDirectory(@this.FullName);
            return null;
        }
        internal static String GetFileNameWithoutExtension(this FileInfo @this)
        {
            return Path.GetFileNameWithoutExtension(@this.FullName);
        }
        private static FileInfo ChangeExtension(this FileInfo @this, String extension)
        {
            return new FileInfo(Path.ChangeExtension(@this.FullName, extension));
        }
        private static string StringJoin<T>(this IEnumerable<T> @this, string separator)
        {
            return string.Join(separator, @this.Select(j => j.ToString()).ToArray());
        }
        private static System.Boolean HasExtension(this FileInfo @this)
        {
            return Path.HasExtension(@this.FullName);
        }
        #endregion
    }
}