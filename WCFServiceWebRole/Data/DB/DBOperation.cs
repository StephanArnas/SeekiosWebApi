using System;
using System.Runtime.Serialization;

namespace WCFServiceWebRole.Data.DTO
{
    [DataContract]
    public class DBOperation
    {
        [DataMember]
        public int IdO { get; set; }
        [DataMember]
        public int? IdU { get; set; }
        [DataMember]
        public int? IdS { get; set; }
        [DataMember]
        public int? IdM { get; set; }
        [DataMember]
        public int Op { get; set; }
        [DataMember]
        public int CA { get; set; }
        [DataMember]
        public DateTime? DE { get; set; }
        [DataMember]
        public DateTime DB { get; set; }
        [DataMember]
        public int? IdD { get; set; }
        [DataMember]
        public int Am { get; set; }
        [DataMember]
        public bool IsOnS { get; set; }

    }
}