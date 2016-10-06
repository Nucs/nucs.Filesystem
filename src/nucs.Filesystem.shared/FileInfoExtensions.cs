using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace nucs.Filesystem {
    public static class FileInfoExtensions {
        /// <summary>
        ///     Handle cases when the file is about to be written to directory but it might already exist.
        ///     Windows usually adds (1).txt and then writes the file but in pure code it is not the case and needs to be done
        ///     manually.
        /// </summary>
        /// <param name="@in">File that is going to be saved.</param>
        /// <returns>Savable file name</returns>
        public static FileInfo HandlePossibleDuplicate(this FileInfo @in) {
            var cache = new FileInfo(@in.FullName); //duplicate
            var SavePath = @in.FullName; //duplicate
            for (var i = 1; File.Exists(SavePath); i++)
                SavePath = Path.Combine(cache.Directory.FullName, $"{cache.Name.Replace(cache.Extension, "")} ({i}){cache.Extension}");

            return new FileInfo(SavePath);
        }

        /// <summary>
        ///     Combines multiples string into a path.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="paths">A variable-length parameters list containing paths.</param>
        /// <returns>
        ///     The combined paths as a FileInfo. If one of the specified paths is a zero-length string, this method returns
        ///     the other path.
        /// </returns>
        /// <example>
        ///     <code>
        ///           using System;
        ///           using System.IO;
        ///           using Microsoft.VisualStudio.TestTools.UnitTesting;
        ///           using Z.ExtensionMethods;
        ///           
        ///           namespace ExtensionMethods.Examples
        ///           {
        ///               [TestClass]
        ///               public class System_IO_DirectoryInfo_PathCombineFile
        ///               {
        ///                   [TestMethod]
        ///                   public void PathCombine()
        ///                   {
        ///                       // Type
        ///                       var @this = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
        ///           
        ///                       string path1 = &quot;Fizz&quot;;
        ///                       string path2 = &quot;Buzz&quot;;
        ///           
        ///                       // Exemples
        ///                       FileInfo result = @this.PathCombineFile(path1, path2); // Combine path1 and path2 with the DirectoryInfo
        ///           
        ///                       // Unit Test
        ///                       var expected = new FileInfo(Path.Combine(@this.FullName, path1, path2));
        ///                       Assert.AreEqual(expected.FullName, result.FullName);
        ///                   }
        ///               }
        ///           }
        ///     </code>
        /// </example>
        public static FileInfo PathCombineFile(this DirectoryInfo @this, params string[] paths) {
            var list = paths.ToList();
            list.Insert(0, @this.FullName);
            return new FileInfo(list.PathCombine());
        }

        /// <summary>
        ///     An IEnumerable&lt;string&gt; extension method that combine all value to return a path.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The path.</returns>
        public static string PathCombine(this IEnumerable<string> @this) {
#if (NET35 || NET3 || NET2)
            var l = @this.ToList();
            var res = l.First();
            res = l.Skip(1).Aggregate(res, Path.Combine);
            return res;
#else
        return Path.Combine(@this.ToArray());
#endif
        }
    }
}