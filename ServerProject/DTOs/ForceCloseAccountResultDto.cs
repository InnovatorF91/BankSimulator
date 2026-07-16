using ShareProject.Common;

namespace ServerProject.DTOs
{
	public class ForceCloseAccountResultDto : DtoBase
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
		/// 关闭时间
		/// </summary>
		public DateTime CloseDate { get; set; }

		/// <summary>
		/// 更新时间
		/// </summary>
		public DateTime UpdateDate { get; set; }

		/// <summary>
		/// 成功的管理员强制关闭账户结果Dto
		/// </summary>
		/// <param name="accountId">账户ID</param>
		/// <param name="status">账户状态</param>
		/// <param name="closeDate">关闭时间</param>
		/// <param name="updateDate">更新时间</param>
		/// <returns>管理员强制关闭账户结果Dto></returns>
		public static ForceCloseAccountResultDto SuccessDto(
			long accountId,
			AccountStatus status,
			DateTime closeDate,
			DateTime updateDate)
		{
			var dto = new ForceCloseAccountResultDto()
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
		/// 失败的管理员强制关闭账户结果Dto
		/// </summary>
		/// <param name="code">失敗代碼</param>
		/// <param name="message">失敗消息</param>
		/// <returns>管理员强制关闭账户结果Dto</returns>
		public static ForceCloseAccountResultDto Failure(int code, string message)
		{
			var dto = new ForceCloseAccountResultDto();
			dto.MarkFail(code, message);
			return dto;
		}
	}
}
