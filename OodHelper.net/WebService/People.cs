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
        static public People[] Get(int Page = 1)
        {
            HttpClient _client = GetClient();

            Uri _uri = new Uri(string.Format("{0}/people/page/{1}", BaseURL, Page));

            Task<Stream> _streamTask = _client.GetStreamAsync(_uri);
            while (!_streamTask.IsCompleted)
                _streamTask.Wait(10);

            return ReadEntity<People[]>(_streamTask.Result);
        }

        static public People[] Get(string Filter, int Page = 1)
        {
            HttpClient _client = GetClient();

            Uri _uri = new Uri(string.Format("{0}/people/filter/{1}/page/{2}", BaseURL, Filter, Page));

            Task<Stream> _streamTask = _client.GetStreamAsync(_uri);
            while (!_streamTask.IsCompleted)
                _streamTask.Wait(10);

            return ReadEntity<People[]>(_streamTask.Result);
        }

        static public People GetByKey(int id)
        {
            HttpClient _client = GetClient();

            Uri _uri = new Uri(string.Format("{0}/people/{1}", BaseURL, id));

            Task<Stream> _streamTask = _client.GetStreamAsync(_uri);
            while (!_streamTask.IsCompleted)
                _streamTask.Wait(10);

            return ReadEntity<People>(_streamTask.Result);
        }

        public People Update()
        {
            HttpClient _client = GetClient();

            Uri _uri = new Uri(string.Format("{0}/people/{1}", BaseURL, id));

            string _encoded = WriteEntity<People>(this);

            HttpContent _content = new StringContent(_encoded, Encoding.UTF8, "application/json");
            Task<HttpResponseMessage> _streamTask = _client.PutAsync(_uri, _content);
            while (!_streamTask.IsCompleted)
                _streamTask.Wait(10);

            Task<Stream> _jsonStreamTask = _streamTask.Result.Content.ReadAsStreamAsync();
            while (!_jsonStreamTask.IsCompleted)
                _jsonStreamTask.Wait(10);

            return ReadEntity<People>(_jsonStreamTask.Result);
        }

        public People Insert()
        {
            HttpClient _client = GetClient();

            Uri _uri = new Uri(string.Format("{0}/people", BaseURL));

            string _encoded = WriteEntity<People>(this);

            HttpContent _content = new StringContent(_encoded, Encoding.UTF8, "application/json");
            Task<HttpResponseMessage> _streamTask = _client.PostAsync(_uri, _content);
            while (!_streamTask.IsCompleted)
                _streamTask.Wait(10);

            Task<Stream> _jsonStreamTask = _streamTask.Result.Content.ReadAsStreamAsync();
            while (!_jsonStreamTask.IsCompleted)
                _jsonStreamTask.Wait(10);

            return ReadEntity<People>(_jsonStreamTask.Result);
        }

        public static void Delete(int Id)
        {
            HttpClient _client = GetClient();

            Uri _uri = new Uri(string.Format("{0}/people/{1}", BaseURL, Id));

            Task<HttpResponseMessage> _deleteTask = _client.DeleteAsync(_uri);

            while (!_deleteTask.IsCompleted)
                _deleteTask.Wait(10);
        }
    }
}
