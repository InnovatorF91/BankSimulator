using ShareProject.Common;

namespace ShareProject.Request
{
    /// <summary>
    /// 存款請求類別，包含存款所需的屬性
    /// </summary>
    public class DepositRequest : BaseRequest
    {
        public Guid AccountId { get; set; } // 帳戶ID
        public decimal Amount { get; set; } // 存款金額
        public CurrencyCode Currency { get; set; } // 幣種
        public string Source { get; set; } = string.Empty; // 存款來源
        public string Memo { get; set; } = string.Empty; // 備註
    }
}
