using System.Text;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;

namespace MockHttp.Store
{
    class StoreRequestFormatter : RequestFormatter
    {
        public override string ToSha1Hash(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            var buffer = CryptographicBuffer.ConvertStringToBinary(text, BinaryStringEncoding.Utf8);

            var hashAlgorithmProvider = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);
            var bufferHash = hashAlgorithmProvider.HashData(buffer);
            
            return CryptographicBuffer.EncodeToHexString(bufferHash);
        }
    }
}
