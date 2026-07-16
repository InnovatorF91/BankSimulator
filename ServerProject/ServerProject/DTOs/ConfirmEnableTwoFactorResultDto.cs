namespace ServerProject.DTOs
{
	/// <summary>
	/// 確認啟用雙因素認證結果資料傳輸物件 (DTO)
	/// </summary>
	public class ConfirmEnableTwoFactorResultDto : DtoBase
	{
		/// <summary>
		/// 確認啟用雙因素認證成功
		/// </summary>
		/// <returns>確認啟用雙因素認證結果資料傳輸物件</returns>
		public static ConfirmEnableTwoFactorResultDto SuccessDto()
		{
			var dto = new ConfirmEnableTwoFactorResultDto();
			dto.MarkSuccess();
			return dto;
		}

		/// <summary>
		/// 確認啟用雙因素認證失敗
		/// </summary>
		/// <param name="code">失敗代碼</param>
		/// <param name="message">失敗消息</param>
		/// <returns>確認啟用雙因素認證結果資料傳輸物件</returns>
		public static ConfirmEnableTwoFactorResultDto Fail(int code, string message)
		{
			var dto = new ConfirmEnableTwoFactorResultDto();
			dto.MarkFail(code, message);
			return dto;
		}
	}
}
