using System;

namespace WCFServiceWebRole.Data.DTO
{
    public class OperationTransactionDTO
    {
        public int IdOperationTransaction { get; set; }
        public int IdPack { get; set; }
        public int IdUser { get; set; }
        public string Status { get; set; }
        public DateTime DateTransaction { get; set; }
        public string RefStore { get; set; }
        public string VersionApp { get; set; }
        public int CreditsPurchased { get; set; }
        public bool IsPackPremium { get; set; }
    }
}