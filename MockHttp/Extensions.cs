using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace MockHttp
{
    static class Extensions
    {
        public static string ToFilePath(this Uri uri)
        {
            return Path.Combine(uri.Host, uri.LocalPath.TrimStart('/').Replace('/', '\\'));
        }
    }
}
