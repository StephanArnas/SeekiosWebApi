using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.DB
{
    public class DBLocation
    {
        public int IdL { get; set; }
        public double Lo { get; set; }
        public double La { get; set; }
        public double Al { get; set; }
        public double Ac { get; set; }
        public DateTime Dc { get; set; }
        public int IdM { get; set; }
        public int IdS { get; set; }
        public int IdLD { get; set; }
    }
}