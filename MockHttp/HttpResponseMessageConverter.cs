using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;

using Newtonsoft.Json;

namespace MockHttp
{
    public sealed class HttpResponseMessageConverter : JsonConverter
    {
        private static readonly Type[] _dontSerializeTypes = new Type[] { typeof(HttpRequestMessage), typeof(StringContent), typeof(StreamContent),
                                                                            typeof(ByteArrayContent), typeof(MultipartContent), typeof(HttpContent)};

        public override bool CanConvert(Type objectType)
        {
            // the request message and the content do not get serialized as part of the response
            return _dontSerializeTypes.Contains(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // write null for those type we don't want serialized
            serializer.Serialize(writer, null);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // this will never be called because we are not deserializing to typed objects
            throw new NotImplementedException();
        }
    }
}
