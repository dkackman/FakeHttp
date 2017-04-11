using System;
using System.Threading.Tasks;
using System.Net.Http;

using Newtonsoft.Json;

namespace FakeHttp
{
    /// <summary>
    /// Class that can store and retrieve response messages using <see cref="System.IO.File"/>
    /// </summary>
    public sealed class ResponseStore : ReadOnlyResponseStore, IResponseStore
    {
        private readonly IResources _resources;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="storeFolder">root folder for storage</param>
        /// <param name="captureFolder">folder to store captued response messages</param>
        /// <param name="callbacks">Object to manage responses at runtime</param>
        public ResponseStore(IResources resources)
            : this(resources, new ResponseCallbacks())
        {
        }

        public ResponseStore(IResources resources, IResponseCallbacks callbacks)
            : base(resources, callbacks)
        {
            _resources = resources ?? throw new ArgumentNullException("resources");
        }

        /// <summary>
        /// Stores a response message for later retrieval
        /// </summary>
        /// <param name="response">The response message to store</param>
        /// <returns>Task</returns>
        public async Task StoreResponse(HttpResponseMessage response)
        {
            if (response == null) throw new ArgumentNullException("response");

            var folderPath = Adapter.Formatter.ToResourcePath(response.RequestMessage.RequestUri);
            var fileName = Adapter.Formatter.ToName(response.RequestMessage, Adapter.RepsonseCallbacks.FilterParameter);

            // this is the object that is serialized (response, normalized request query and pointer to the content file)
            var info = Adapter.Formatter.PackageResponse(response, Adapter.RepsonseCallbacks.FilterParameter);

            // get the content stream loaded and serialize it
            await response.Content.LoadIntoBufferAsync();
            var content = await Adapter.RepsonseCallbacks.Serializing(response);
            _resources.Store(folderPath, info.ContentFileName, content);

            // now serialize the response object and its meta-data
            var json = JsonConvert.SerializeObject(info, Formatting.Indented, new VersionConverter());
            _resources.Store(folderPath, fileName + ".response.json", json);
        }
    }
}
