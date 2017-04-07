using System;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;

namespace FakeHttp.Store
{
    sealed class StoreMessageFormatter : MessageFormatter
    {
        public StoreMessageFormatter()
            : this(new ResponseCallbacks())
        {
        }

        public StoreMessageFormatter(IResponseCallbacks callbacks)
            : base(callbacks)
        {
        }

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
