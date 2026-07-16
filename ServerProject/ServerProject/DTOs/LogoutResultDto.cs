namespace ServerProject.DTOs
{
	/// <summary>
	/// 登出結果 DTO
	/// </summary>
	public class LogoutResultDto : DtoBase
	{
		/// <summary>
		/// 登出成功
		/// </summary>
		/// <returns>登出結果 DTO </returns>
		public static LogoutResultDto SuccessDto()
		{
			var dto = new LogoutResultDto();
			dto.MarkSuccess();
			return dto;
		}

		/// <summary>
		/// 登出失敗
		/// </summary>
		/// <param name="code">失敗代碼</param>
		/// <param name="message">失敗消息</param>
		/// <returns>登出結果 DTO </returns>
		public static LogoutResultDto Fail(int code, string message)
		{
			var dto = new LogoutResultDto();
			dto.MarkFail(code, message);
			return dto;
		}
	}
}
