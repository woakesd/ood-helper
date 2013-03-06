using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace OodHelper.WebService
{
    [DataContract]
    public class People
    {
        static People()
        {
            BaseURL = Properties.Settings.Default.ResultsWebServiceBaseURL;
            BaseUsername = Properties.Settings.Default.ResultsWebServiceBaseUsername;
            BasePassword = Properties.Settings.Default.ResultsWebServiceBasePassword;
        }

        static private HttpClient GetClient()
        {
            WebRequestHandler _handler = new WebRequestHandler();
            _handler.Credentials = new System.Net.NetworkCredential(BaseUsername, BasePassword);
            _handler.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(ValidateServerCertificate);

            return new HttpClient(_handler, true);
        }

        protected static string BaseURL;
        protected static string BaseUsername;
        protected static string BasePassword;

        static public People GetPerson(int id)
        {
            HttpClient _client = GetClient();

            Uri _uri = new Uri(string.Format("{0}/people/{1}", BaseURL, id));

            Task<Stream> _streamTask = _client.GetStreamAsync(_uri);
            while (!_streamTask.IsCompleted)
                _streamTask.Wait(10);

            return ReadPeople(_streamTask.Result);
        }

        private static People ReadPeople(Stream _stream)
        {
            DataContractJsonSerializer _serial = new DataContractJsonSerializer(typeof(People));
            MemoryStream _ms = _stream as MemoryStream;
            if (_ms != null)
            {
                string res = Encoding.UTF8.GetString(_ms.ToArray());
            }
            return _serial.ReadObject(_stream) as People;
        }

        public People UpdatePeople()
        {
            HttpClient _client = GetClient();

            Uri _uri = new Uri(string.Format("{0}/people/{1}", BaseURL, id));

            DataContractJsonSerializer _serial = new DataContractJsonSerializer(typeof(People));
            string _encoded = string.Empty;
            using (MemoryStream _ms = new MemoryStream())
            {
                _serial.WriteObject(_ms, this);
                _encoded = Encoding.UTF8.GetString(_ms.ToArray());
            }
            HttpContent _content = new StringContent(_encoded, Encoding.UTF8, "application/json");
            Task<HttpResponseMessage> _streamTask = _client.PutAsync(_uri, _content);
            while (!_streamTask.IsCompleted)
                _streamTask.Wait(10);

            Task<Stream> _jsonStreamTask = _streamTask.Result.Content.ReadAsStreamAsync();
            while (!_jsonStreamTask.IsCompleted)
                _jsonStreamTask.Wait(10);

            return ReadPeople(_jsonStreamTask.Result);
        }

        public People InsertPeople()
        {
            HttpClient _client = GetClient();

            Uri _uri = new Uri(string.Format("{0}/people", BaseURL));

            DataContractJsonSerializer _serial = new DataContractJsonSerializer(typeof(People));
            string _encoded = string.Empty;
            using (MemoryStream _ms = new MemoryStream())
            {
                _serial.WriteObject(_ms, this);
                _encoded = Encoding.UTF8.GetString(_ms.ToArray());
            }
            HttpContent _content = new StringContent(_encoded, Encoding.UTF8, "application/json");
            Task<HttpResponseMessage> _streamTask = _client.PostAsync(_uri, _content);
            while (!_streamTask.IsCompleted)
                _streamTask.Wait(10);

            Task<Stream> _jsonStreamTask = _streamTask.Result.Content.ReadAsStreamAsync();
            while (!_jsonStreamTask.IsCompleted)
                _jsonStreamTask.Wait(10);

            return ReadPeople(_jsonStreamTask.Result);
        }

        public static void DeletePeople(int Id)
        {
            HttpClient _client = GetClient();

            Uri _uri = new Uri(string.Format("{0}/people/{1}", BaseURL, Id));

            Task<HttpResponseMessage> _deleteTask = _client.DeleteAsync(_uri);

            while (!_deleteTask.IsCompleted)
                _deleteTask.Wait(10);
        }

        public static bool ValidateServerCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            //
            // potentially this is weak. I'm using a self signed certificate on the web site so cant validate the chain...
            //
            return true;
        }



        [DataMember]
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
    }
}
