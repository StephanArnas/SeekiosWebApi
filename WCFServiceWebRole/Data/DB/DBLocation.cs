using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.DB
{
    [DataContract]
    public class DBLocation
    {
        [DataMember]
        public int IdL { get; set; }
        [DataMember]
        public double Lo { get; set; }
        [DataMember]
        public double La { get; set; }
        [DataMember]
        public double Al { get; set; }
        [DataMember]
        public double Ac { get; set; }
        [DataMember]
        public DateTime Dc { get; set; }
        [DataMember]
        public int? IdM { get; set; }
        [DataMember]
        public int IdS { get; set; }
        [DataMember]
        public int IdLD { get; set; }

        public static DBLocation LocationToDbLocation(location source)
        {
            if (source == null) return null;
            return new DBLocation()
            {
                IdL = source.idlocation,
                Lo = source.longitude ?? 0.0,
                La = source.latitude ?? 0.0,
                Al = source.altitude ?? 0.0,
                Ac = source.accuracy ?? 0.0,
                Dc = source.dateLocationCreation ?? DateTime.MinValue,
                IdM = source.mode_idmode,
                IdS = source.seekios_idseekios,
                IdLD = source.locationDefinition_idlocationDefinition
            };
        }
    }
}