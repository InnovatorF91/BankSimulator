namespace ServerProject.DTOs
{
	/// <summary>
	/// 驗證會話結果資料傳輸物件 (DTO)
	/// </summary>
	public class ValidateSessionResultDto : DtoBase
	{
		/// <summary>
		/// 驗證會話成功
		/// </summary>
		/// <returns>驗證會話結果資料傳輸物件</returns>
		public static ValidateSessionResultDto SuccessDto()
		{
			var dto = new ValidateSessionResultDto();
			dto.MarkSuccess();
			return dto;
		}

		/// <summary>
		/// 驗證會話失敗
		/// </summary>
		/// <param name="code">失敗代碼</param>
		/// <param name="message">失敗消息</param>
		/// <returns>驗證會話結果資料傳輸物件</returns>
		public static ValidateSessionResultDto Fail(int code, string message)
		{
			var dto = new ValidateSessionResultDto();
			dto.MarkFail(code, message);
			return dto;
		}
	}
}
