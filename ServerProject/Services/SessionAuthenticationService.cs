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
		/// 客户认证仓储。
		/// </summary>
		private readonly ICustomerAuthRepository _customerAuthRepository;

		/// <summary>
		/// 会话仓储
		/// </summary>
		private readonly ICustomerSessionRepository _customerSessionRepository;

		/// <summary>
		/// 會話選項接口
		/// </summary>
		private readonly ISessionOption _sessionOption;

		/// <summary>
		/// 时间提供者
		/// </summary>
		private readonly ITimeProvider _timeProvider;

		/// <summary>
		/// 日志接口
		/// </summary>
		private readonly ILogger<SessionAuthenticationService> _logger;

		/// <summary>
		/// 初始化会话认证服务
		/// </summary>
		/// <param name="connectionFactory"> 连接工厂 </param>
		/// <param name="customerSessionRepository"> 会话仓储 </param>
		/// <param name="timeProvider"> 时间提供者 </param>
		public SessionAuthenticationService(
			IConnectionFactory connectionFactory,
			ICustomerSessionRepository customerSessionRepository,
			ICustomerAuthRepository customerAuthRepository,
			ISessionOption sessionOption,
			ILogger<SessionAuthenticationService> logger,
			ITimeProvider timeProvider) : base(connectionFactory, logger)
		{
			_customerSessionRepository = customerSessionRepository;
			_customerAuthRepository = customerAuthRepository;
			_sessionOption = sessionOption;
			_timeProvider = timeProvider;
			_logger = logger;
		}

		/// <summary>
		/// 认证会话
		/// </summary>
		/// <param name="sessionId">会话ID</param>
		/// <returns> 会话认证结果，如果认证失败则返回null </returns>
		public async Task<SessionAuthenticationResult?> Authenticate(Guid sessionId)
		{
			// 查询会话。
			var session = await ExecuteDbAsync<CustomerSessionEntity?>(
				async dataAccess =>
				{
					_customerSessionRepository.DataAccess = dataAccess;
					return await _customerSessionRepository.GetSessionById(sessionId);
				}
			);

			// 会话不存在或已失效。
			if (session == null || !session.IsValid)
			{
				return null;
			}

			var now = _timeProvider.Now();

			// 会话已过期。
			if (session.ExpiredAt <= now 
				|| session.LastSeenAt.Add(_sessionOption.IdleTimeout) <= now
	            || session.CreatedAt.Add(_sessionOption.AbsoluteLifetime) <= now)
			{
				return null;
			}

			// 查询会话所属账号的认证信息。
			var auth = await ExecuteDbAsync<CustomerAuthEntity?>(
				async dataAccess =>
				{
					_customerAuthRepository.DataAccess = dataAccess;
					return await _customerAuthRepository.GetAuthByCustomerId(
						session.UserId);
				}
			);

			// 认证记录不存在或已删除。
			if (auth == null || auth.IsDeleted)
			{
				return null;
			}

			// 返回客户身份及数据库中的管理员标记。
			return new SessionAuthenticationResult
			{
				CustomerId = session.UserId,
				SessionId = session.SessionId,
				IsAdmin = auth.IsAdmin
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

		/// <summary>
		/// 是否为管理员。
		/// </summary>
		public bool IsAdmin { get; init; }
	}
}
