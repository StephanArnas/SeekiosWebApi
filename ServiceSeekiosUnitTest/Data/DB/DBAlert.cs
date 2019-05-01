using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.DB
{
    public class DBAlert
    {
        public int Idalert { get; set; }
        public string Content { get; set; }
        public int? Mode_idmode { get; set; }
        public int AlertDefinition_idalertType { get; set; }
        public string Title { get; set; }
        public DateTime CreationDate { get; set; }
    }
}