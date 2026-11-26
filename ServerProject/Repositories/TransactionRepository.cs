using Dapper;
using ServerProject.Common;
using ServerProject.Entities;
using ShareProject.Common;
using System.Diagnostics;

namespace ServerProject.Repositories
{
    /// <summary>
    /// 交易服務實現類
    /// </summary>
    public class TransactionRepository : RepositoryBase, ITransactionRepository
    {
		/// <summary>
		/// 日志接口
		/// </summary>
		private readonly ILogger<TransactionRepository> _logger;

		public TransactionRepository(IDataAccess dataAccess, ILogger<TransactionRepository> logger) : base(dataAccess)
		{
			_logger = logger;
		}

		/// <summary>
		/// 通過交易ID獲取交易信息
		/// </summary>
		/// <param name="transactionId">交易ID</param>
		/// <returns>交易模型</returns>
		public async Task<TransactionEntity?> GetTransactionById(long transactionId)
        {
			var stopwatch = Stopwatch.StartNew();
			var outcome = LogOutcome.Failed;

			_logger.LogStart(level: LogLevel.Debug);

			try
			{
				var sql = LoadSql("Transaction.GetTransactionById");
				var transaction = await _dataAccess.DbConnection.QuerySingleOrDefaultAsync<TransactionEntity>(sql, new { TransactionId = transactionId }, _dataAccess.DbTransaction);

				outcome = LogOutcome.Completed;
				return transaction;
			}
			finally
			{
				_logger.LogEnd(outcome, stopwatch.ElapsedMilliseconds, level: LogLevel.Debug);
			}
		}

        /// <summary>
        /// 插入新的交易記錄
        /// </summary>
        /// <param name="transaction">交易模型</param>
        /// <returns>新插入交易的ID</returns>
        public async Task<long> InsertTransaction(TransactionEntity transaction)
        {
			var stopwatch = Stopwatch.StartNew();
			var outcome = LogOutcome.Failed;

			_logger.LogStart(level: LogLevel.Debug);

			try
			{
				var sql = LoadSql("Transaction.InsertTransaction");
				var transacionId = await _dataAccess.DbConnection.ExecuteScalarAsync<long>(sql, transaction, _dataAccess.DbTransaction);

				outcome = transacionId > 0 ? LogOutcome.Completed : LogOutcome.Failed;
				return transacionId;
			}
			finally
			{
				_logger.LogEnd(outcome, stopwatch.ElapsedMilliseconds, level: LogLevel.Debug);
			}
		}

        /// <summary>
        /// 通過交易ID獲取該交易的金額變化
        /// </summary>
        /// <param name="transactionId">交易ID</param>
        /// <returns>交易金額變化</returns>
        public async Task<decimal> GetAmountDeltaByTransactionId(long transactionId)
        {
			var stopwatch = Stopwatch.StartNew();
			var outcome = LogOutcome.Failed;

			_logger.LogStart(level: LogLevel.Debug);

			try
			{
				var sql = LoadSql("Transaction.GetAmountDeltaByTransactionId");
				var amountDelta = await _dataAccess.DbConnection.ExecuteScalarAsync<decimal>(sql, new { TransactionId = transactionId }, _dataAccess.DbTransaction);

				outcome = LogOutcome.Completed;
				return amountDelta;
			}
			finally
			{
				_logger.LogEnd(outcome, stopwatch.ElapsedMilliseconds, level: LogLevel.Debug);
			}
		}

		/// <summary>
		/// 通过交易ID和账户ID取得交易记录
		/// </summary>
		/// <param name="transactionId">交易ID</param>
		/// <param name="accountId">账户ID</param>
		/// <returns>交易模型</returns>
		public async Task<TransactionEntity?> GetTransactionByIdAndAccountId(long transactionId, long accountId)
		{
			var stopwatch = Stopwatch.StartNew();
			var outcome = LogOutcome.Failed;

			_logger.LogStart(level: LogLevel.Debug);

			try
			{
				var sql = LoadSql("Transaction.GetTransactionByIdAndAccountId");
				var transaction = await _dataAccess.DbConnection.QuerySingleOrDefaultAsync<TransactionEntity>(sql, new { TransactionId = transactionId, AccountId = accountId }, _dataAccess.DbTransaction);

				outcome = LogOutcome.Completed;
				return transaction;
			}
			finally
			{
				_logger.LogEnd(outcome, stopwatch.ElapsedMilliseconds, level: LogLevel.Debug);
			}
		}

		/// <summary>
		/// 通过账户ID取得该账户下所有的交易信息
		/// </summary>
		/// <param name="accountId">账户ID</param>
		/// <param name="from">开始时间</param>
		/// <param name="to">截至时间</param>
		/// <param name="transactionType">交易类型</param>
		/// <param name="status">交易状态</param>
		/// <returns>交易模型组成的表</returns>
		public async Task<List<TransactionEntity>> GetTransactionsByAccountId(long accountId, DateTime? from, DateTime? to, TransactionType? transactionType, TransactionStatus? status)
		{
			var stopwatch = Stopwatch.StartNew();
			var outcome = LogOutcome.Failed;

			_logger.LogStart(level: LogLevel.Debug);

			try
			{
				var sql = LoadSql("Transaction.GetTransactionsByAccountId");
				var transactions = (await _dataAccess.DbConnection.QueryAsync<TransactionEntity>(sql, new { AccountId = accountId, From = from, To = to, TransactionType = transactionType, Status = status }, _dataAccess.DbTransaction)).AsList();

				outcome = LogOutcome.Completed;
				return transactions;
			}
			finally
			{
				_logger.LogEnd(outcome, stopwatch.ElapsedMilliseconds, level: LogLevel.Debug);
			}
		}

		/// <summary>
		/// 通过组队ID取得交易信息
		/// </summary>
		/// <param name="groupId">组队ID</param>
		/// <returns>交易模型组成的表</returns>
		public async Task<List<TransactionEntity>> GetTransactionsByGroupId(Guid groupId)
		{
			var stopwatch = Stopwatch.StartNew();
			var outcome = LogOutcome.Failed;

			_logger.LogStart(level: LogLevel.Debug);

			try
			{
				var sql = LoadSql("Transaction.GetTransactionsByGroupId");
				var transactions = (await _dataAccess.DbConnection.QueryAsync<TransactionEntity>(sql, new { GroupId = groupId }, _dataAccess.DbTransaction)).AsList();

				outcome = LogOutcome.Completed;
				return transactions;
			}
			finally
			{
				_logger.LogEnd(outcome, stopwatch.ElapsedMilliseconds, level: LogLevel.Debug);
			}
		}
	}

    /// <summary>
    /// 交易服務接口
    /// </summary>
    public interface ITransactionRepository
    {
		/// <summary>
		/// 數據訪問接口屬性
		/// </summary>
		IDataAccess DataAccess { set; }

		/// <summary>
		/// 通過交易ID獲取交易信息
		/// </summary>
		/// <param name="transactionId">交易ID</param>
		/// <returns>交易模型</returns>
		Task<TransactionEntity?> GetTransactionById(long transactionId);

        /// <summary>
        /// 插入新的交易記錄
        /// </summary>
        /// <param name="transaction">交易模型</param>
        /// <returns>新插入交易的ID</returns>
        Task<long> InsertTransaction(TransactionEntity transaction);

        /// <summary>
        /// 通過交易ID獲取該交易的金額變化
        /// </summary>
        /// <param name="transactionId">交易ID</param>
        /// <returns>交易金額變化</returns>
        Task<decimal> GetAmountDeltaByTransactionId(long transactionId);

        /// <summary>
        /// 通过交易ID和账户ID取得交易记录
        /// </summary>
        /// <param name="transactionId">交易ID</param>
        /// <param name="accountId">账户ID</param>
        /// <returns>交易模型</returns>
		Task<TransactionEntity?> GetTransactionByIdAndAccountId(long transactionId,long accountId);

        /// <summary>
        /// 通过账户ID取得该账户下所有的交易信息
        /// </summary>
        /// <param name="accountId">账户ID</param>
        /// <param name="from">开始时间</param>
        /// <param name="to">截至时间</param>
        /// <param name="transactionType">交易类型</param>
        /// <param name="status">交易状态</param>
        /// <returns>交易模型组成的表</returns>
		Task<List<TransactionEntity>> GetTransactionsByAccountId(long accountId,DateTime? from,DateTime? to,TransactionType? transactionType,TransactionStatus? status);

		/// <summary>
		/// 通过组队ID取得交易信息
		/// </summary>
		/// <param name="groupId">组队ID</param>
		/// <returns>交易模型组成的表</returns>
		Task<List<TransactionEntity>> GetTransactionsByGroupId(Guid groupId);
	}
}
