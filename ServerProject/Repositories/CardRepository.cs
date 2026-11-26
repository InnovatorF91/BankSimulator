using Dapper;
using ServerProject.Common;
using ServerProject.Entities;
using ShareProject.Common;
using System.Diagnostics;

namespace ServerProject.Repositories
{
    /// <summary>
    /// 卡片服務實現類
    /// </summary>
    public class CardRepository : RepositoryBase, ICardRepository
    {
		/// <summary>
		/// 日志接口
		/// </summary>
		private readonly ILogger<CardRepository> _logger;

		public CardRepository(IDataAccess dataAccess, ILogger<CardRepository> logger) : base(dataAccess)
		{
			_logger = logger;
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
			var stopwatch = Stopwatch.StartNew();
			var outcome = LogOutcome.Failed;

			_logger.LogStart(level: LogLevel.Debug);

			try
			{
				var sql = LoadSql("Card.DeactivateAllCards");
				var count = await _dataAccess.DbConnection.ExecuteAsync(sql, new
				{
					AccountId = accountId,
					Status = status,
					DeactivatedAt = deactivatedAt,
					UpdatedAt = updatedAt
				}, _dataAccess.DbTransaction);

				outcome = LogOutcome.Completed;
				return count;
			}
			finally
			{
				_logger.LogEnd(outcome, stopwatch.ElapsedMilliseconds, level: LogLevel.Debug);
			}
		}

        /// <summary>
        /// 通過卡片ID獲取卡片信息
        /// </summary>
        /// <param name="cardId">卡片ID</param>
        /// <returns>卡片模型</returns>
        public async Task<CardEntity?> GetCardById(long cardId)
        {
			var stopwatch = Stopwatch.StartNew();
			var outcome = LogOutcome.Failed;

			_logger.LogStart(level: LogLevel.Debug);

			try
			{
				var sql = LoadSql("Card.GetCardById");
				var card = await _dataAccess.DbConnection.QuerySingleOrDefaultAsync<CardEntity>(sql, new { CardId = cardId }, _dataAccess.DbTransaction);

				outcome = LogOutcome.Completed;
				return card;
			}
			finally
			{
				_logger.LogEnd(outcome, stopwatch.ElapsedMilliseconds, level: LogLevel.Debug);
			}
		}

        /// <summary>
        /// 通過帳戶ID獲取該帳戶的所有卡片
        /// </summary>
        /// <param name="accountId">帳戶ID</param>
        /// <returns>卡片模型列表</returns>
        public async Task<List<CardEntity>> GetCardsByAccountId(long accountId)
        {
			var stopwatch = Stopwatch.StartNew();
			var outcome = LogOutcome.Failed;

			_logger.LogStart(level: LogLevel.Debug);

			try
			{
				var sql = LoadSql("Card.GetCardsByAccountId");
				var cards = (await _dataAccess.DbConnection.QueryAsync<CardEntity>(sql, new { AccountId = accountId }, _dataAccess.DbTransaction)).AsList();

				outcome = LogOutcome.Completed;
				return cards;
			}
			finally
			{
				_logger.LogEnd(outcome, stopwatch.ElapsedMilliseconds, level: LogLevel.Debug);
			}
		}

		/// <summary>
		/// 统计账户下同类型、未过期的 Active／Blocked 卡片数量。
		/// </summary>
		/// <param name="accountId">账户ID</param>
		/// <param name="cardType">卡片类型</param>
		/// <param name="currentYear">当前年份</param>
		/// <param name="currentMonth">当前月份</param>
		/// <returns>卡片数量</returns>
		public async Task<int> CountUsableCardsByAccountIdAndType(long accountId,CardType cardType,short currentYear,short currentMonth)
        {
			var stopwatch = Stopwatch.StartNew();
			var outcome = LogOutcome.Failed;

			_logger.LogStart(level: LogLevel.Debug);

			try
			{
				var sql = LoadSql("Card.CountUsableCardsByAccountIdAndType");
				var count = await _dataAccess.DbConnection.ExecuteScalarAsync<int>(sql, new
				{
					AccountId = accountId,
					CardType = cardType,
					CurrentYear = currentYear,
					CurrentMonth = currentMonth,
					ActiveStatus = CardStatus.Active,
					BlockedStatus = CardStatus.Blocked
				}, _dataAccess.DbTransaction);

				outcome = LogOutcome.Completed;
				return count;
			}
			finally
			{
				_logger.LogEnd(outcome, stopwatch.ElapsedMilliseconds, level: LogLevel.Debug);
			}
		}

        /// <summary>
        /// 插入新卡片
        /// </summary>
        /// <param name="card">卡片模型</param>
        /// <returns>新插入卡片的ID</returns>
        public async Task<long> InsertNewCard(CardEntity card)
        {
			var stopwatch = Stopwatch.StartNew();
			var outcome = LogOutcome.Failed;

			_logger.LogStart(level: LogLevel.Debug);

			try
			{
				var sql = LoadSql("Card.InsertNewCard");
				var cardId = await _dataAccess.DbConnection.ExecuteScalarAsync<long>(sql, card, _dataAccess.DbTransaction);

				outcome = cardId > 0 ? LogOutcome.Completed : LogOutcome.Failed;
				return cardId;
			}
			finally
			{
				_logger.LogEnd(outcome, stopwatch.ElapsedMilliseconds, level: LogLevel.Debug);
			}
		}

		/// <summary>
		/// 更新卡片PIN碼
		/// </summary>
		/// <param name="cardId">卡片ID</param>
		/// <param name="newPINHash">新的PIN哈希</param>
		/// <param name="updatedAt">更新時間</param>
		public async Task<int> UpdateCardPIN(long cardId, string newPINHash, DateTime updatedAt)
        {
			var stopwatch = Stopwatch.StartNew();
			var outcome = LogOutcome.Failed;

			_logger.LogStart(level: LogLevel.Debug);

			try
			{
				var sql = LoadSql("Card.UpdateCardPIN");
				var count = await _dataAccess.DbConnection.ExecuteAsync(sql, new
				{
					CardId = cardId,
					PINHash = newPINHash,
					UpdatedAt = updatedAt
				}, _dataAccess.DbTransaction);

				outcome = count == 1 ? LogOutcome.Completed : LogOutcome.Failed;
				return count;
			}
			finally
			{
				_logger.LogEnd(outcome, stopwatch.ElapsedMilliseconds, level: LogLevel.Debug);
			}
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
			var stopwatch = Stopwatch.StartNew();
			var outcome = LogOutcome.Failed;

			_logger.LogStart(level: LogLevel.Debug);

			try
			{
				var sql = LoadSql("Card.UpdateCardStatusIfCurrentStatus");
				var count = await _dataAccess.DbConnection.ExecuteAsync(sql, new
				{
					CardId = cardId,
					CurrentStatus = currentStatus,
					NewStatus = newStatus,
					UpdatedAt = updatedAt
				}, _dataAccess.DbTransaction);

				outcome = count switch
				{
					1 => LogOutcome.Completed,
					0 => LogOutcome.Rejected,
					_ => LogOutcome.Failed
				};
				return count;
			}
			finally
			{
				_logger.LogEnd(outcome, stopwatch.ElapsedMilliseconds, level: LogLevel.Debug);
			}
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
			var stopwatch = Stopwatch.StartNew();
			var outcome = LogOutcome.Failed;

			_logger.LogStart(level: LogLevel.Debug);

			try
			{
				var sql = LoadSql("Card.UpdateCardPINFailCount");
				var count = await _dataAccess.DbConnection.ExecuteAsync(sql, new
				{
					CardId = cardId,
					PINFailCount = newFailCount,
					PINLockedUntil = lockedUntil,
					UpdatedAt = updatedAt
				}, _dataAccess.DbTransaction);

				outcome = count == 1 ? LogOutcome.Completed : LogOutcome.Failed;
				return count;
			}
			finally
			{
				_logger.LogEnd(outcome, stopwatch.ElapsedMilliseconds, level: LogLevel.Debug);
			}
		}

		/// <summary>
		/// 获取卡片信息用于更新操作（带锁）
		/// </summary>
		/// <param name="cardId">卡片ID</param>
		/// <returns>卡片模型</returns>
		public async Task<CardEntity?> GetCardByIdForUpdate(long cardId)
		{
			var stopwatch = Stopwatch.StartNew();
			var outcome = LogOutcome.Failed;

			_logger.LogStart(level: LogLevel.Debug);

			try
			{
				var sql = LoadSql("Card.GetCardByIdForUpdate");
				var card = await _dataAccess.DbConnection.QuerySingleOrDefaultAsync<CardEntity>(sql, new { CardId = cardId }, _dataAccess.DbTransaction);

				outcome = LogOutcome.Completed;
				return card;
			}
			finally
			{
				_logger.LogEnd(outcome, stopwatch.ElapsedMilliseconds, level: LogLevel.Debug);
			}

		}

		/// <summary>
		/// 停用卡片以進行替換操作
		/// </summary>
		/// <param name="cardId">卡片ID</param>
		/// <param name="currentStatus">當前卡片狀態</param>
		/// <param name="newCardId">新卡片ID</param>
		/// <param name="now">當前時間</param>
		/// <returns>受影響的行數</returns>
		public async Task<int> DeactivateCardForReplacement(long cardId, CardStatus currentStatus, long newCardId, DateTime now)
		{
			var stopwatch = Stopwatch.StartNew();
			var outcome = LogOutcome.Failed;

			_logger.LogStart(level: LogLevel.Debug);

			try
			{
				var sql = LoadSql("Card.DeactivateCardForReplacement");
				var count = await _dataAccess.DbConnection.ExecuteAsync(
					sql,
					new
					{
						CardId = cardId,
						CurrentStatus = currentStatus,
						InactiveStatus = CardStatus.Inactive,
						NewCardId = newCardId,
						DeactivatedAt = now,
						UpdatedAt = now
					},
					_dataAccess.DbTransaction);

				outcome = count switch
				{
					1 => LogOutcome.Completed,
					0 => LogOutcome.Rejected,
					_ => LogOutcome.Failed
				};
				return count;
			}
			finally
			{
				_logger.LogEnd(outcome, stopwatch.ElapsedMilliseconds, level: LogLevel.Debug);
			}
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
		/// 统计账户下同类型、未过期的 Active／Blocked 卡片数量。
		/// </summary>
		/// <param name="accountId">账户ID</param>
		/// <param name="cardType">卡片类型</param>
		/// <param name="currentYear">当前年份</param>
		/// <param name="currentMonth">当前月份</param>
		/// <returns>卡片数量</returns>
		Task<int> CountUsableCardsByAccountIdAndType(long accountId,CardType cardType,short currentYear,short currentMonth);

		/// <summary>
		/// 获取卡片信息用于更新操作（带锁）
		/// </summary>
		/// <param name="cardId">卡片ID</param>
		/// <returns>卡片模型</returns>
		Task<CardEntity?> GetCardByIdForUpdate(long cardId);

		/// <summary>
		/// 停用卡片以進行替換操作
		/// </summary>
		/// <param name="cardId">卡片ID</param>
		/// <param name="currentStatus">當前卡片狀態</param>
		/// <param name="newCardId">新卡片ID</param>
		/// <param name="now">當前時間</param>
		/// <returns>受影響的行數</returns>
		Task<int> DeactivateCardForReplacement(long cardId,CardStatus currentStatus,long newCardId,DateTime now);
	}
}
