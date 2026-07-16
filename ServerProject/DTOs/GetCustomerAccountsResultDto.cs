namespace ServerProject.DTOs
{
	/// <summary>
	/// 客戶帳戶查詢結果的 DTO 類別
	/// </summary>
	public class GetCustomerAccountsResultDto : DtoBase
	{
		/// <summary>
		/// 客戶ID
		/// </summary>
		public int CustomerId { get; set; }

		/// <summary>
		/// 客戶的帳戶列表
		/// </summary>
		public List<AccountDto> Accounts { get; set; } = new();

		/// <summary>
		/// 建立成功的 DTO 實例
		/// </summary>
		/// <param name="customerId">客戶ID</param>
		/// <param name="accounts">客戶的帳戶列表</param>
		/// <returns>成功的客戶帳戶查詢結果實例</returns>
		public static GetCustomerAccountsResultDto SuccessDto(
			int customerId,
			List<AccountDto> accounts)
		{
			var dto = new GetCustomerAccountsResultDto
			{
				CustomerId = customerId,
				Accounts = accounts
			};

			dto.MarkSuccess();
			return dto;
		}

		/// <summary>
		/// 建立失敗的 DTO 實例
		/// </summary>
		/// <param name="code">失敗代碼</param>
		/// <param name="message">失敗訊息</param>
		/// <returns>失敗的客戶帳戶查詢結果實例</returns>
		public static GetCustomerAccountsResultDto Failure(int code, string message)
		{
			var dto = new GetCustomerAccountsResultDto();
			dto.MarkFail(code, message);
			return dto;
		}
	}
}
