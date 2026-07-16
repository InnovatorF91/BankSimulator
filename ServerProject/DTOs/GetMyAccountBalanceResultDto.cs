using ShareProject.Common;

namespace ServerProject.DTOs
{
	/// <summary>
	/// 获取我的账户余额结果DTO
	/// </summary>
	public class GetMyAccountBalanceResultDto : DtoBase
	{
		/// <summary>
		/// 账户ID
		/// </summary>
		public long AccountId { get; set; }

		/// <summary>
		/// 账户余额
		/// </summary>
		public decimal Balance { get; set; }

		/// <summary>
		/// 货币类型
		/// </summary>
		public CurrencyCode Currency { get; set; }

		/// <summary>
		/// 账户状态
		/// </summary>
		public AccountStatus Status { get; set; }

		/// <summary>
		/// 最后更新时间
		/// </summary>
		public DateTime UpdateDate { get; set; }

		/// <summary>
		/// 创建成功的结果DTO
		/// </summary>
		/// <param name="accountId">账户ID</param>
		/// <param name="balance">账户余额</param>
		/// <param name="currency">货币类型</param>
		/// <param name="status">账户状态</param>
		/// <param name="updateDate">最后更新时间</param>
		/// <returns>成功的结果DTO</returns>
		public static GetMyAccountBalanceResultDto SuccessDto(
			long accountId,
			decimal balance,
			CurrencyCode currency,
			AccountStatus status,
			DateTime updateDate)
		{
			var dto = new GetMyAccountBalanceResultDto
			{
				AccountId = accountId,
				Balance = balance,
				Currency = currency,
				Status = status,
				UpdateDate = updateDate
			};

			dto.MarkSuccess();
			return dto;
		}

		/// <summary>
		/// 创建失败的结果DTO
		/// </summary>
		/// <param name="code">错误代码</param>
		/// <param name="message">错误信息</param>
		/// <returns>失败的结果DTO</returns>
		public static GetMyAccountBalanceResultDto Failure(int code, string message)
		{
			var dto = new GetMyAccountBalanceResultDto();
			dto.MarkFail(code, message);
			return dto;
		}
	}
}
