namespace ServerProject.DTOs
{
	/// <summary>
	/// 触摸会话结果 DTO
	/// </summary>
	public class TouchSessionResultDto : DtoBase
	{
		/// <summary>
		/// 触摸会话成功
		/// </summary>
		/// <returns>触摸会话结果 DTO </returns>
		public static TouchSessionResultDto SuccessDto()
		{
			var dto = new TouchSessionResultDto();
			dto.MarkSuccess();
			return dto;
		}

		/// <summary>
		/// 触摸会话失败
		/// </summary>
		/// <param name="code">失败代码</param>
		/// <param name="message">失败消息</param>
		/// <returns>触摸会话结果 DTO </returns>
		public static TouchSessionResultDto Fail(int code, string message)
		{
			var dto = new TouchSessionResultDto();
			dto.MarkFail(code, message);
			return dto;
		}
	}
}
