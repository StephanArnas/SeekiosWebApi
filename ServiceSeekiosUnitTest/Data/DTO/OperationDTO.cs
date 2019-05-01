using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.DTO
{
    public class OperationDTO
    {
        public int IdOperation { get; set; }
        public int? IdUser { get; set; }
        public int? IdSeekios { get; set; }
        public int? IdMode { get; set; }
        public int OperationType { get; set; }
        public int CreditAmount { get; set; }
        public DateTime? DateEnd { get; set; }
        public DateTime DateBegin { get; set; }
        public int? IdDevice { get; set; }
        public int Amount { get; set; }
        public bool IsOnSeekios { get; set; }
    }
}