using Dapper;
using ServerProject.Common;
using ServerProject.Entities;

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
            return await _dataAccess.DbConnection.QuerySingleOrDefaultAsync<TransactionEntity>(sql, new { TransactionId = transactionId });
		}

        /// <summary>
        /// 通過帳戶ID獲取該帳戶的所有交易記錄
        /// </summary>
        /// <param name="accountId">帳戶ID</param>
        /// <returns>交易模型列表</returns>
        public async Task<List<TransactionEntity>> GetTransactionsByAccountId(long accountId)
        {
            var sql = LoadSql("Transaction.GetTransactionsByAccountId");

            var transactions = await _dataAccess.DbConnection.QueryAsync<TransactionEntity>(sql, new { AccountId = accountId },_dataAccess.DbTransaction);

            return transactions.ToList();
		}

        /// <summary>
        /// 插入新的交易記錄
        /// </summary>
        /// <param name="transaction">交易模型</param>
        /// <returns>新插入交易的ID</returns>
        public async Task<long> InsertTransaction(TransactionEntity transaction)
        {
            var sql = LoadSql("Transaction.InsertTransaction");

            return await _dataAccess.DbConnection.ExecuteScalarAsync<long>(sql, transaction);
		}

        /// <summary>
        /// 通過交易ID獲取該交易的金額變化
        /// </summary>
        /// <param name="transactionId">交易ID</param>
        /// <returns>交易金額變化</returns>
        public async Task<decimal> GetAmountDeltaByTransactionId(long transactionId)
        {
            var sql = LoadSql("Transaction.GetAmountDeltaByTransactionId");

            return await _dataAccess.DbConnection.ExecuteScalarAsync<decimal>(sql, new { TransactionId = transactionId });
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
        /// 通過帳戶ID獲取該帳戶的所有交易記錄
        /// </summary>
        /// <param name="accountId">帳戶ID</param>
        /// <returns>交易模型列表</returns>
        Task<List<TransactionEntity>> GetTransactionsByAccountId(long accountId);

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
    }
}
