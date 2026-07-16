namespace ShareProject.Common
{
	/// <summary>
	/// 交易狀態
	/// </summary>
	public enum TransactionStatus
	{
		Pending, // 待處理
		Completed, // 已完成
		Failed, // 失敗
		Reversed, // 已撤銷
	}
}
