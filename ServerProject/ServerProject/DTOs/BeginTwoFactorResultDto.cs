namespace ServerProject.DTOs
{
	/// <summary>
	/// 開始雙因素認證結果資料傳輸物件 (DTO)
	/// </summary>
	public class BeginTwoFactorResultDto : DtoBase
	{
		/// <summary>
		/// 密鑰
		/// </summary>
		public string? Secret { get; set; }

		/// <summary>
		/// 過期時間
		/// </summary>
		public DateTime? ExpiresAt { get; set; }

		/// <summary>
		/// otpauth URI
		/// </summary>
		public string? OtpauthUri { get; set; }

		/// <summary>
		/// 開始雙因素認證成功
		/// </summary>
		/// <param name="secret">密鑰</param>
		/// <param name="expiresAt">過期時間</param>
		/// <param name="otpauthUri">otpauth URI</param>
		/// <returns>開始雙因素認證結果資料傳輸物件</returns>
		public static BeginTwoFactorResultDto SuccessDto(string secret, DateTime expiresAt, string otpauthUri)
		{
			var dto = new BeginTwoFactorResultDto()
			{
				Secret = secret,
				ExpiresAt = expiresAt,
				OtpauthUri = otpauthUri,
			};
			dto.MarkSuccess();
			return dto;
		}

		/// <summary>
		/// 開始雙因素認證失敗
		/// </summary>
		/// <param name="code">失敗代碼</param>
		/// <param name="message">失敗消息</param>
		/// <returns>開始雙因素認證結果資料傳輸物件</returns>
		public static BeginTwoFactorResultDto Fail(int code, string message)
		{
			var dto = new BeginTwoFactorResultDto();
			dto.MarkFail(code, message);
			return dto;
		}
	}
}
