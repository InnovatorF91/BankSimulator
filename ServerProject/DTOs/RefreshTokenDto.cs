namespace ServerProject.DTOs
{
	/// <summary>
	/// 刷新令牌資料傳輸物件 (DTO)
	/// </summary>
	public class RefreshTokenDto : DtoBase
	{
		/// <summary>
		/// 令牌ID（主键，数据库生成）
		/// </summary>
		public string RefreshToken { get; set; } = string.Empty;

		/// <summary>
		/// 访问令牌（JWT），成功时返回，失败时为空字符串
		/// </summary>
		public string AccessToken { get; set; } = string.Empty;

		/// <summary>
		///  刷新令牌資料成功
		/// </summary>
		/// <returns>刷新令牌資料傳輸物件 (DTO)</returns>
		public static RefreshTokenDto SuccessDto(string accessToken,string refreshToken)
		{
			var dto = new RefreshTokenDto();
			dto.AccessToken = accessToken;
			dto.RefreshToken = refreshToken;
			dto.MarkSuccess();
			return dto;
		}

		/// <summary>
		/// 刷新令牌資料失敗
		/// </summary>
		/// <param name="code">失敗代碼</param>
		/// <param name="message">失敗消息</param>
		/// <returns>刷新令牌資料傳輸物件 (DTO)</returns>
		public static RefreshTokenDto Fail(int code, string message)
		{
			var dto = new RefreshTokenDto();
			dto.MarkFail(code, message);
			return dto;
		}
	}
}
