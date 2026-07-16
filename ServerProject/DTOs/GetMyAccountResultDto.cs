using ShareProject.Common;

namespace ServerProject.DTOs
{
	public class GetMyAccountResultDto : DtoBase
	{
		/// <summary>
		/// 帳戶ID
		/// </summary>
		public long AccountId { get; set; }

		/// <summary>
		/// 帳戶類型，1: 儲蓄帳戶, 2: 支票帳戶, 3: 外幣帳戶
		/// </summary>
		public AccountType AccountType { get; set; }

		/// <summary>
		/// 帳戶餘額
		/// </summary>
		public decimal Balance { get; set; }

		/// <summary>
		/// 貨幣類型，1: JPY, 2: USD, 3: EUR, 4: CNY, 5: HKD, 6: TWD
		/// </summary>
		public CurrencyCode Currency { get; set; }

		/// <summary>
		/// 帳戶狀態，1: 正常, 2: 冻结, 3: 关闭
		/// </summary>
		public AccountStatus Status { get; set; }

		/// <summary>
		/// 開戶日期
		/// </summary>
		public DateTime OpenDate { get; set; }

		/// <summary>
		/// 關閉日期，若帳戶未關閉則為 null
		/// </summary>
		public DateTime? CloseDate { get; set; }

		/// <summary>
		/// 最後更新日期
		/// </summary>
		public DateTime UpdateDate { get; set; }

		/// <summary>
		/// 建立一個成功的 GetMyAccountResultDto 實例
		/// </summary>
		/// <param name="accountId">帳戶ID</param>
		/// <param name="accountType">帳戶類型</param>
		/// <param name="balance">帳戶餘額</param>
		/// <param name="currency">貨幣類型</param>
		/// <param name="status">帳戶狀態</param>
		/// <param name="openDate">開戶日期</param>
		/// <param name="closeDate">關閉日期，若帳戶未關閉則為 null</param>
		/// <param name="updateDate">最後更新日期</param>"
		/// <returns>成功的 GetMyAccountResultDto 實例</returns>
		public static GetMyAccountResultDto SuccessDto(long accountId, AccountType accountType, decimal balance, CurrencyCode currency, AccountStatus status, DateTime openDate, DateTime? closeDate, DateTime updateDate)
		{
			var dto = new GetMyAccountResultDto
			{
				AccountId = accountId,
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
		/// 建立一個失敗的 GetMyAccountResultDto 實例
		/// </summary>
		/// <param name="code">错误码</param>
		/// <param name="message">错误信息</param>
		/// <returns>失敗的 GetMyAccountResultDto 實例</returns>
		public static GetMyAccountResultDto Failure(int code, string message)
		{
			var dto = new GetMyAccountResultDto();
			dto.MarkFail(code, message);
			return dto;
		}
	}
}
