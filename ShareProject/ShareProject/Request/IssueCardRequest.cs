using ShareProject.Common;

namespace ShareProject.Request
{
    /// <summary>
    /// 發卡請求類別，包含發卡所需的屬性
    /// </summary>
    public class IssueCardRequest : BaseRequest
    {
        public Guid AccountId { get; set; } // 帳戶ID
        public string CardholderName { get; set; } = string.Empty; // 持卡人姓名
        public CardType CardType { get; set; } // 卡片類型
        public string DeliveryAddress { get; set; } = string.Empty; // 寄送地址
        public DateTime? ExpiryHint { get; set; } // 到期日提示，若不提供則使用預設值
    }
}
