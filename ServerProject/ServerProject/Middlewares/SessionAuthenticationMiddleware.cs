using ServerProject.Services;
using System.Security.Claims;

namespace ServerProject.Middlewares
{
	/// <summary>
	/// 中间件，用于基于会话的身份验证
	/// </summary>
	public sealed class SessionAuthenticationMiddleware
	{
		/// <summary>
		/// 下一个请求委托
		/// </summary>
		private readonly RequestDelegate _next;

		/// <summary>
		/// 初始化中间件
		/// </summary>
		/// <param name="next">下一个请求委托</param>
		public SessionAuthenticationMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		/// <summary>
		/// 处理HTTP请求
		/// </summary>
		/// <param name="context"> HTTP上下文</param>
		/// <param name="sessionAuthenticationService"> 会话认证服务</param>
		/// <returns> 一个表示异步操作的任务</returns>
		public async Task InvokeAsync(
			HttpContext context,
			ISessionAuthenticationService sessionAuthenticationService)
		{
			// 如果用户已经通过其他方式认证，直接调用下一个中间件
			if (context.User?.Identity?.IsAuthenticated == true)
			{
				await _next(context);
				return;
			}

			// 从请求头中获取会话ID
			string? sessionIdValue = context.Request.Headers["X-Session-Id"].FirstOrDefault();

			// 如果会话ID为空或无效，直接调用下一个中间件
			if (string.IsNullOrWhiteSpace(sessionIdValue))
			{
				await _next(context);
				return;
			}

			// 尝试将会话ID解析为GUID，如果失败，直接调用下一个中间件
			if (!Guid.TryParse(sessionIdValue, out Guid sessionId))
			{
				await _next(context);
				return;
			}

			// 调用会话认证服务进行认证
			var result = await sessionAuthenticationService.Authenticate(sessionId);

			// 如果认证失败，直接调用下一个中间件
			if (result == null)
			{
				await _next(context);
				return;
			}

			// 创建声明列表
			var claims = new List<Claim>
			{
				new Claim("uid", result.CustomerId.ToString()),
				new Claim("auth_type", "Session"),
				new Claim("session_id", result.SessionId.ToString())
			};

			// 创建声明身份
			var identity = new ClaimsIdentity(
				claims,
				authenticationType: "Session");

			// 将声明身份设置为当前用户
			context.User = new ClaimsPrincipal(identity);

			// 调用下一个中间件
			await _next(context);
		}
	}
}
