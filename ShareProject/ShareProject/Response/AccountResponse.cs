using ShareProject.Common;

namespace ShareProject.Response
{
	public class AccountResponse : BaseResponse
	{
		public decimal? Amount { get; set; }

		public AccountDto Account { get; set; } = new AccountDto();

		public List<AccountDto> Accounts { get; set; } = new List<AccountDto>();
	}
}
