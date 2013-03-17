using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace OodHelper.WebService
{
    [DataContract]
    class EntityCount : ServiceEntity
    {
        [DataMember]
        public int count { get; set; }

        public static EntityCount GetCount(string Entity, string Filter = null)
        {
            HttpClient _client = GetClient();

            Uri _uri;
            if (Filter == null)
                _uri = new Uri(string.Format("{0}/{1}/count", BaseURL, Entity));
            else
                _uri = new Uri(string.Format("{0}/{1}/count/filter/{2}", BaseURL, Entity, Filter));

            Task<Stream> _streamTask = _client.GetStreamAsync(_uri);
            while (!_streamTask.IsCompleted)
                _streamTask.Wait(10);

            return ReadEntity<EntityCount>(_streamTask.Result);
        }
    }
}
