namespace ServerProject.DTOs
{
	public class GetAccountOperationLogsResultDto : DtoBase
	{
		/// <summary>
		/// 账户ID
		/// </summary>
		public long AccountId { get; set; }

		/// <summary>
		/// 账户操作日志列表
		/// </summary>
		public List<AccountOperationLogDto> Logs { get; set; } = new();

		/// <summary>
		/// 成功的取得所有账户操作日志的DTO
		/// </summary>
		/// <param name="accountId">账户ID</param>
		/// <param name="logs">账户操作日志列表</param>
		/// <returns>取得所有账户操作日志的DTO</returns>
		public static GetAccountOperationLogsResultDto SuccessDto(
			long accountId,
			List<AccountOperationLogDto> logs)
		{
			var dto = new GetAccountOperationLogsResultDto()
			{
				AccountId = accountId,
				Logs = logs
			};
			dto.MarkSuccess();
			return dto;
		}

		/// <summary>
		/// 失败的取得所有账户操作日志的DTO
		/// </summary>
		/// <param name="code">失敗代碼</param>
		/// <param name="message">失敗消息</param>
		/// <returns>取得所有账户操作日志的DTO</returns>
		public static GetAccountOperationLogsResultDto Failure(int code, string message)
		{
			var dto = new GetAccountOperationLogsResultDto();
			dto.MarkFail(code, message);
			return dto;
		}
	}
}
