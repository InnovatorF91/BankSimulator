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
		/// <returns>true:更新成功/false:更新失敗</returns>
		public async Task<bool> DeactivateAllCards(long accountId, CardStatus status, DateTime deactivatedAt)
        {
            var sql = LoadSql("Card.DeactivateAllCards");

            return await _dataAccess.DbConnection.ExecuteAsync(sql, new
            {
                AccountId = accountId,
                Status = status,
                DeactivatedAt = deactivatedAt
            }) > 0;
		}

        /// <summary>
        /// 通過卡片ID獲取卡片信息
        /// </summary>
        /// <param name="cardId">卡片ID</param>
        /// <returns>卡片模型</returns>
        public async Task<CardEntity?> GetCardById(long cardId)
        {
            var sql = LoadSql("Card.GetCardById");
            return await _dataAccess.DbConnection.QuerySingleOrDefaultAsync<CardEntity>(sql, new { CardId = cardId });
		}

        /// <summary>
        /// 通過帳戶ID獲取該帳戶的所有卡片
        /// </summary>
        /// <param name="accountId">帳戶ID</param>
        /// <returns>卡片模型列表</returns>
        public async Task<List<CardEntity>> GetCardsByAccountId(long accountId)
        {
            var sql = LoadSql("Card.GetCardsByAccountId");
            var cards = await _dataAccess.DbConnection.QueryAsync<CardEntity>(sql, new { AccountId = accountId });

            return cards.ToList();
		}

        /// <summary>
        /// 檢查帳戶是否有任何活躍的卡片
        /// </summary>
        /// <param name="accountId">賬戶ID</param>
        /// <returns>true:有活躍卡片/false:無活躍卡片</returns>
        public async Task<bool> HasActiveCards(long accountId)
        {
            var sql = LoadSql("Card.HasActiveCards");
            return await _dataAccess.DbConnection.ExecuteAsync(sql, new { AccountId = accountId }) > 0;
		}

        /// <summary>
        /// 插入新卡片
        /// </summary>
        /// <param name="card">卡片模型</param>
        /// <returns>新插入卡片的ID</returns>
        public async Task<long> InsertNewCard(CardEntity card)
        {
            var sql = LoadSql("Card.InsertNewCard");
            return await _dataAccess.DbConnection.ExecuteScalarAsync<long>(sql, new
            {
                AccountId = card.AccountId,
                CardNumber = card.CardNumber,
                ExpiryYear = card.ExpiryYear,
                ExpiryMonth = card.ExpiryMonth,
				PINHash = card.PINHash,
				PINFailCount = card.PINFailCount,
                PINLockedUntil = card.PINLockedUntil,
                CardType = card.CardType,
				Status = card.Status,
                CreatedAt = card.CreateAt
            });
		}

        /// <summary>
        /// 更新卡片PIN碼
        /// </summary>
        /// <param name="cardId">卡片ID</param>
        /// <param name="newPINHash">新的PIN哈希</param>
        /// <returns>true:更新成功/false:更新失敗</returns>
        public async Task<bool> UpdateCardPIN(long cardId, string newPINHash)
        {
            var sql = LoadSql("Card.UpdateCardPIN");
            return await _dataAccess.DbConnection.ExecuteAsync(sql, new
            {
                CardId = cardId,
                PINHash = newPINHash
            }) > 0;
		}

        /// <summary>
        /// 更新卡片狀態
        /// </summary>
        /// <param name="cardId">卡片ID</param>
        /// <param name="status">新的卡片狀態</param>
        /// <param name="deactivatedAt">卡片被停用的時間</param>
        /// <param name="replacedBy">更換的新卡ID</param>
        /// <returns>true:更新成功/false:更新失敗</returns>
        public async Task<bool> UpdateCardStatus(long cardId, CardStatus status, DateTime? deactivatedAt = null, long? replacedBy = null)
        {
            var sql = LoadSql("Card.UpdateCardStatus");
            return await _dataAccess.DbConnection.ExecuteAsync(sql, new
            {
                CardId = cardId,
                Status = status,
                DeactivatedAt = deactivatedAt,
                ReplacedBy = replacedBy
            }) > 0;
		}

        /// <summary>
        /// 更新PIN碼失敗次數及鎖定時間
        /// </summary>
        /// <param name="cardId">卡片ID</param>
        /// <param name="newFailCount">新的失敗次數</param>
        /// <param name="lockedUntil">鎖定到期時間（如果有的話）</param>
        /// <returns>true:更新成功/false:更新失敗</returns>
        public async Task<bool> UpdatePINFailCount(long cardId, short newFailCount, DateTime? lockedUntil = null)
        {
            var sql = LoadSql("Card.UpdatePINFailCount");
            return await _dataAccess.DbConnection.ExecuteAsync(sql, new
            {
                CardId = cardId,
                PINFailCount = newFailCount,
                PINLockedUntil = lockedUntil
            }) > 0;
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
        /// <param name="status">新的卡片狀態</param>
        /// <param name="deactivatedAt">卡片被停用的時間</param>
        /// <param name="replacedBy">更換的新卡ID</param>
        /// <returns>true:更新成功/false:更新失敗</returns>
        Task<bool> UpdateCardStatus(long cardId, CardStatus status, DateTime? deactivatedAt = null, long? replacedBy = null);

        /// <summary>
        /// 更新卡片PIN碼
        /// </summary>
        /// <param name="cardId">卡片ID</param>
        /// <param name="newPINHash">新的PIN哈希</param>
        /// <returns>true:更新成功/false:更新失敗</returns>
        Task<bool> UpdateCardPIN(long cardId, string newPINHash);

        /// <summary>
        /// 更新PIN碼失敗次數及鎖定時間
        /// </summary>
        /// <param name="cardId">卡片ID</param>
        /// <param name="newFailCount">新的失敗次數</param>
        /// <param name="lockedUntil">鎖定到期時間（如果有的話）</param>
        /// <returns>true:更新成功/false:更新失敗</returns>
        Task<bool> UpdatePINFailCount(long cardId, short newFailCount, DateTime? lockedUntil = null);

        /// <summary>
        /// 停用帳戶下的所有卡片
        /// </summary>
        /// <param name="accountId">賬戶ID</param>
        /// <param name="status">新的卡片狀態</param>
        /// <param name="deactivatedAt">卡片被停用的時間</param>
        /// <returns>true:更新成功/false:更新失敗</returns>
        Task<bool> DeactivateAllCards(long accountId, CardStatus status, DateTime deactivatedAt);

        /// <summary>
        /// 檢查帳戶是否有任何活躍的卡片
        /// </summary>
        /// <param name="accountId">賬戶ID</param>
        /// <returns>true:有活躍卡片/false:無活躍卡片</returns>
        Task<bool> HasActiveCards(long accountId);
    }
}
