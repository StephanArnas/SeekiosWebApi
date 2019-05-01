using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole.Data.DTO
{
    public class ShortSeekiosDTO
    {
        public int Idseekios { get; set; }
        public string SeekiosName { get; set; }
        public string SeekiosPicture { get; set; }
        public int User_iduser { get; set; }
    }
}