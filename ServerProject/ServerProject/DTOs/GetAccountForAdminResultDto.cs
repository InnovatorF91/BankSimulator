using ShareProject.Common;

namespace ServerProject.DTOs
{
	public class GetAccountForAdminResultDto : DtoBase
	{
		/// <summary>
		/// 帳戶ID
		/// </summary>
		public long AccountId { get; set; }

		/// <summary>
		/// 客戶ID
		/// </summary>
		public int CustomerId { get; set; }

		/// <summary>
		/// 帳戶類型
		/// </summary>
		public AccountType AccountType { get; set; }

		/// <summary>
		/// 帳戶餘額
		/// </summary>
		public decimal Balance { get; set; }

		/// <summary>
		/// 帳戶貨幣
		/// </summary>
		public CurrencyCode Currency { get; set; }

		/// <summary>
		/// 帳戶狀態
		/// </summary>
		public AccountStatus Status { get; set; }

		/// <summary>
		/// 開戶日期
		/// </summary>
		public DateTime OpenDate { get; set; }

		/// <summary>
		/// 關戶日期
		/// </summary>
		public DateTime? CloseDate { get; set; }

		/// <summary>
		/// 更新日期
		/// </summary>
		public DateTime UpdateDate { get; set; }

		/// <summary>
		/// 建立成功的 DTO 實例
		/// </summary>
		/// <param name="accountId">帳戶ID</param>
		/// <param name="customerId">客戶ID</param>
		/// <param name="accountType">帳戶類型</param>
		/// <param name="balance">帳戶餘額</param>
		/// <param name="currency">帳戶貨幣</param>
		/// <param name="status">帳戶狀態</param>
		/// <param name="openDate">開戶日期</param>
		/// <param name="closeDate">關戶日期</param>
		/// <param name="updateDate">更新日期</param>
		/// <returns>成功的帳戶查詢結果實例</returns>
		public static GetAccountForAdminResultDto SuccessDto(
			long accountId,
			int customerId,
			AccountType accountType,
			decimal balance,
			CurrencyCode currency,
			AccountStatus status,
			DateTime openDate,
			DateTime? closeDate,
			DateTime updateDate)
		{
			var dto = new GetAccountForAdminResultDto
			{
				AccountId = accountId,
				CustomerId = customerId,
				AccountType = accountType,
				Balance = balance,
				Currency = currency,
				Status = status,
				OpenDate = openDate,
				CloseDate = closeDate,
				UpdateDate = updateDate
			};

			dto.MarkSuccess();
			return dto;
		}

		/// <summary>
		/// 建立失敗的 DTO 實例
		/// </summary>
		/// <param name="code">失敗代碼</param>
		/// <param name="message">失敗訊息</param>
		/// <returns>失敗的帳戶查詢結果實例</returns>
		public static GetAccountForAdminResultDto Failure(int code, string message)
		{
			var dto = new GetAccountForAdminResultDto();
			dto.MarkFail(code, message);
			return dto;
		}
	}
}
