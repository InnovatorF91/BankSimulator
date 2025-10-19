using Dapper;
using ServerProject.Common;
using ServerProject.Models;
using ShareProject.Common;

namespace ServerProject.Repositories
{
    /// <summary>
    /// 卡片服務實現類
    /// </summary>
    public class CardRepository : ICardRepository
    {
        /// <summary>
        /// SQL查詢語句：通過卡片ID獲取卡片信息
        /// </summary>
        private readonly string queryGetCardById = "SELECT * FROM \"Cards\" WHERE card_id = @CardId";

        /// <summary>
        /// SQL查詢語句：通過帳戶ID獲取該帳戶的所有卡片
        /// </summary>
        private readonly string queryGetCardsByAccountId = "SELECT * FROM \"Cards\" WHERE account_id = @AccountId AND card_status = 0;";

        /// <summary>
        /// SQL插入語句：插入新卡片
        /// </summary>
        private readonly string queryInsertNewCard = "INSERT INTO \"Cards\" " +
            "(account_id, card_number, expiry_year, expiry_month, pin_hash, pin_fail_count, pin_locked_until, card_type, card_status, create_at) " +
            "VALUES " +
            "(@AccountId, @CardNumber, @ExpiryYear, @ExpiryMonth, @PINHash, @PINFailCount, @PINLockedUntil, @CardType, @Status, @CreateAt) " +
            "RETURNING card_id";

        /// <summary>
        /// SQL更新語句：更新卡片狀態
        /// </summary>
        private readonly string queryUpdateCardStatus = "UPDATE \"Cards\" " +
            "SET card_status = @Status, deactivated_at = @DeactivatedAt, replaced_by = @ReplacedBy " +
            "WHERE card_id = @CardId;";

        /// <summary>
        /// SQL更新語句：更新卡片PIN碼
        /// </summary>
        private readonly string queryUpdateCardPIN = "UPDATE \"Cards\" " +
            "SET pin_hash = @PINHash " +
            "WHERE card_id = @CardId;";

        /// <summary>
        /// SQL更新語句：更新PIN碼失敗次數及鎖定時間
        /// </summary>
        private readonly string queryUpdateCardPINFailCount = "UPDATE \"Cards\" " +
            "SET pin_fail_count = @PINFailCount, pin_locked_until = @PINLockedUntil " +
            "WHERE card_id = @CardId;";

        /// <summary>
        /// SQL更新語句：停用帳戶下的所有卡片
        /// </summary>
        private readonly string queryDeactivateAllCards = "UPDATE \"Cards\" " +
            "SET card_status = @Status, deactivated_at = @DeactivatedAt " +
            "WHERE account_id = @AccountId AND card_status != @Status;";

        /// <summary>
        /// SQL查詢語句：檢查帳戶是否有任何活躍的卡片
        /// </summary>
        private readonly string queryHasActiveCards = "SELECT COUNT(1) FROM \"Cards\" " +
            "WHERE account_id = @AccountId AND card_status = 0;";

        /// <summary>
        /// 數據訪問對象，用於與數據庫進行交互
        /// </summary>
        private IDataAccess _dataAccess;

        /// <summary>
        /// 數據訪問對象
        /// </summary>
        public IDataAccess DataAccess { set => throw new NotImplementedException(); }

        /// <summary>
        /// 構造函數，初始化CardRepository實例並注入數據訪問對象
        /// </summary>
        /// <param name="dataAccess">數據訪問對象</param>
        public CardRepository(IDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
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
            // 使用Dapper執行更新操作，並返回受影響的行數
            return await _dataAccess.DbConnection.ExecuteAsync(queryDeactivateAllCards, new
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
        public async Task<CardModel> GetCardById(long cardId)
        {
            // 使用Dapper執行查詢，並將結果映射到CardModel對象
            var card = await _dataAccess.DbConnection.QueryFirstOrDefaultAsync<CardModel>(queryGetCardById, new { CardId = cardId });

            // 如果未找到卡片，則拋出異常
            if (card == null)
            {
                throw new InvalidOperationException($"Card with ID {cardId} not found.");
            }

            // 返回查詢到的卡片模型
            return card;
        }

        /// <summary>
        /// 通過帳戶ID獲取該帳戶的所有卡片
        /// </summary>
        /// <param name="accountId">帳戶ID</param>
        /// <returns>卡片模型列表</returns>
        public async Task<List<CardModel>> GetCardsByAccountId(long accountId)
        {
            // 使用Dapper執行查詢，並將結果映射到CardModel對象列表
            var cards = await _dataAccess.DbConnection.QueryAsync<CardModel>(queryGetCardsByAccountId, new { AccountId = accountId });

            // 將結果轉換為列表並返回
            return cards.ToList();
        }

        /// <summary>
        /// 檢查帳戶是否有任何活躍的卡片
        /// </summary>
        /// <param name="accountId">賬戶ID</param>
        /// <returns>true:有活躍卡片/false:無活躍卡片</returns>
        public async Task<bool> HasActiveCards(long accountId)
        {
            // 使用Dapper執行查詢，並返回是否存在活躍卡片
            return await _dataAccess.DbConnection.ExecuteScalarAsync<int>(queryHasActiveCards, new { AccountId = accountId }) > 0;
        }

        /// <summary>
        /// 插入新卡片
        /// </summary>
        /// <param name="card">卡片模型</param>
        /// <returns>新插入卡片的ID</returns>
        public async Task<long> InsertNewCard(CardModel card)
        {
            // 使用Dapper執行插入操作，並返回新插入卡片的ID
            var newCardId = await _dataAccess.DbConnection.QuerySingleAsync(queryInsertNewCard, card);

            // 返回新插入卡片的ID
            return newCardId.card_id;
        }

        /// <summary>
        /// 更新卡片PIN碼
        /// </summary>
        /// <param name="cardId">卡片ID</param>
        /// <param name="newPINHash">新的PIN哈希</param>
        /// <returns>true:更新成功/false:更新失敗</returns>
        public async Task<bool> UpdateCardPIN(long cardId, string newPINHash)
        {
            return await _dataAccess.DbConnection.ExecuteAsync(queryUpdateCardPIN, new
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
            // 使用Dapper執行更新操作，並返回受影響的行數
            return await _dataAccess.DbConnection.ExecuteAsync(queryUpdateCardStatus, new
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
            return await _dataAccess.DbConnection.ExecuteAsync(queryUpdateCardPINFailCount, new
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
        Task<CardModel> GetCardById(long cardId);

        /// <summary>
        /// 通過帳戶ID獲取該帳戶的所有卡片
        /// </summary>
        /// <param name="accountId">帳戶ID</param>
        /// <returns>卡片模型列表</returns>
        Task<List<CardModel>> GetCardsByAccountId(long accountId);

        /// <summary>
        /// 插入新卡片
        /// </summary>
        /// <param name="card">卡片模型</param>
        /// <returns>新插入卡片的ID</returns>
        Task<long> InsertNewCard(CardModel card);

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
