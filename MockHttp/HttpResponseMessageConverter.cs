using System;
using System.Net.Http;

using Newtonsoft.Json;

namespace MockHttp
{
    class HttpResponseMessageConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            // the request message and the content do not get serialized as part of the response
            return objectType == typeof(HttpRequestMessage) || objectType.IsSubclassOf(typeof(HttpContent));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, null);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // this will never be called because we are not deserializing to typed objects
            throw new NotImplementedException();
        }
    }
}
