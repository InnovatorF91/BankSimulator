using System.Data;

namespace ServerProject.Common
{
    public class DataAccess : IDataAccess
    {
		/// <summary>
		/// 數據庫連接對象
		/// </summary>
		private readonly IDbConnection _conn;

		/// <summary>
		/// 數據庫交易對象
		/// </summary>
		private IDbTransaction? _tx;

		/// <summary>
		/// 是否已經釋放資源
		/// </summary>
		private bool _disposed;

		/// <summary>
		/// 是否使用交易
		/// </summary>
		public bool UseTransaction { get; }

		/// <summary>
		/// 數據庫連接對象
		/// </summary>
		public IDbConnection DbConnection => _conn;

		/// <summary>
		/// 數據庫交易對象
		/// </summary>
		public IDbTransaction DbTransaction =>
			_tx ?? throw new InvalidOperationException(
				"No active transaction.");

		/// <summary>
		/// 初始化 DataAccess 類別的新實例
		/// </summary>
		/// <param name="connection">数据库连接对象</param>
		/// <param name="useTransaction">是否使用交易</param>
		/// <param name="isolationLevel">交易隔離級別</param>
		/// <exception cref="ArgumentNullException"></exception>
		public DataAccess(
			IDbConnection connection,
			bool useTransaction = false,
			IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
		{
			_conn = connection
				?? throw new ArgumentNullException(nameof(connection));

			try
			{
				_conn.Open();

				if (useTransaction)
				{
					_tx = _conn.BeginTransaction(isolationLevel);
				}

				UseTransaction = useTransaction;
			}
			catch
			{
				_conn.Dispose();
				throw;
			}
		}

		/// <summary>
		/// 提交交易
		/// </summary>
		/// <exception cref="InvalidOperationException">没有活动的交易可提交 </exception>
		public void Commit()
		{
			ThrowIfDisposed();

			if (_tx == null)
				throw new InvalidOperationException(
					"No active transaction to commit.");

			var tx = _tx;

			try
			{
				tx.Commit();
			}
			finally
			{
				_tx = null;

				try
				{
					tx.Dispose();
				}
				catch
				{
					// TODO: 后续统一接入日志系统。
					// 不让资源释放异常覆盖 Commit 的结果。
				}
			}
		}

		/// <summary>
		/// 回滚交易
		/// </summary>
		public void Rollback()
		{
			ThrowIfDisposed();

			if (_tx == null)
				return;

			var tx = _tx;

			try
			{
				tx.Rollback();
			}
			finally
			{
				_tx = null;

				try
				{
					tx.Dispose();
				}
				catch
				{
					// TODO: 后续统一接入日志系统。
					// 不让 Dispose 异常覆盖 Rollback 异常。
				}
			}
		}

		/// <summary>
		/// 釋放資源
		/// </summary>
		public void Dispose()
		{
			if (_disposed)
				return;

			try
			{
				_tx?.Dispose();
				_tx = null;
			}
			finally
			{
				_conn.Dispose();
				_disposed = true;
			}

			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// 檢查是否已經釋放資源，如果已經釋放則拋出 ObjectDisposedException
		/// </summary>
		private void ThrowIfDisposed()
		{
			if (_disposed)
				throw new ObjectDisposedException(nameof(DataAccess));
		}
	}
}
