using ShareProject.Common;

namespace ServerProject.DTOs
{
	public class UnfreezeAccountResultDto : DtoBase
	{
		/// <summary>
		/// 账户ID
		/// </summary>
		public long AccountId { get; set; }

		/// <summary>
		/// 账户状态
		/// </summary>
		public AccountStatus Status { get; set; }

		/// <summary>
		/// 更新日期
		/// </summary>
		public DateTime UpdateDate { get; set; }

		/// <summary>
		/// 建立一個成功的解除凍結帳戶結果 DTO
		/// </summary>
		/// <param name="accountId">账户ID</param>
		/// <param name="status">账户状态</param>
		/// <param name="updateDate">更新日期</param>
		/// <returns>成功的解除凍結帳戶結果</returns>
		public static UnfreezeAccountResultDto SuccessDto(
			long accountId,
			AccountStatus status,
			DateTime updateDate)
		{
			var dto = new UnfreezeAccountResultDto
			{
				AccountId = accountId,
				Status = status,
				UpdateDate = updateDate
			};

			dto.MarkSuccess();
			return dto;
		}

		/// <summary>
		/// 建立一個失敗的解除凍結帳戶結果 DTO
		/// </summary>
		/// <param name="code">錯誤代碼</param>
		/// <param name="message">錯誤訊息</param>
		/// <returns>失敗的解除凍結帳戶結果</returns>
		public static UnfreezeAccountResultDto Failure(int code, string message)
		{
			var dto = new UnfreezeAccountResultDto();
			dto.MarkFail(code, message);
			return dto;
		}
	}
}
