using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace nucs.Filesystem.Zip {
    internal static class ResourceHelper {
        
        /// <summary>
        ///     Finds the resource in all assemblies that contains the given string 'name'.
        /// </summary>
        internal static Stream GetResource(string name) {
            var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(asam => asam.GetManifestResourceNames().Any(mrn => mrn.Contains(name)));
            var target = asm.GetManifestResourceNames().FirstOrDefault(mrn => mrn.Contains(name));
            if (target==null) throw new FileNotFoundException($"Could not find a resource that contains the name '{name}'");
            return asm.GetManifestResourceStream(target);
        }
         
    }
}