using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WCFServiceWebRole.Enum;

namespace WCFServiceWebRole.Data.LOCAL
{
    public class SeekiosInstruction
    {
        public DateTime DateInstruction { get; set; }
        public InstructionType TypeInstruction { get; set; }
        public string TrameInstruction { get; set; }
    }
}