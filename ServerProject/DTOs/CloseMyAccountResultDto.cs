using ShareProject.Common;

namespace ServerProject.DTOs
{
	public class CloseMyAccountResultDto : DtoBase
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
		/// 账户关闭日期
		/// </summary>
		public DateTime CloseDate { get; set; }

		/// <summary>
		/// 账户信息更新时间
		/// </summary>
		public DateTime UpdateDate { get; set; }

		/// <summary>
		/// 创建成功的关闭账户结果DTO
		/// </summary>
		/// <param name="accountId">账户ID</param>
		/// <param name="status">账户状态</param>
		/// <param name="closeDate">账户关闭日期</param>
		/// <param name="updateDate">账户信息更新时间</param>
		/// <returns>关闭账户结果DTO</returns>
		public static CloseMyAccountResultDto SuccessDto(
			long accountId,
			AccountStatus status,
			DateTime closeDate,
			DateTime updateDate)
		{
			var dto = new CloseMyAccountResultDto
			{
				AccountId = accountId,
				Status = status,
				CloseDate = closeDate,
				UpdateDate = updateDate
			};

			dto.MarkSuccess();
			return dto;
		}

		/// <summary>
		/// 创建失败的关闭账户结果DTO
		/// </summary>
		/// <param name="code">错误代码</param>
		/// <param name="message">错误信息</param>
		/// <returns></returns>
		public static CloseMyAccountResultDto Failure(int code, string message)
		{
			var dto = new CloseMyAccountResultDto();
			dto.MarkFail(code, message);
			return dto;
		}
	}
}
