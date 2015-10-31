using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FakeHttp
{
    /// <summary>
    /// A custom <see cref="JsonConverter"/> for <see cref="Version"/> that only writes the <see cref="Version.Major"/>
    /// and <see cref="Version.Minor"/> values. HTTP specifies only a two field version. <see cref="Version"/> defaults 
    /// build, revision etc to -1 when not set, and these values break deserialization.
    /// </summary>
    public sealed class VersionConverter : JsonConverter
    {
        /// <summary>
        /// <see cref="JsonConverter.WriteJson(JsonWriter, object, JsonSerializer)"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var version = value as Version;
            if (version != null)
            {
                var j = new JObject();
                j.Add("Major", JToken.FromObject(version.Major));
                j.Add("Minor", JToken.FromObject(version.Minor));
                j.WriteTo(writer);
            }
            else
            {
                writer.WriteNull();
            }
        }

        /// <summary>
        /// not implemented
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// We only need to cusotmize writing, not reading as the two field version will desrialize just fine with default behavior
        /// </summary>
        public override bool CanRead
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// <see cref="JsonConverter.CanConvert(Type)"/>
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType)
        {
            return typeof(Version) == objectType;
        }
    }
}
