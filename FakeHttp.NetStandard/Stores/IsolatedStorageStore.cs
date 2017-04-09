using System;
using System.Collections.Generic;
using System.Text;

namespace FakeHttp.Stores
{
    public sealed class IsolatedStorageStore : ReadonlyResponseStore
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="storeFolder">root folder for storage</param>
        public IsolatedStorageStore(string storeFolder)
            : this(storeFolder, new ResponseCallbacks())
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="storeFolder">root folder for storage</param>
        /// <param name="callbacks">Object to manage responses at runtime</param>
        public IsolatedStorageStore(string storeFolder, IResponseCallbacks callbacks)
            : base(new ZipResources(storeFolder), callbacks)
        {
        }
    }
}
