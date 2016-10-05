using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using nucs.SystemCore.String;

namespace nucs.SystemCore {
    internal static class UriExtensions {

        internal static string CssValidHost(this Uri uri) {
            return uri.Host.Replace("www.", "").Replace('.', '-'); 
        }

    }
}
