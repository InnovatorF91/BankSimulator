using Dapper;
using ServerProject.Common;
using ServerProject.Entities;
using ShareProject.Common;

namespace ServerProject.Repositories
{
    /// <summary>
    /// 卡片服務實現類
    /// </summary>
    public class CardRepository : RepositoryBase, ICardRepository
    {
		public CardRepository(IDataAccess dataAccess) : base(dataAccess)
		{
		}

		/// <summary>
		/// 停用帳戶下的所有卡片
		/// </summary>
		/// <param name="accountId">賬戶ID</param>
		/// <param name="status">新的卡片狀態</param>
		/// <param name="deactivatedAt">卡片被停用的時間</param>
		/// <param name="updatedAt">更新時間</param>
		public async Task<int> DeactivateAllCards(long accountId, CardStatus status, DateTime deactivatedAt, DateTime updatedAt)
        {
            var sql = LoadSql("Card.DeactivateAllCards");

            return await _dataAccess.DbConnection.ExecuteAsync(sql, new
            {
                AccountId = accountId,
                Status = status,
                DeactivatedAt = deactivatedAt,
                UpdatedAt = updatedAt
			}, _dataAccess.DbTransaction);
		}

        /// <summary>
        /// 通過卡片ID獲取卡片信息
        /// </summary>
        /// <param name="cardId">卡片ID</param>
        /// <returns>卡片模型</returns>
        public async Task<CardEntity?> GetCardById(long cardId)
        {
            var sql = LoadSql("Card.GetCardById");
            return await _dataAccess.DbConnection.QuerySingleOrDefaultAsync<CardEntity>(sql, new { CardId = cardId }, _dataAccess.DbTransaction);
		}

        /// <summary>
        /// 通過帳戶ID獲取該帳戶的所有卡片
        /// </summary>
        /// <param name="accountId">帳戶ID</param>
        /// <returns>卡片模型列表</returns>
        public async Task<List<CardEntity>> GetCardsByAccountId(long accountId)
        {
            var sql = LoadSql("Card.GetCardsByAccountId");
            var cards = await _dataAccess.DbConnection.QueryAsync<CardEntity>(sql, new { AccountId = accountId }, _dataAccess.DbTransaction);

            return cards.ToList();
		}

		/// <summary>
		/// 获取账户下 Active 状态的卡片数量
		/// </summary>
		/// <param name="accountId">账户ID</param>
		/// <returns>Active 状态的卡片数量</returns>
		public async Task<int> CountActiveCards(long accountId)
        {
            var sql = LoadSql("Card.CountActiveCards");
            return await _dataAccess.DbConnection.ExecuteScalarAsync<int>(sql, new { AccountId = accountId }, _dataAccess.DbTransaction);
		}

        /// <summary>
        /// 插入新卡片
        /// </summary>
        /// <param name="card">卡片模型</param>
        /// <returns>新插入卡片的ID</returns>
        public async Task<long> InsertNewCard(CardEntity card)
        {
            var sql = LoadSql("Card.InsertNewCard");
            return await _dataAccess.DbConnection.ExecuteScalarAsync<long>(sql, card, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 更新卡片PIN碼
		/// </summary>
		/// <param name="cardId">卡片ID</param>
		/// <param name="newPINHash">新的PIN哈希</param>
		/// <param name="updatedAt">更新時間</param>
		public async Task<int> UpdateCardPIN(long cardId, string newPINHash, DateTime updatedAt)
        {
            var sql = LoadSql("Card.UpdateCardPIN");
            return await _dataAccess.DbConnection.ExecuteAsync(sql, new
            {
                CardId = cardId,
                PINHash = newPINHash,
                UpdatedAt = updatedAt
			}, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 更新卡片狀態
		/// </summary>
		/// <param name="cardId">卡片ID</param>
		/// <param name="currentStatus">當前卡片狀態</param>
		/// <param name="newStatus">新的卡片狀態</param>
		/// <param name="updatedAt">更新時間</param>
		public async Task<int> UpdateCardStatusIfCurrentStatus(long cardId,CardStatus currentStatus,CardStatus newStatus,DateTime updatedAt)
        {
            var sql = LoadSql("Card.UpdateCardStatusIfCurrentStatus");
            return await _dataAccess.DbConnection.ExecuteAsync(sql, new
            {
				CardId = cardId,
				CurrentStatus = currentStatus,
				NewStatus = newStatus,
				UpdatedAt = updatedAt
			}, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 更新PIN碼失敗次數及鎖定時間
		/// </summary>
		/// <param name="cardId">卡片ID</param>
		/// <param name="newFailCount">新的失敗次數</param>
		/// <param name="lockedUntil">鎖定到期時間（如果有的話）</param>
		/// <param name="updatedAt">更新時間</param>
		public async Task<int> UpdatePINFailCount(long cardId, short newFailCount,DateTime updatedAt, DateTime? lockedUntil = null)
        {
            var sql = LoadSql("Card.UpdateCardPINFailCount");
            return await _dataAccess.DbConnection.ExecuteAsync(sql, new
            {
                CardId = cardId,
                PINFailCount = newFailCount,
                PINLockedUntil = lockedUntil,
                UpdatedAt = updatedAt
			}, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 获取卡片信息用于更新操作（带锁）
		/// </summary>
		/// <param name="cardId">卡片ID</param>
		/// <returns>卡片模型</returns>
		public async Task<CardEntity?> GetCardByIdForUpdate(long cardId)
		{
			var sql = LoadSql("Card.GetCardByIdForUpdate");
			return await _dataAccess.DbConnection.QuerySingleOrDefaultAsync<CardEntity>(sql, new { CardId = cardId }, _dataAccess.DbTransaction);

		}
	}

    /// <summary>
    /// 卡片服務接口
    /// </summary>
    public interface ICardRepository
    {
        /// <summary>
        /// 數據訪問對象
        /// </summary>
        IDataAccess DataAccess { set; }

        /// <summary>
        /// 通過卡片ID獲取卡片信息
        /// </summary>
        /// <param name="cardId">卡片ID</param>
        /// <returns>卡片模型</returns>
        Task<CardEntity?> GetCardById(long cardId);

        /// <summary>
        /// 通過帳戶ID獲取該帳戶的所有卡片
        /// </summary>
        /// <param name="accountId">帳戶ID</param>
        /// <returns>卡片模型列表</returns>
        Task<List<CardEntity>> GetCardsByAccountId(long accountId);

        /// <summary>
        /// 插入新卡片
        /// </summary>
        /// <param name="card">卡片模型</param>
        /// <returns>新插入卡片的ID</returns>
        Task<long> InsertNewCard(CardEntity card);

		/// <summary>
		/// 更新卡片狀態
		/// </summary>
		/// <param name="cardId">卡片ID</param>
		/// <param name="currentStatus">當前卡片狀態</param>
		/// <param name="newStatus">新的卡片狀態</param>
		/// <param name="updatedAt">更新時間</param>
		Task<int> UpdateCardStatusIfCurrentStatus(long cardId, CardStatus currentStatus, CardStatus newStatus, DateTime updatedAt);

		/// <summary>
		/// 更新卡片PIN碼
		/// </summary>
		/// <param name="cardId">卡片ID</param>
		/// <param name="newPINHash">新的PIN哈希</param>
		/// <param name="updatedAt">更新時間</param>
		Task<int> UpdateCardPIN(long cardId, string newPINHash, DateTime updatedAt);

		/// <summary>
		/// 更新PIN碼失敗次數及鎖定時間
		/// </summary>
		/// <param name="cardId">卡片ID</param>
		/// <param name="newFailCount">新的失敗次數</param>
		/// <param name="lockedUntil">鎖定到期時間（如果有的話）</param>
		/// <param name="updatedAt">更新時間</param>
		Task<int> UpdatePINFailCount(long cardId, short newFailCount, DateTime updatedAt, DateTime? lockedUntil = null);

		/// <summary>
		/// 停用帳戶下的所有卡片
		/// </summary>
		/// <param name="accountId">賬戶ID</param>
		/// <param name="status">新的卡片狀態</param>
		/// <param name="deactivatedAt">卡片被停用的時間</param>
		/// <param name="updatedAt">更新時間</param>
		Task<int> DeactivateAllCards(long accountId, CardStatus status, DateTime deactivatedAt ,DateTime updatedAt);

		/// <summary>
		/// 获取账户下 Active 状态的卡片数量
		/// </summary>
		/// <param name="accountId">账户ID</param>
		Task<int> CountActiveCards(long accountId);

		/// <summary>
		/// 获取卡片信息用于更新操作（带锁）
		/// </summary>
		/// <param name="cardId">卡片ID</param>
		/// <returns>卡片模型</returns>
		Task<CardEntity?> GetCardByIdForUpdate(long cardId);
	}
}
