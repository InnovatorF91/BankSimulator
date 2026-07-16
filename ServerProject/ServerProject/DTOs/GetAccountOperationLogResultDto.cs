namespace ServerProject.DTOs
{
	public class GetAccountOperationLogResultDto : DtoBase
	{
		/// <summary>
		/// 操作日志DTO
		/// </summary>
		public AccountOperationLogDto? Log { get; set; }

		/// <summary>
		/// 成功的取得操作日志DTO
		/// </summary>
		/// <param name="log">操作日志DTO</param>
		/// <returns>取得指定操作日志DTO</returns>
		public static GetAccountOperationLogResultDto SuccessDto(
			AccountOperationLogDto log)
		{
			var dto = new GetAccountOperationLogResultDto()
			{
				Log = log
			};
			dto.MarkSuccess();
			return dto;
		}

		/// <summary>
		/// 失败的取得操作日志DTO
		/// </summary>
		/// <param name="code">失敗代碼</param>
		/// <param name="message">失敗消息</param>
		/// <returns>取得指定操作日志DTO</returns>
		public static GetAccountOperationLogResultDto Failure(int code, string message)
		{
			var dto = new GetAccountOperationLogResultDto();
			dto.MarkFail(code, message);
			return dto;
		}
	}
}
