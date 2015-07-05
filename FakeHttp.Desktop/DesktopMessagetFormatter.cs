using System;
using System.Text;
using System.Security.Cryptography;

namespace FakeHttp.Desktop
{
    class DesktopMessagetFormatter : MessageFormatter
    {
        public DesktopMessagetFormatter()
            : this((name, value) => false) // by default do not filter any query parameters
        {
        }

        public DesktopMessagetFormatter(Func<string, string, bool> paramFilter)
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
