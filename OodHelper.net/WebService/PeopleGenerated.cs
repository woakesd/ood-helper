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
            return GetPeopleAsync(Page).Result;
        }

        static private async Task<People[]> GetPeopleAsync(int Page)
        {
            HttpClient _client = GetClient();
            Uri _uri = new Uri(string.Format("{0}/people/page/{1}", BaseURL, Page));

            Stream _stream = await _client.GetStreamAsync(_uri).ConfigureAwait(false);

            return ReadEntity<People[]>(_stream);
        }

        static public People GetByKey(int id)
        {
            return GetByKeyAsync(id).Result;
        }

        static private async Task<People> GetByKeyAsync(int id)
        {
            HttpClient _client = GetClient();

            Uri _uri = new Uri(string.Format("{0}/people/{1}", BaseURL, id));

            Stream _stream = await _client.GetStreamAsync(_uri).ConfigureAwait(false);

            return ReadEntity<People>(_stream);
        }

        public People Update()
        {
            return UpdateAsync().Result;
        }

        private async Task<People> UpdateAsync()
        {
            HttpClient _client = GetClient();

            Uri _uri = new Uri(string.Format("{0}/people/{1}", BaseURL, id));

            string _encoded = WriteEntity<People>(this);

            HttpContent _content = new StringContent(_encoded, Encoding.UTF8, "application/json");
            HttpResponseMessage _stream = await _client.PutAsync(_uri, _content).ConfigureAwait(false);

            Stream _jsonStream = await _stream.Content.ReadAsStreamAsync().ConfigureAwait(false);

            return ReadEntity<People>(_jsonStream);
        }

        public People Insert()
        {
            return InsertAsync().Result;
        }

        private async Task<People> InsertAsync()
        {
            HttpClient _client = GetClient();

            Uri _uri = new Uri(string.Format("{0}/people", BaseURL));

            string _encoded = WriteEntity<People>(this);

            HttpContent _content = new StringContent(_encoded, Encoding.UTF8, "application/json");

            HttpResponseMessage _streamTask = await _client.PostAsync(_uri, _content).ConfigureAwait(false);

            Stream _jsonStream = await _streamTask.Content.ReadAsStreamAsync().ConfigureAwait(false);

            return ReadEntity<People>(_jsonStream);
        }

        public void Delete()
        {
            DeleteAsync();
        }

        private async void DeleteAsync()
        {
            HttpClient _client = GetClient();

            Uri _uri = new Uri(string.Format("{0}/people/{1}", BaseURL, id));

            HttpResponseMessage _delete = await _client.DeleteAsync(_uri).ConfigureAwait(false);
        }
    }
}
