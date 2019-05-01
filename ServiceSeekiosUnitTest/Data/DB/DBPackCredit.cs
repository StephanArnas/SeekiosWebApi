using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.DB
{
    public class DBPackCredit
    {
        public int IdPackCredit { get; set; }
        public string IdProduct { get; set; }
        public string Price { get; set; }
        public int Rewarding { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int IsPromotion { get; set; }
        public string Promotion { get; set; }
        public string ColorBackground { get; set; }
        public string ColorHeaderBackground { get; set; }
        public string LanguagePackCredit { get; set; }
    }
}