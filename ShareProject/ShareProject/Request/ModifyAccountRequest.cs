using ShareProject.Common;

namespace ShareProject.Request
{
    /// <summary>
    /// 修改帳戶請求類別，包含修改現有銀行帳戶所需的屬性
    /// </summary>
    public class ModifyAccountRequest : BaseRequest
    {
        public AccountDto Account { get; set; } = new AccountDto(); // 要修改的帳戶資料
        public string Email { get; set; } = string.Empty; // 使用者電子郵件
        public string Phone { get; set; } = string.Empty; // 使用者電話號碼
        public long TransactionId { get; set; } // 相關交易ID（如果有的話）
    }
}
