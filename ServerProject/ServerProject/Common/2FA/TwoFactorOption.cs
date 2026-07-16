namespace ServerProject.Common
{
	/// <summary>
	/// 雙重認證選項配置
	/// </summary>
	public class TwoFactorOption : ITwoFactorOption
	{
		/// <summary>
		/// 雙重認證待定過期時間
		/// </summary>
		public TimeSpan TwoFactorPendingExpiresAt => TimeSpan.FromMinutes(10);
	}

	/// <summary>
	/// 雙重認證選項接口
	/// </summary>
	public interface ITwoFactorOption
	{
		/// <summary>
		/// 雙重認證待定過期時間
		/// </summary>
		TimeSpan TwoFactorPendingExpiresAt { get; }
	}
}
