namespace ServerProject.Common
{
	/// <summary>
	/// 會話選項配置
	/// </summary>
	public class SessionOption : ISessionOption
	{
		public SessionOption()
		{
		}

		/// <summary>
		/// 絕對存活時間
		/// </summary>
		public TimeSpan AbsoluteLifetime => TimeSpan.FromHours(5);

		/// <summary>
		/// 閒置超時時間
		/// </summary>
		public TimeSpan IdleTimeout => TimeSpan.FromMinutes(30);
	}

	/// <summary>
	/// 會話選項接口
	/// </summary>
	public interface ISessionOption
	{
		/// <summary>
		/// 絕對存活時間
		/// </summary>
		TimeSpan AbsoluteLifetime { get; }

		/// <summary>
		/// 閒置超時時間
		/// </summary>
		TimeSpan IdleTimeout { get; }
	}
}
