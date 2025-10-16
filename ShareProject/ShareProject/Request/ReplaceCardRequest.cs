using ShareProject.Common;

namespace ShareProject.Request
{
    /// <summary>
    /// 換卡請求類別，包含換卡所需的屬性
    /// </summary>
    public class ReplaceCardRequest : BaseRequest
    {
        public Guid CardId { get; set; } // 卡片ID
        public CardReplaceReason Reason { get; set; } // 換卡原因
        public string NewDeliveryAddress { get; set; } = string.Empty; // 新的寄送地址
    }
}
