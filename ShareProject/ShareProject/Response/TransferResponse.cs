using ShareProject.Common;

namespace ShareProject.Response
{
    /// <summary>
    /// 轉帳回應類別，包含轉帳的相關資訊
    /// </summary>
    public class TransferResponse : BaseResponse
    {
        public Guid FromTransactionId { get; set; } // 轉出交易ID
        public Guid ToTransactionId { get; set; } // 轉入交易ID
        public decimal Fee { get; set; } // 轉帳手續費
        public CurrencyCode Currency { get; set; } // 幣種
    }
}
