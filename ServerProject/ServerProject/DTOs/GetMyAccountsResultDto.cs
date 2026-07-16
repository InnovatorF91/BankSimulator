namespace ServerProject.DTOs
{
	public class GetMyAccountsResultDto : DtoBase
	{
		public List<AccountDto> Accounts { get; set; } = new();

		public static GetMyAccountsResultDto SuccessDto(List<AccountDto> accounts)
		{
			var dto = new GetMyAccountsResultDto
			{
				Accounts = accounts
			};
			dto.MarkSuccess();
			return dto;
		}

		public static GetMyAccountsResultDto Failure(int code, string message)
		{
			var dto = new GetMyAccountsResultDto();
			dto.MarkFail(code, message);
			return dto;
		}
	}
}
