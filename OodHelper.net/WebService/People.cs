using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace OodHelper.WebService
{
    [DataContract]
    public class People
    {
        public People(int id)
        {
            GetPerson(id);
        }

        public string JSON { get; set; }
        public async void GetPerson(int id)
        {
            HttpClientHandler _handler = new HttpClientHandler();
            _handler.Credentials = new System.Net.NetworkCredential("pedb", "jugeit");

            HttpClient _client = new HttpClient(_handler, true);

            Uri _uri = new Uri(string.Format("http://peycrace.info/results/people/id/{0}", id));

            DataContractJsonSerializer _serial = new DataContractJsonSerializer(typeof(People[]));
            string JSON = await _client.GetStringAsync(_uri);
            object x = _serial.ReadObject(GenerateStreamFromString(JSON));
        }

        private Stream GenerateStreamFromString(string value)
        {
            return new MemoryStream(UTF8Encoding.Default.GetBytes(value ?? ""));
        }

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
        public int? cp { get; set; }
        [DataMember]
        public int? s { get; set; }
        [DataMember]
        public int id { get; set; }
        [DataMember]
        public string manmemo { get; set; }
        [DataMember]
        public int? sid { get; set; }
        [DataMember]
        public string check { get; set; }
        [DataMember]
        public int? novice { get; set; }
        [DataMember]
        public Guid? uid { get; set; }
        [DataMember]
        public int? papernewsletter { get; set; }
        [DataMember]
        public int? handbookexclude { get; set; }
        [DataMember]
        public int? delete { get; set; }
        [DataMember]
        public int? updated { get; set; }
        [DataMember]
        public string crewon { get; set; }
        [DataMember]
        public int? old_member { get; set; }
    }
}
