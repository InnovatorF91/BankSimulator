using ShareProject.Common;

namespace ShareProject.Request
{
	public class LogoutCurrentDeviceRequest
	{
		/// <summary>
		/// 用户ID
		/// </summary>
		public int CustomerId { get; set; }

		/// <summary>
		/// 当前认证类型
		/// </summary>
		public AuthType AuthType { get; set; }

		/// <summary>
		/// Session 模式时使用
		/// </summary>
		public Guid? SessionId { get; set; }

		/// <summary>
		/// JWT 模式时使用
		/// </summary>
		public string? RefreshToken { get; set; }
	}
}
