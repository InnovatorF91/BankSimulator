namespace ShareProject.Response
{
	public class LoginResponse : BaseResponse
	{
		/// <summary>
		/// 登录成功后创建的会话 ID，用于后续请求的 X-Session-Id 请求头。
		/// </summary>
		public Guid SessionId { get; set; }
	}
}