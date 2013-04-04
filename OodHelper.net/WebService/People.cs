using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OodHelper.WebService
{
    public partial class People : ServiceEntity
    {
        static public People[] Get(string Filter, int Page = 1)
        {
            HttpClient _client = GetClient();

            Uri _uri = new Uri(string.Format("{0}/people/filter:{1}/page:{2}", BaseURL, Filter, Page));

            Task<Stream> _streamTask = _client.GetStreamAsync(_uri);
            while (!_streamTask.IsCompleted)
                _streamTask.Wait(10);

            return ReadEntity<People[]>(_streamTask.Result);
        }

    }
}
