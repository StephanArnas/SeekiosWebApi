using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.DTO
{
    public class SeekiosIMEIAndPinDTO : IEquatable<SeekiosIMEIAndPinDTO>
    {
        public int IdSeekios { get; set; }
        public string IMEI { get; set; }
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