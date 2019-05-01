using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.DTO
{
    [DataContract]
    public class ShortSeekiosDTO
    {
        [DataMember]
        public int Idseekios { get; set; }
        [DataMember]
        public string SeekiosName { get; set; }
        [DataMember]
        public string SeekiosPicture { get; set; }
        [DataMember]
        public int User_iduser { get; set; }

        public static ShortSeekiosDTO SeekiosToShortSeekiosDTO(seekios source)
        {
            if (source == null) return null;
            return new ShortSeekiosDTO()
            {
                Idseekios = source.idseekios,
                SeekiosName = source.seekiosName,
                SeekiosPicture = source.seekiosPicture == null ? "" : Convert.ToBase64String(source.seekiosPicture),
                User_iduser = source.user_iduser,
            };
        }
    }
}