using ServerProject.Common;
using ServerProject.Entities;
using ServerProject.Repositories;

namespace ServerProject.Services
{
	/// <summary>
	/// 会话认证服务
	/// </summary>
	public sealed class SessionAuthenticationService : ServiceBase , ISessionAuthenticationService
	{
		/// <summary>
		/// 会话仓储
		/// </summary>
		private readonly ICustomerSessionRepository _customerSessionRepository;

		/// <summary>
		/// 时间提供者
		/// </summary>
		private readonly ITimeProvider _timeProvider;

		/// <summary>
		/// 初始化会话认证服务
		/// </summary>
		/// <param name="connectionFactory"> 连接工厂 </param>
		/// <param name="customerSessionRepository"> 会话仓储 </param>
		/// <param name="timeProvider"> 时间提供者 </param>
		public SessionAuthenticationService(
			IConnectionFactory connectionFactory,
			ICustomerSessionRepository customerSessionRepository,
			ITimeProvider timeProvider) : base(connectionFactory)
		{
			_customerSessionRepository = customerSessionRepository;
			_timeProvider = timeProvider;
		}

		/// <summary>
		/// 认证会话
		/// </summary>
		/// <param name="sessionId">会话ID</param>
		/// <returns> 会话认证结果，如果认证失败则返回null </returns>
		public async Task<SessionAuthenticationResult?> Authenticate(Guid sessionId)
		{
			// 获取会话信息
			var session = await ExecuteDbAsync<CustomerSessionEntity?>(
				async dataAccess =>
				{
					_customerSessionRepository.DataAccess = dataAccess;
					return await _customerSessionRepository.GetSessionById(sessionId);
				}
			);

			// 如果会话不存在或无效，返回null
			if (session == null)
			{
				return null;
			}

			// 如果会话无效，返回null
			if (!session.IsValid)
			{
				return null;
			}

			// 获取当前时间
			var now = _timeProvider.Now();

			// 如果会话已过期，返回null
			if (session.ExpiredAt < now)
			{
				return null;
			}

			// 返回会话认证结果
			return new SessionAuthenticationResult
			{
				CustomerId = session.UserId,
				SessionId = session.SessionId
			};
		}
	}

	/// <summary>
	/// 会话认证服务接口
	/// </summary>
	public interface ISessionAuthenticationService
	{
		/// <summary>
		/// 认证会话
		/// </summary>
		/// <param name="sessionId">会话ID</param>
		/// <returns>会话认证结果，如果认证失败则返回null</returns>
		Task<SessionAuthenticationResult?> Authenticate(Guid sessionId);
	}

	/// <summary>
	/// 会话认证结果
	/// </summary>
	public sealed class SessionAuthenticationResult
	{
		/// <summary>
		/// 客户ID
		/// </summary>
		public int CustomerId { get; init; }

		/// <summary>
		/// 会话ID
		/// </summary>
		public Guid SessionId { get; init; }
	}
}
