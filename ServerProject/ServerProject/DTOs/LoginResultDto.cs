namespace ServerProject.DTOs
{
	/// <summary>
	/// 登录结果数据传输对象 (DTO)
	/// </summary>
	public class LoginResultDto : DtoBase
	{
		/// <summary>是否需要进行二步验证</summary>
		public bool NeedTwoFactor { get; set; }

		/// <summary>Session 登录时返回的 SessionId（给前端存 Cookie 或 Header 用）</summary>
		public Guid? SessionId { get; set; }

		/// <summary>JWT 登录时返回的 AccessToken</summary>
		public string? AccessToken { get; set; }

		/// <summary>JWT 登录时返回的 RefreshToken（明文，给前端保存）</summary>
		public string? RefreshToken { get; set; }

		/// <summary>
		/// 登录失败
		/// </summary>
		/// <param name="code">失败代码</param>
		/// <param name="message">失败消息</param>
		/// <returns>登录结果 DTO</returns>
		public static LoginResultDto Fail(int code,string message)
		{
			var dto = new LoginResultDto();
			dto.MarkFail(code, message);
			return dto;
		}

		/// <summary>
		/// 需要二步验证
		/// </summary>
		/// <param name="code">失败代码</param>
		/// <returns>登录结果 DTO</returns>
		public static LoginResultDto RequireTwoFactor(int code)
		{
			var dto = new LoginResultDto();
			dto.NeedTwoFactor = true;
			dto.MarkFail(code, "Two-step verification required");
			return dto;
		}

		/// <summary>
		/// 登录成功，使用 Session 认证
		/// </summary>
		/// <param name="sessionId">会话 ID</param>
		/// <returns>登录结果 DTO</returns>
		public static LoginResultDto SuccessWithSession(Guid sessionId)
		{
			var dto = new LoginResultDto();
			dto.SessionId = sessionId;
			dto.MarkSuccess();
			return dto;
		}

		/// <summary>
		/// 登录成功，使用 JWT 认证
		/// </summary>
		/// <param name="accessToken">访问令牌</param>
		/// <param name="refreshToken">刷新令牌</param>
		/// <returns>登录结果 DTO</returns>
		public static LoginResultDto SuccessWithJwt(string accessToken, string refreshToken)
		{
			var dto = new LoginResultDto();
			dto.AccessToken = accessToken;
			dto.RefreshToken = refreshToken;
			dto.MarkSuccess();
			return dto;
		}
	}
}
