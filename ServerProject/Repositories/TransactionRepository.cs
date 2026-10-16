using Dapper;
using ServerProject.Common;
using ServerProject.Entities;
using ShareProject.Common;

namespace ServerProject.Repositories
{
    /// <summary>
    /// 交易服務實現類
    /// </summary>
    public class TransactionRepository : RepositoryBase, ITransactionRepository
    {
		public TransactionRepository(IDataAccess dataAccess) : base(dataAccess)
        {
		}

		/// <summary>
		/// 通過交易ID獲取交易信息
		/// </summary>
		/// <param name="transactionId">交易ID</param>
		/// <returns>交易模型</returns>
		public async Task<TransactionEntity?> GetTransactionById(long transactionId)
        {
            var sql = LoadSql("Transaction.GetTransactionById");
            return await _dataAccess.DbConnection.QuerySingleOrDefaultAsync<TransactionEntity>(sql, new { TransactionId = transactionId }, _dataAccess.DbTransaction);
		}

        /// <summary>
        /// 插入新的交易記錄
        /// </summary>
        /// <param name="transaction">交易模型</param>
        /// <returns>新插入交易的ID</returns>
        public async Task<long> InsertTransaction(TransactionEntity transaction)
        {
            var sql = LoadSql("Transaction.InsertTransaction");

            return await _dataAccess.DbConnection.ExecuteScalarAsync<long>(sql, transaction, _dataAccess.DbTransaction);
		}

        /// <summary>
        /// 通過交易ID獲取該交易的金額變化
        /// </summary>
        /// <param name="transactionId">交易ID</param>
        /// <returns>交易金額變化</returns>
        public async Task<decimal> GetAmountDeltaByTransactionId(long transactionId)
        {
            var sql = LoadSql("Transaction.GetAmountDeltaByTransactionId");

            return await _dataAccess.DbConnection.ExecuteScalarAsync<decimal>(sql, new { TransactionId = transactionId }, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 通过交易ID和账户ID取得交易记录
		/// </summary>
		/// <param name="transactionId">交易ID</param>
		/// <param name="accountId">账户ID</param>
		/// <returns>交易模型</returns>
		public async Task<TransactionEntity?> GetTransactionByIdAndAccountId(long transactionId, long accountId)
		{
			var sql = LoadSql("Transaction.GetTransactionByIdAndAccountId");

			return await _dataAccess.DbConnection.QuerySingleOrDefaultAsync<TransactionEntity>(sql,new{TransactionId = transactionId,AccountId = accountId},_dataAccess.DbTransaction);
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
			var sql = LoadSql("Transaction.GetTransactionsByAccountId");

			var transactions = await _dataAccess.DbConnection.QueryAsync<TransactionEntity>(sql,new{AccountId = accountId,From = from,To = to,TransactionType = transactionType,Status = status},_dataAccess.DbTransaction);

			return transactions.ToList();
		}

		/// <summary>
		/// 通过组队ID取得交易信息
		/// </summary>
		/// <param name="groupId">组队ID</param>
		/// <returns>交易模型组成的表</returns>
		public async Task<List<TransactionEntity>> GetTransactionsByGroupId(Guid groupId)
		{
			var sql = LoadSql("Transaction.GetTransactionsByGroupId");

			var transactions = await _dataAccess.DbConnection.QueryAsync<TransactionEntity>(sql,new{GroupId = groupId},_dataAccess.DbTransaction);

			return transactions.ToList();
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
