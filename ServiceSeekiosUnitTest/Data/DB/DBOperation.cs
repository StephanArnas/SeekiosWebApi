using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.DTO
{
    [DataContract]
    public class DBOperation
    {
        [DataMember]
        public int IdOperation { get; set; }
        [DataMember]
        public int? IdUser { get; set; }
        [DataMember]
        public int? IdSeekios { get; set; }
        [DataMember]
        public int? IdMode { get; set; }
        [DataMember]
        public int OperationType { get; set; }
        [DataMember]
        public int CreditAmount { get; set; }
        [DataMember]
        public DateTime? DateEnd { get; set; }
        [DataMember]
        public DateTime DateBegin { get; set; }
        [DataMember]
        public int? IdDevice { get; set; }
        [DataMember]
        public int Amount { get; set; }
        [DataMember]
        public bool IsOnSeekios { get; set; }

    }
}