using ShareProject.Common;

namespace ShareProject.Request
{
    /// <summary>
    /// 開戶請求類別，包含開立新銀行帳戶所需的屬性
    /// </summary>
    public class OpenAccountRequest : BaseRequest
    {
        public AccountDto Account { get; set; } = new AccountDto(); // 要開立的帳戶資料
        public string Email { get; set; } = string.Empty; // 使用者電子郵件
        public string Phone { get; set; } = string.Empty; // 使用者電話號碼
    }
}
