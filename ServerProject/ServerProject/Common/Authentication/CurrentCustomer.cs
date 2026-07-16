namespace ServerProject.Common
{
	/// <summary>
	/// 当前登录客户信息
	/// </summary>
	public class CurrentCustomer : ICurrentCustomer
	{
		/// <summary>
		/// HTTP上下文访问器
		/// </summary>
		private readonly IHttpContextAccessor _httpContextAccessor;

		/// <summary>
		/// 初始化当前登录客户信息
		/// </summary>
		/// <param name="httpContextAccessor">HTTP上下文访问器</param>
		public CurrentCustomer(IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
		}

		/// <summary>
		/// 当前请求是否已经通过认证。
		/// 当 HTTP 上下文或用户信息可用时，为 true。
		/// </summary>
		public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;

		/// <summary>
		/// 当前登录客户 ID。
		/// </summary>
		public int CustomerId
		{
			get
			{
				// 如果当前请求未通过认证，抛出异常
				if (!IsAuthenticated)
				{
					throw new InvalidOperationException("当前请求未通过认证。");
				}

				// 尝试从用户的 Claim 中获取客户 ID
				var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("uid");

				// 如果 Claim 不存在或无法解析为整数，抛出异常
				if (claim == null || !int.TryParse(claim.Value, out int customerId))
				{
					throw new InvalidOperationException("当前请求的用户信息不完整，无法获取客户ID。");
				}

				// 返回客户 ID
				return customerId;
			}
		}
	}

	/// <summary>
	/// 当前登录客户信息接口
	/// </summary>
	public interface ICurrentCustomer
	{
		/// <summary>
		/// 当前请求是否已经通过认证。
		/// </summary>
		bool IsAuthenticated { get; }

		/// <summary>
		/// 当前登录客户 ID。
		/// 未认证或 Claim 不存在时抛出异常。
		/// </summary>
		int CustomerId { get; }
	}
}
