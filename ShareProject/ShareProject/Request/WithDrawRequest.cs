using ShareProject.Common;

namespace ShareProject.Request
{
    /// <summary>
    /// 提款請求類別，包含提款所需的屬性
    /// </summary>
    public class WithDrawRequest : BaseRequest
    {
        public Guid AccountId { get; set; } // 帳戶ID
        public decimal Amount { get; set; } // 提款金額
        public CurrencyCode Currency { get; set; } // 幣種
        public string Method { get; set; } = string.Empty; // 提款方式
        public string Memo { get; set; } = string.Empty; // 備註
    }
}
