using ShareProject.Common;

namespace ServerProject.DTOs
{
	public class FreezeAccountResultDto : DtoBase
	{
		/// <summary>
		/// 帳戶ID
		/// </summary>
		public long AccountId { get; set; }

		/// <summary>
		/// 帳戶狀態
		/// </summary>
		public AccountStatus Status { get; set; }

		/// <summary>
		/// 更新日期
		/// </summary>
		public DateTime UpdateDate { get; set; }

		/// <summary>
		/// 建立一個成功的凍結帳戶結果 DTO
		/// </summary>
		/// <param name="accountId">帳戶ID</param>
		/// <param name="status">帳戶狀態</param>
		/// <param name="updateDate">更新日期</param>
		/// <returns>成功的凍結帳戶結果 DTO</returns>
		public static FreezeAccountResultDto SuccessDto(
			long accountId,
			AccountStatus status,
			DateTime updateDate)
		{
			var dto = new FreezeAccountResultDto
			{
				AccountId = accountId,
				Status = status,
				UpdateDate = updateDate
			};

			dto.MarkSuccess();
			return dto;
		}

		/// <summary>
		/// 建立一個失敗的凍結帳戶結果 DTO
		/// </summary>
		/// <param name="code">錯誤代碼</param>
		/// <param name="message">錯誤訊息</param>
		/// <returns>失敗的凍結帳戶結果 DTO</returns>
		public static FreezeAccountResultDto Failure(int code, string message)
		{
			var dto = new FreezeAccountResultDto();
			dto.MarkFail(code, message);
			return dto;
		}
	}
}
