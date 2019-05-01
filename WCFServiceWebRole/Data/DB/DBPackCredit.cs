using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.DB
{
    [DataContract]
    public class DBPackCredit
    {
        [DataMember]
        public int IdPackCredit { get; set; }
        [DataMember]
        public string IdProduct { get; set; }
        [DataMember]
        public string Price { get; set; }
        [DataMember]
        public int Rewarding { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public int IsPromotion { get; set; }
        [DataMember]
        public string Promotion { get; set; }
        [DataMember]
        public string ColorBackground { get; set; }
        [DataMember]
        public string ColorHeaderBackground { get; set; }
        [DataMember]
        public string LanguagePackCredit { get; set; }

        public static DBPackCredit PackCreditToDBPackCredit(packCredit source)
        {
            if (source == null) return null;
            return new DBPackCredit()
            {
                IdPackCredit = source.idcreditPack,
                IdProduct = source.idProduct,
                ColorBackground = source.colorBacground,
                ColorHeaderBackground = source.colorHeaderBackground,
                Description = source.description,
                Title = source.title,
                IsPromotion = source.isPromotion,
                Price = source.price,
                Promotion = source.promotion,
                Rewarding = source.rewarding,
                LanguagePackCredit = source.language
            };
        }
    }
}