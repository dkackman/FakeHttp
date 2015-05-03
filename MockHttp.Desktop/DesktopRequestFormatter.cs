using System;
using System.Text;

using System.Security.Cryptography;

namespace MockHttp.Desktop
{
    class DesktopRequestFormatter : RequestFormatter
    {
        public DesktopRequestFormatter()
            : this((name, value) => false) // by default do filter any qquery paramters
        {
        }

        public DesktopRequestFormatter(Func<string, string, bool> paramFilter)
            : base(paramFilter)
        {
        }

        public override string ToSha1Hash(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            using (var sha1 = new SHA1Managed())
            {
                byte[] textData = Encoding.UTF8.GetBytes(text);
                byte[] hash = sha1.ComputeHash(textData);

                return BitConverter.ToString(hash).Replace("-", string.Empty);
            }
        }
    }
}
