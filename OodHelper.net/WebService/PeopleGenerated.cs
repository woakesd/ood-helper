using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace OodHelper.WebService
{
    [DataContract]
    public partial class People : ServiceEntity
    {

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
