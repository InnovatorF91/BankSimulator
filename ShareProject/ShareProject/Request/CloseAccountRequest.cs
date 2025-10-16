using ShareProject.Common;

namespace ShareProject.Request
{
    /// <summary>
    /// 關閉帳戶請求類別，包含關閉帳戶所需的屬性
    /// </summary>
    public class CloseAccountRequest : BaseRequest
    {
        public Guid AccountId { get; set; } // 帳戶ID
        public Guid? PayoutTargetAccountId { get; set; } // 退款目標帳戶ID，用於處理關閉帳戶後的餘額轉移
        public TransactionType TransactionType { get; set; } // 交易類型，用於記錄關閉帳戶時的交易
    }
}
