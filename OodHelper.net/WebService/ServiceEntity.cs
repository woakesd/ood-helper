using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace OodHelper.WebService
{
    [DataContract]
    public class ServiceEntity
    {
        static ServiceEntity()
        {
            BaseURL = Settings.ResultsWebServiceBaseURL;
            BaseUsername = Settings.ResultsWebServiceBaseUsername;
            BasePassword = Settings.ResultsWebServiceBasePassword;
        }

        static private HttpClient Client = null;

        static protected HttpClient GetClient()
        {
            if (Client == null)
            {
                WebRequestHandler _handler = new WebRequestHandler();
                _handler.Credentials = new System.Net.NetworkCredential(BaseUsername, BasePassword);
                _handler.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(ValidateServerCertificate);
                _handler.PreAuthenticate = true;

                Client = new HttpClient(_handler, true);
            }
            return Client;
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

        protected static TEntity ReadEntity<TEntity>(Stream JsonStream)
        {
            DataContractJsonSerializer _serial = new DataContractJsonSerializer(typeof(TEntity));
            MemoryStream _ms = JsonStream as MemoryStream;
            if (_ms != null)
            {
                string res = Encoding.UTF8.GetString(_ms.ToArray());
            }
            try
            {
                TEntity _return = (TEntity)_serial.ReadObject(JsonStream);
                return _return;
            }
            catch (Exception ex)
            {
            }
            return default(TEntity);
        }

        protected static string WriteEntity<TEntity>(TEntity Entity)
        {
            DataContractJsonSerializer _serial = new DataContractJsonSerializer(typeof(TEntity));
            string _encoded = string.Empty;
            using (MemoryStream _ms = new MemoryStream())
            {
                _serial.WriteObject(_ms, Entity);
                _encoded = Encoding.UTF8.GetString(_ms.ToArray());
            }
            return _encoded;
        }

        protected static string BaseURL;
        protected static string BaseUsername;
        protected static string BasePassword;
    }
}
