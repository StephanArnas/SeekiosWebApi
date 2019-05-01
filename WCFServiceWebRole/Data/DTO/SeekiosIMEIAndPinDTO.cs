using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.DTO
{
    [DataContract]
    public class SeekiosIMEIAndPinDTO : IEquatable<SeekiosIMEIAndPinDTO>
    {
        [DataMember]
        public int IdSeekios { get; set; }
        [DataMember]
        public string IMEI { get; set; }
        [DataMember]
        public string PIN { get; set; }

        public bool Equals(SeekiosIMEIAndPinDTO other)
        {
            if (other == null) return false;
            else return this.IdSeekios.Equals(other.IdSeekios);
        }

        public override int GetHashCode()
        {
            return this.IdSeekios.GetHashCode();
        }
    }
}