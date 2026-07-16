namespace ServerProject.DTOs
{
	public abstract class DtoBase
	{
		/// <summary>
		/// 用例是否成功
		/// </summary>
		public bool Success { get; protected set; }

		/// <summary>
		/// 业务结果码（0 = 成功，其它为失败）
		/// </summary>
		public int Code { get; protected set; }

		/// <summary>
		/// 给 Controller / 日志用的消息
		/// </summary>
		public string? Message { get; protected set; }

		/// <summary>
		/// 构造函数
		/// </summary>
		protected DtoBase()
		{
		}

		/// <summary>
		/// 标记为成功
		/// </summary>
		/// <param name="code">业务结果码(默认为0)</param>
		/// <param name="message">给 Controller / 日志用的消息</param>
		protected void MarkSuccess(
			int code = 0,
			string? message = null)
		{
			Success = true;
			Code = code;
			Message = message;
		}

		/// <summary>
		/// 标记为失败
		/// </summary>
		/// <param name="code">业务结果码</param>
		/// <param name="message">给 Controller / 日志用的消息</param>
		protected void MarkFail(
			int code,
			string message)
		{
			Success = false;
			Code = code;
			Message = message;
		}
	}
}
