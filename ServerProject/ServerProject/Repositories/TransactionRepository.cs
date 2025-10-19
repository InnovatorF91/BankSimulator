using Dapper;
using ServerProject.Common;
using ServerProject.Models;

namespace ServerProject.Repositories
{
    /// <summary>
    /// 交易服務實現類
    /// </summary>
    public class TransactionRepository : ITransactionRepository
    {
        /// <summary>
        /// 查詢語句：通過交易ID獲取交易信息
        /// </summary>
        private readonly string queryGetTransactionById = "SELECT * FROM \"Transactions\" WHERE transaction_id = @TransactionId";

        /// <summary>
        /// 查詢語句：通過帳戶ID獲取該帳戶的所有交易記錄
        /// </summary>
        private readonly string queryGetTransactionsByAccountId = "SELECT * FROM \"Transactions\" WHERE account_id = @AccountId";

        /// <summary>
        /// 插入語句：插入新的交易記錄
        /// </summary>
        private readonly string queryInsertTransaction = "INSERT INTO \"Transactions\" " +
            "(account_id, transaction_type, amount_delta, related_account, create_at, status, note) " +
            "VALUES " +
            "(@AccountId, @TransactionType, @AmountDelta, @RelatedAccount, @CreateAt, @Status, @Note) " +
            "RETURNING transaction_id";

        private readonly string queryGetAmountDeltaByTransactionId = "SELECT amount_delta FROM \"Transactions\" WHERE transaction_id = @TransactionId";

        private IDataAccess _dataAccess;

        public IDataAccess DataAccess { set => throw new NotImplementedException(); }

        public TransactionRepository(IDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }

        /// <summary>
        /// 通過交易ID獲取交易信息
        /// </summary>
        /// <param name="transactionId">交易ID</param>
        /// <returns>交易模型</returns>
        public async Task<TransactionModel> GetTransactionById(long transactionId)
        {
            // 使用Dapper執行查詢，並將結果映射到TransactionModel對象
            var transaction = await _dataAccess.DbConnection.QueryFirstOrDefaultAsync<TransactionModel>(queryGetTransactionById, new { TransactionId = transactionId });

            // 如果未找到交易，則拋出異常
            if (transaction == null)
            {
                throw new InvalidOperationException($"Transaction with ID {transactionId} not found.");
            }

            // 返回查詢到的交易模型
            return transaction;
        }

        /// <summary>
        /// 通過帳戶ID獲取該帳戶的所有交易記錄
        /// </summary>
        /// <param name="accountId">帳戶ID</param>
        /// <returns>交易模型列表</returns>
        public async Task<List<TransactionModel>> GetTransactionsByAccountId(long accountId)
        {
            // 使用Dapper執行查詢，並將結果映射到TransactionModel對象列表
            var transactions = await _dataAccess.DbConnection.QueryAsync<TransactionModel>(queryGetTransactionsByAccountId, new { AccountId = accountId });

            // 將結果轉換為列表並返回
            return transactions.ToList();
        }

        /// <summary>
        /// 插入新的交易記錄
        /// </summary>
        /// <param name="transaction">交易模型</param>
        /// <returns>新插入交易的ID</returns>
        public async Task<long> InsertTransaction(TransactionModel transaction)
        {
            // 使用Dapper執行插入操作，並返回新插入交易的ID
            var transactionId = await _dataAccess.DbConnection.QuerySingleAsync(queryInsertTransaction, transaction);

            // 返回新插入交易的ID
            return transactionId.transaction_id;
        }

        /// <summary>
        /// 通過交易ID獲取該交易的金額變化
        /// </summary>
        /// <param name="transactionId">交易ID</param>
        /// <returns>交易金額變化</returns>
        public async Task<decimal> GetAmountDeltaByTransactionId(long transactionId)
        {
            // 使用Dapper執行查詢，並返回交易的金額變化
            return await _dataAccess.DbConnection.QuerySingleAsync<decimal>(queryGetAmountDeltaByTransactionId, new { TransactionId = transactionId });
        }
    }

    /// <summary>
    /// 交易服務接口
    /// </summary>
    public interface ITransactionRepository
    {
        /// <summary>
        /// 數據訪問對象
        /// </summary>
        IDataAccess DataAccess { set; }

        /// <summary>
        /// 通過交易ID獲取交易信息
        /// </summary>
        /// <param name="transactionId">交易ID</param>
        /// <returns>交易模型</returns>
        Task<TransactionModel> GetTransactionById(long transactionId);

        /// <summary>
        /// 通過帳戶ID獲取該帳戶的所有交易記錄
        /// </summary>
        /// <param name="accountId">帳戶ID</param>
        /// <returns>交易模型列表</returns>
        Task<List<TransactionModel>> GetTransactionsByAccountId(long accountId);

        /// <summary>
        /// 插入新的交易記錄
        /// </summary>
        /// <param name="transaction">交易模型</param>
        /// <returns>新插入交易的ID</returns>
        Task<long> InsertTransaction(TransactionModel transaction);

        /// <summary>
        /// 通過交易ID獲取該交易的金額變化
        /// </summary>
        /// <param name="transactionId">交易ID</param>
        /// <returns>交易金額變化</returns>
        Task<decimal> GetAmountDeltaByTransactionId(long transactionId);
    }
}
