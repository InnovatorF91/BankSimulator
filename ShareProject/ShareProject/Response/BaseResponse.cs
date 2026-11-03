namespace ShareProject.Response
{
	/// <summary>
	/// API 回应的公共字段。
	/// </summary>
	public abstract class BaseResponse
	{
		/// <summary>
		/// 业务是否成功。
		/// </summary>
		public bool Success { get; set; }

		/// <summary>
		/// 业务结果码：0 表示成功，其它值表示失败。
		/// </summary>
		public int Code { get; set; }

		/// <summary>
		/// 面向客户端的结果说明。
		/// </summary>
		public string Message { get; set; } = string.Empty;
	}
}