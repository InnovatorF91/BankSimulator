using Npgsql;
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

		protected virtual Task<IDataAccess> CreateConnectionAsync(bool transaction = false)
		{
			var conn = _connectionFactory.GetConnection();

			IDataAccess da = new DataAccess(
				conn,
				useTransaction: transaction);

			return Task.FromResult(da);
		}

		/// <summary>
		/// 不使用交易：統一翻譯 DB 例外為 UnableToOperateDBException
		/// </summary>
		protected async Task<T> ExecuteDbAsync<T>(
			Func<IDataAccess, Task<T>> action,
			string operationName = "DB operation")
		{
			IDataAccess? da= null; // 初始化為 null，避免編譯器警告
			try
			{
				da = await CreateConnectionAsync(transaction: false);

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
			finally
			{
				da?.Dispose();
			}
		}

		/// <summary>
		/// 使用交易：自動 Commit/Rollback，並統一翻譯 DB 例外
		/// </summary>
		protected async Task<T> ExecuteInTxAsync<T>(
			Func<IDataAccess, Task<T>> action,
			string operationName = "DB transaction")
		{
			IDataAccess? da = null;

			try
			{
				da = await CreateConnectionAsync(transaction: true);

				var result = await action(da);

				da.Commit();

				return result;
			}
			catch (PostgresException ex)
				when (ex.SqlState == PostgresErrorCodes.DeadlockDetected)
			{
				if (da != null)
					TryRollback(da);

				throw new DbDeadlockException(
					$"{operationName} failed due to a database deadlock.",
					ex);
			}
			catch (DbException ex)
			{
				if (da != null)
					TryRollback(da);

				throw new UnableToOperateDBException(
					$"{operationName} failed.", ex);
			}
			catch (TimeoutException ex)
			{
				if (da != null)
					TryRollback(da);

				throw new UnableToOperateDBException(
					$"{operationName} timed out.", ex);
			}
			catch
			{
				if (da != null)
					TryRollback(da);

				throw;
			}
			finally
			{
				da?.Dispose();
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

		/// <summary>
		/// 嘗試回滾交易，避免覆蓋原始例外
		/// </summary>
		/// <param name="da">IDataAccess 實例</param>
		private static void TryRollback(IDataAccess da)
		{
			try
			{
				da.Rollback();
			}
			catch
			{
				// 避免回滚异常覆盖原始异常。
				// TODO: 后续统一接入日志系统。
			}
		}
	}
}
