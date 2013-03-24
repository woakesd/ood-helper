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
    [DataContract]
    public partial class Boats : ServiceEntity
    {
        [DataMember]
        public string boatname { get; set; }
        [DataMember]
        public string boatclass { get; set; }
        [DataMember]
        public string sailno { get; set; }
        [DataMember]
        public bool? dngy { get; set; }
        [DataMember]
        public string h { get; set; }
        [DataMember]
        public int? bid { get; set; }
        [DataMember]
        public decimal? distance { get; set; }
        [DataMember]
        public string crewname { get; set; }
        [DataMember]
        public decimal? ohp { get; set; }
        [DataMember]
        public string ohstat { get; set; }
        [DataMember]
        public decimal? chp { get; set; }
        [DataMember]
        public decimal? rhp { get; set; }
        [DataMember]
        public decimal? ihp { get; set; }
        [DataMember]
        public decimal? csf { get; set; }
        [DataMember]
        public string eng { get; set; }
        [DataMember]
        public string kl { get; set; }
        [DataMember]
        public string deviations { get; set; }
        [DataMember]
        public string subscriptn { get; set; }
        [DataMember]
        public string p { get; set; }
        [DataMember]
        public bool? s { get; set; }
        [DataMember]
        public string boatmemo { get; set; }
        [DataMember]
        public decimal? id { get; set; }
        [DataMember]
        public decimal? beaten { get; set; }
        [DataMember]
        public string berth { get; set; }
        [DataMember]
        public bool? hired { get; set; }
        [DataMember]
        public string check { get; set; }
        [DataMember]
        public string uid { get; set; }
        [DataMember]
        public bool? delete { get; set; }
    }
}
