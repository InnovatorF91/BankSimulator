namespace ShareProject.Request
{
	/// <summary>
	/// 获取客户信息的请求。
	/// </summary>
	public class GetCustomerRequest
	{
		/// <summary>
		/// 要查询的客户 ID。
		/// </summary>
		public int Id { get; set; }
	}
}
