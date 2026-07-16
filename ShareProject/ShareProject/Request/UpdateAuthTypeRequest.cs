using ShareProject.Common;

namespace ShareProject.Request
{
	public class UpdateAuthTypeRequest
	{
		/// <summary>
		/// 用户ID
		/// </summary>
		public int CustomerId { get; set; }

		/// <summary>
		/// 认证类型
		/// </summary>
		public AuthType AuthType { get; set; }
	}
}
