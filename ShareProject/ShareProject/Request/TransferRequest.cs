using ShareProject.Common;

namespace ShareProject.Request
{
    /// <summary>
    /// 轉帳請求類別，包含轉帳所需的屬性
    /// </summary>
    public class TransferRequest : BaseRequest
    {
        public Guid FromAccountId { get; set; } // 轉出帳戶ID
        public Guid ToAccountId { get; set; } // 轉入帳戶ID
        public decimal Amount { get; set; } // 轉帳金額
        public CurrencyCode Currency { get; set; } // 幣種
        public string Memo { get; set; } = string.Empty; // 備註
    }
}
