namespace ServerProject.Common
{
	/// <summary>
	/// 数据库死锁异常
	/// </summary>
	public class DbDeadlockException
		: UnableToOperateDBException
	{
		public DbDeadlockException(
			string message,
			Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
