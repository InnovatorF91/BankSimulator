using System.Data.Common;

namespace ServerProject.Common
{
	/// <summary>
	/// 邏輯基類：統一管理 DB 連線/交易/例外翻譯
	/// </summary>
	public class ServiceBase
	{
		protected readonly IConnectionFactory _connectionFactory;

		public ServiceBase(IConnectionFactory connectionFactory)
		{
			_connectionFactory = connectionFactory;
		}

		/// <summary>
		/// 建立 IDataAccess（是否開啟交易由參數決定）
		/// </summary>
		protected virtual Task<IDataAccess> CreateConnectionAsync(bool transaction = false)
		{
			// Task.Run 沒必要：這裡只是建立物件/取得連線
			var conn = _connectionFactory.GetConnection(transaction);
			var da = new DataAccess(conn, _connectionFactory.Transaction);
			return Task.FromResult<IDataAccess>(da);
		}

		/// <summary>
		/// 不使用交易：統一翻譯 DB 例外為 UnableToOperateDBException
		/// </summary>
		protected async Task<T> ExecuteDbAsync<T>(
			Func<IDataAccess, Task<T>> action,
			string operationName = "DB operation")
		{
			using var da = await CreateConnectionAsync(transaction: false);

			try
			{
				return await action(da);
			}
			catch (DbException ex)
			{
				throw new UnableToOperateDBException($"{operationName} failed.", ex);
			}
			catch (TimeoutException ex)
			{
				throw new UnableToOperateDBException($"{operationName} timed out.", ex);
			}
		}

		/// <summary>
		/// 使用交易：自動 Commit/Rollback，並統一翻譯 DB 例外
		/// </summary>
		protected async Task<T> ExecuteInTxAsync<T>(
			Func<IDataAccess, Task<T>> action,
			string operationName = "DB transaction")
		{
			using var da = await CreateConnectionAsync(transaction: true);

			try
			{
				var result = await action(da);

				da.Commit();
				return result;
			}
			catch (DbException ex)
			{
				da.Rollback();
				throw new UnableToOperateDBException($"{operationName} failed.", ex);
			}
			catch (TimeoutException ex)
			{
				da.Rollback();
				throw new UnableToOperateDBException($"{operationName} timed out.", ex);
			}
			catch
			{
				// 非 DB 類例外也要 rollback，避免交易掛著
				da.Rollback();
				throw;
			}
		}

		/// <summary>
		/// void 版本：不使用交易
		/// </summary>
		protected Task ExecuteDbAsync(
			Func<IDataAccess, Task> action,
			string operationName = "DB operation")
			=> ExecuteDbAsync(async da => { await action(da); return true; }, operationName);

		/// <summary>
		/// void 版本：使用交易
		/// </summary>
		protected Task ExecuteInTxAsync(
			Func<IDataAccess, Task> action,
			string operationName = "DB transaction")
			=> ExecuteInTxAsync(async da => { await action(da); return true; }, operationName);
	}
}
