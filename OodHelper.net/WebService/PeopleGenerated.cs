using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace OodHelper.WebService
{
    [DataContract(Name="people")]
    public partial class People : ServiceEntity
    {
        [DataMember(Name="id")]
        public int id { get; set; }
        [DataMember]
        public int? sid { get; set; }
        [DataMember]
        public string firstname { get; set; }
        [DataMember]
        public string surname { get; set; }
        [DataMember]
        public string address1 { get; set; }
        [DataMember]
        public string address2 { get; set; }
        [DataMember]
        public string address3 { get; set; }
        [DataMember]
        public string address4 { get; set; }
        [DataMember]
        public string postcode { get; set; }
        [DataMember]
        public string hometel { get; set; }
        [DataMember]
        public string worktel { get; set; }
        [DataMember]
        public string mobile { get; set; }
        [DataMember]
        public string email { get; set; }
        [DataMember]
        public string club { get; set; }
        [DataMember]
        public string member { get; set; }
        [DataMember]
        public bool? cp { get; set; }
        [DataMember]
        public bool? s { get; set; }
        [DataMember]
        public string manmemo { get; set; }
        [DataMember]
        public string check { get; set; }
        [DataMember]
        public bool? novice { get; set; }
        [DataMember]
        public Guid? uid { get; set; }
        [DataMember]
        public bool? papernewsletter { get; set; }
        [DataMember]
        public bool? handbookexclude { get; set; }
        [DataMember]
        public bool? delete { get; set; }
        [DataMember]
        public bool? updated { get; set; }
        [DataMember]
        public string crewon { get; set; }
        [DataMember]
        public bool? old_member { get; set; }

        static public People[] Get(int Page = 1)
        {
            HttpClient _client = GetClient();

            Uri _uri = new Uri(string.Format("{0}/people/page/{1}", BaseURL, Page));

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
