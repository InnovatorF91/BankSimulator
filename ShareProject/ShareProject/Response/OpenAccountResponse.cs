using ShareProject.Common;

namespace ShareProject.Response
{
    /// <summary>
    /// 新開戶回應類別，包含新開戶的帳戶資料
    /// </summary>
    public class OpenAccountResponse : BaseResponse
    {
        public AccountDto Account { get; set; } = new AccountDto(); // 新開戶的帳戶資料
    }
}
