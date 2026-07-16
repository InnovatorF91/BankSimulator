using Dapper;
using ServerProject.Common;
using ServerProject.Entities;

namespace ServerProject.Repositories
{
    /// <summary>
    /// 客戶認證服務實現類
    /// </summary>
    public class CustomerAuthRepository : RepositoryBase, ICustomerAuthRepository, ICustomerSessionRepository, IRefreshTokenRepository ,IPasswordResetTokenRepository
	{
		public CustomerAuthRepository(IDataAccess dataAccess) : base(dataAccess)
		{
		}

		/// <summary>
		/// 刪除客戶認證信息
		/// </summary>
		/// <param name="customerAuth">需要刪除的客戶信息</param>
		/// <returns>删除結果數量</returns>
		public async Task<int> RemoveAuthEntry(CustomerAuthEntity customerAuth)
        {
			var sql = LoadSql("CustomerAuth.DeleteAuthEntry");

			return await _dataAccess.DbConnection.ExecuteAsync(sql, customerAuth, _dataAccess.DbTransaction);
		}

        /// <summary>
        /// 根據客戶ID獲取客戶認證信息
        /// </summary>
        /// <param name="customerId">客戶ID</param>
        /// <returns>客戶認證信息</returns>
        public async Task<CustomerAuthEntity?> GetAuthByCustomerId(int customerId)
        {
            var sql = LoadSql("CustomerAuth.GetAuthByCustomerId");

			return await _dataAccess.DbConnection.QuerySingleOrDefaultAsync<CustomerAuthEntity>(sql, new { CustomerId = customerId }, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 插入新的客戶認證信息
		/// </summary>
		/// <param name="customerAuth">新的客戶認證信息</param>
		/// <returns>插入結果數量</returns>
		public async Task<int> InsertAuthEntry(CustomerAuthEntity customerAuth)
        {
            var sql = LoadSql("CustomerAuth.InsertAuthEntry");

			return await _dataAccess.DbConnection.ExecuteAsync(sql, customerAuth, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 更新客戶認證信息
		/// </summary>
		/// <param name="customerAuth">需要更新的客戶信息</param>
		/// <returns>更新結果數量</returns>
		public async Task<int> UpdateAuthEntry(CustomerAuthEntity customerAuth)
        {
            var sql = LoadSql("CustomerAuth.UpdateAuthEntry");

			return await _dataAccess.DbConnection.ExecuteAsync(sql, customerAuth, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 根據登入ID獲取客戶認證信息
		/// </summary>
		/// <param name="loginId">登入ID</param>
		/// <returns>客戶認證信息</returns>
		public async Task<CustomerAuthEntity?> GetAuthByLoginId(string loginId)
		{
			var sql = LoadSql("CustomerAuth.GetAuthByLoginId");

			return await _dataAccess.DbConnection.QuerySingleOrDefaultAsync<CustomerAuthEntity>(sql, new { LoginId = loginId }, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 更新密碼哈希值
		/// </summary>
		/// <param name="userId">登入ID</param>
		/// <param name="hash">密碼哈希值</param>
		/// <param name="updateDate">更新時間</param>
		/// <returns>更新結果數量</returns>
		public async Task<int> UpdatePasswordHash(int userId, string hash, DateTime updateDate)
		{
			var sql = LoadSql("CustomerAuth.UpdatePasswordHash");
			return await _dataAccess.DbConnection.ExecuteAsync(sql, new
			{
				UserId = userId,
				Hash = hash,
				UpdateDate = updateDate
			}, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 更新失敗次數
		/// </summary>
		/// <param name="userId">登入ID</param>
		/// <param name="updateDate">更新時間</param>
		/// <returns>更新結果數量</returns>
		public async Task<int> UpdateFailedCount(int userId, DateTime updateDate)
		{
			var sql = LoadSql("CustomerAuth.UpdateFailedCount");
			return await _dataAccess.DbConnection.ExecuteAsync(sql, new
			{
				UserId = userId,
				UpdateDate = updateDate
			}, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 锁定帳號
		/// </summary>
		/// <param name="userId">登入ID</param>
		/// <param name="lockedUntil">鎖定時間</param>
		/// <param name="updateDate">更新時間</param>
		/// <returns>更新結果數量</returns>
		public async Task<int> Lock(int userId, DateTime lockedUntil, DateTime updateDate)
		{
			var sql = LoadSql("CustomerAuth.LockAccount");
			return await _dataAccess.DbConnection.ExecuteAsync(sql, new
			{
				UserId = userId,
				LockedUntil = lockedUntil,
				UpdateDate = updateDate
			}, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 解除鎖定
		/// </summary>
		/// <param name="userId">登入ID</param>
		/// <param name="updateDate">更新時間</param>
		/// <returns>更新結果數量</returns>
		public async Task<int> UnLock(int userId, DateTime updateDate)
		{
			var sql = LoadSql("CustomerAuth.UnLockAccount");
			return await _dataAccess.DbConnection.ExecuteAsync(sql, new
			{
				UserId = userId,
				UpdateDate = updateDate
			}, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 撤銷刷新令牌
		/// </summary>
		/// <param name="tokenId">令牌ID</param>
		/// <param name="now">當前時間</param>
		/// <returns>更新結果數量</returns>
		public async Task<int> RevokeRefreshToken(Guid tokenId, DateTime now)
		{
			var sql = LoadSql("CustomerAuth.RevokeRefresh");
			return await _dataAccess.DbConnection.ExecuteAsync(sql, new
			{
				TokenId = tokenId,
				Now = now
			}, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 撤銷所有刷新令牌
		/// </summary>
		/// <param name="userId">登入ID</param>
		/// <param name="now">當前時間</param>
		/// <returns>被撤銷的令牌數量</returns>
		public async Task<int> RevokeAllRefreshTokens(int userId, DateTime now)
		{
			var sql = LoadSql("CustomerAuth.RevokeAllRefresh");
			return await _dataAccess.DbConnection.ExecuteAsync(sql, new
			{
				UserId = userId,
				Now = now
			}, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 提升令牌版本
		/// </summary>
		/// <param name="userId">登入ID</param>
		/// <param name="now">當前時間</param>
		/// <returns>更新結果數量</returns>
		public async Task<int> BumpTokenVersion(int userId, DateTime now)
		{
			var sql = LoadSql("CustomerAuth.BumpTokenVersion");
			return await _dataAccess.DbConnection.ExecuteAsync(sql, new
			{
				UserId = userId,
				Now = now
			}, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 創建新會話
		/// </summary>
		/// <param name="userId">登入ID</param>
		/// <param name="device">机器</param>
		/// <param name="ip">ip地址</param>
		/// <param name="now">當前時間</param>
		/// <param name="expiredAt">過期時間</param>
		/// <returns>新會話</returns>
		public async Task<Guid?> CreateSession(int userId, string? device, string? ip, DateTime now, DateTime expiredAt)
		{
			var sql = LoadSql("CustomerAuth.CreateSession");
			return await _dataAccess.DbConnection.ExecuteScalarAsync<Guid>(sql, new
			{
				UserId = userId,
				Device = device,
				Ip = ip,
				Now = now,
				ExpiredAt = expiredAt
			}, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 使會話無效
		/// </summary>
		/// <param name="sessionId">會話ID</param>
		/// <param name="now">當前時間</param>
		/// <returns>更新結果數量</returns>
		public async Task<int> InvalidateSession(Guid sessionId, DateTime now)
		{
			var sql = LoadSql("CustomerAuth.InvalidateSession");
			return await _dataAccess.DbConnection.ExecuteAsync(sql, new
			{
				SessionId = sessionId,
				Now = now
			}, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 使所有會話無效
		/// </summary>
		/// <param name="userId">登入ID</param>
		/// <param name="now">當前時間</param>
		/// <returns>被使無效的會話數量</returns>
		public async Task<int> InvalidateAllSessions(int userId, DateTime now)
		{
			var sql = LoadSql("CustomerAuth.InvalidateAllSessions");
			return await _dataAccess.DbConnection.ExecuteAsync(sql, new
			{
				UserId = userId,
				Now = now
			}, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 更新會話的最後活動時間
		/// </summary>
		/// <param name="sessionId">會話ID</param>
		/// <param name="now">當前時間</param>
		/// <param name="newExpiredAt">新的過期時間</param>
		/// <returns>更新結果數量</returns>
		public async Task<int> Touch(Guid sessionId, DateTime now, DateTime newExpiredAt)
		{
			var sql = LoadSql("CustomerAuth.TouchSession");
			return await _dataAccess.DbConnection.ExecuteAsync(sql, new
			{
				SessionId = sessionId,
				Now = now,
				NewExpiredAt = newExpiredAt
			}, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 存儲刷新令牌
		/// </summary>
		/// <param name="userId">登入ID</param>
		/// <param name="tokenHash">令牌哈希值</param>
		/// <param name="issuedAt">發行時間</param>
		/// <param name="expiresAt">過期時間</param>
		/// <param name="device">机器</param>
		/// <param name="ip">ip地址</param>
		/// <param name="tokenVersion">令牌版本</param>
		/// <returns>令牌ID</returns>
		public async Task<Guid?> StoreRefreshToken(int userId, string tokenHash, DateTime issuedAt, DateTime expiresAt, string? device, string? ip,int tokenVersion)
		{
			var sql = LoadSql("CustomerAuth.StoreRefresh");
			return await _dataAccess.DbConnection.ExecuteScalarAsync<Guid>(sql, new
			{
				UserId = userId,
				TokenHash = tokenHash,
				IssuedAt = issuedAt,
				ExpiresAt = expiresAt,
				Device = device,
				Ip = ip,
				TokenVersion = tokenVersion
			}, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 更新認證類型
		/// </summary>
		/// <param name="userId">登入ID</param>
		/// <param name="authType">認證類型</param>
		/// <param name="updateDate">更新時間</param>
		/// <returns>更新結果數量</returns>
		public async Task<int> UpdateAuthType(int userId, short authType, DateTime updateDate)
		{
			var sql = LoadSql("CustomerAuth.UpdateAuthType");
			return await _dataAccess.DbConnection.ExecuteAsync(sql, new
			{
				UserId = userId,
				AuthType = authType,
				UpdateDate = updateDate
			}, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 開始啟用雙重認證
		/// </summary>
		/// <param name="userId">登入ID</param>
		/// <param name="secret">密鑰</param>
		/// <param name="now">當前時間</param>
		/// <param name="expiresAt">過期時間</param>
		/// <returns>任務結果，包含受影響的行數</returns>
		public async Task<int> BeginEnableTwoFactor(int userId, string secret, DateTime now, DateTime expiresAt)
		{
			var sql = LoadSql("CustomerAuth.BeginEnableTwoFactor");
			return await _dataAccess.DbConnection.ExecuteAsync(sql, new
			{
				UserId = userId,
				Secret = secret,
				Now = now,
				ExpiresAt = expiresAt
			}, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 確認啟用雙重認證
		/// </summary>
		/// <param name="userId">登入ID</param>
		/// <param name="now">當前時間</param>
		/// <returns>任務結果，包含受影響的行數</returns>
		public async Task<int> ConfirmEnableTwoFactor(int userId, DateTime now)
		{
			var sql = LoadSql("CustomerAuth.ConfirmEnableTwoFactor");
			return await _dataAccess.DbConnection.ExecuteAsync(sql, new
			{
				UserId = userId,
				Now = now
			}, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 根據會話ID獲取會話信息
		/// </summary>
		/// <param name="sessionId">會話ID</param>
		/// <returns>客戶會話模型</returns>
		public async Task<CustomerSessionEntity?> GetSessionById(Guid sessionId)
		{
			var sql = LoadSql("CustomerAuth.GetSessionById");
			return await _dataAccess.DbConnection.QuerySingleOrDefaultAsync<CustomerSessionEntity>(sql, new { SessionId = sessionId }, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 根據刷新令牌哈希值獲取刷新令牌信息
		/// </summary>
		/// <param name="refreshTokenHash">刷新令牌哈希值</param>
		/// <returns>刷新令牌信息</returns>
		public async Task<RefreshTokenEntity?> GetRefreshTokenByTokenHash(string refreshTokenHash)
		{
			var sql = LoadSql("CustomerAuth.GetRefreshTokenByTokenHash");
			return await _dataAccess.DbConnection.QuerySingleOrDefaultAsync<RefreshTokenEntity>(sql, new { RefreshTokenHash = refreshTokenHash }, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 更新密碼哈希值並提升令牌版本
		/// </summary>
		/// <param name="userId">登入ID</param>
		/// <param name="newPwdHash">新密码哈希</param>
		/// <param name="now">當前時間</param>
		/// <returns>任务结果，包含受影响的行数</returns>
		public async Task<int> UpdatePasswordHashAndBumpTokenVersion(int userId, string newPwdHash, DateTime now)
		{
			var sql = LoadSql("CustomerAuth.UpdatePasswordHashAndBumpTokenVersion");
			return await _dataAccess.DbConnection.ExecuteAsync(sql, new
			{
				CustomerId = userId,
				PasswordHash = newPwdHash,
				Now = now
			}, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 禁用雙重認證
		/// </summary>
		/// <param name="customerId">登入ID</param>
		/// <param name="updatedAt">更新時間</param>
		/// <returns>任务结果，包含受影响的行数</returns>
		public async Task<int> DisableTwoFactor(int customerId, DateTime updatedAt)
		{
			var sql = LoadSql("CustomerAuth.DisableTwoFactor");
			return await _dataAccess.DbConnection.ExecuteAsync(sql, new
			{
				CustomerId = customerId,
				UpdatedAt = updatedAt
			}, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 存储密碼重置令牌
		/// </summary>
		/// <param name="customerId">登入ID</param>
		/// <param name="tokenHash">令牌哈希值</param>
		/// <param name="createdAt">創建時間</param>
		/// <param name="expiresAt">過期時間</param>
		/// <param name="createdByIp">創建者IP地址</param>
		/// <param name="createdByDevice">創建者設備信息</param>
		/// <returns>令牌ID</returns>
		public async Task<Guid?> StoreResetToken(int customerId, string tokenHash, DateTime createdAt, DateTime expiresAt, string? createdByIp, string? createdByDevice)
		{
			var sql = LoadSql("CustomerAuth.StoreResetToken");
			return await _dataAccess.DbConnection.ExecuteScalarAsync<Guid>(sql, new
			{
				CustomerId = customerId,
				TokenHash = tokenHash,
				CreatedAt = createdAt,
				ExpiresAt = expiresAt,
				CreatedByIp = createdByIp,
				CreatedByDevice = createdByDevice
			}, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 撤銷所有有效的密碼重置令牌
		/// </summary>
		/// <param name="customerId">登入ID</param>
		/// <param name="now">當前時間</param>
		/// <returns>被撤銷的令牌數量</returns>
		public async Task<int> RevokeActiveResetTokens(int customerId, DateTime now)
		{
			var sql = LoadSql("CustomerAuth.RevokeActiveResetTokens");
			return await _dataAccess.DbConnection.ExecuteAsync(sql, new
			{
				CustomerId = customerId,
				Now = now
			}, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 根據令牌哈希值獲取密碼重置令牌信息
		/// </summary>
		/// <param name="tokenHash">令牌哈希值</param>
		/// <returns>密碼重置令牌信息</returns>
		public async Task<PasswordResetTokenEntity?> GetResetTokenByTokenHash(string tokenHash)
		{
			var sql = LoadSql("CustomerAuth.GetResetTokenByTokenHash");
			return await _dataAccess.DbConnection.QuerySingleOrDefaultAsync<PasswordResetTokenEntity>(sql, new { TokenHash = tokenHash }, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 根據令牌ID標記密碼重置令牌為已使用
		/// </summary>
		/// <param name="tokenId">令牌ID</param>
		/// <param name="usedAt">使用時間</param>
		/// <returns>更新結果數量</returns>
		public async Task<int> MarkResetTokenAsUsed(Guid tokenId, DateTime usedAt)
		{
			var sql = LoadSql("CustomerAuth.MarkResetTokenAsUsed");
			return await _dataAccess.DbConnection.ExecuteAsync(sql, new
			{
				TokenId = tokenId,
				UsedAt = usedAt
			}, _dataAccess.DbTransaction);
		}
	}

    /// <summary>
    /// 客戶認證服務接口
    /// </summary>
    public interface ICustomerAuthRepository
    {
        /// <summary>
        /// 數據訪問對象，用於執行數據庫操作
        /// </summary>
        IDataAccess DataAccess { set; }

        /// <summary>
        /// 根據客戶ID獲取客戶認證信息
        /// </summary>
        /// <param name="customerId">客戶ID</param>
        /// <returns>客戶認證信息</returns>
        Task<CustomerAuthEntity?> GetAuthByCustomerId(int customerId);

        /// <summary>
        /// 插入新的客戶認證信息
        /// </summary>
        /// <param name="customerAuth">新的客戶認證信息</param>
        /// <returns>true:插入成功/false:插入失敗</returns>
        Task<int> InsertAuthEntry(CustomerAuthEntity customerAuth);

        /// <summary>
        /// 更新客戶認證信息
        /// </summary>
        /// <param name="customerAuth">需要更新的客戶信息</param>
        /// <returns>true:更新成功/false:更新失敗</returns>
        Task<int> UpdateAuthEntry(CustomerAuthEntity customerAuth);

        /// <summary>
        /// 刪除客戶認證信息
        /// </summary>
        /// <param name="customerAuth">需要刪除的客戶信息</param>
        /// <returns>true:刪除成功/false:刪除失敗</returns>
        Task<int> RemoveAuthEntry(CustomerAuthEntity customerAuth);

        /// <summary>
        /// 根據登入ID獲取客戶認證信息
        /// </summary>
        /// <param name="loginId">登入ID</param>
        /// <returns>客戶認證信息</returns>
        Task<CustomerAuthEntity?> GetAuthByLoginId(string loginId);

		/// <summary>
		/// 更新密碼哈希值
		/// </summary>
		/// <param name="userId">登入ID</param>
		/// <param name="hash">密碼哈希值</param>
		/// <param name="updateDate">更新時間</param>
		/// <returns>true:更新成功/false:更新失敗</returns>
		Task<int> UpdatePasswordHash(int userId, string hash, DateTime updateDate);

		/// <summary>
		/// 更新失敗次數
		/// </summary>
		/// <param name="userId">登入ID</param>
		/// <param name="updateDate">更新時間</param>
		/// <returns>true:更新成功/false:更新失敗</returns>
		Task<int> UpdateFailedCount(int userId, DateTime updateDate);

		/// <summary>
		/// 锁定帳號
		/// </summary>
		/// <param name="userId">登入ID</param>
		/// <param name="lockedUntil">鎖定時間</param>
		/// <param name="updateDate">更新時間</param>
		/// <returns>true:更新成功/false:更新失敗</returns>
		Task<int> Lock(int userId, DateTime lockedUntil, DateTime updateDate);

		/// <summary>
		/// 解除鎖定
		/// </summary>
		/// <param name="userId">登入ID</param>
		/// <param name="updateDate">更新時間</param>
		/// <returns>true:更新成功/false:更新失敗</returns>
		Task<int> UnLock(int userId, DateTime updateDate);

		/// <summary>
		/// 更新認證類型
		/// </summary>
		/// <param name="userId">登入ID</param>
		/// <param name="authType">認證類型</param>
		/// <param name="updateDate">更新時間</param>
		/// <returns>true:更新成功/false:更新失敗</returns>
		Task<int> UpdateAuthType(int userId, short authType, DateTime updateDate);

		/// <summary>
		/// 開始啟用雙重認證
		/// </summary>
		/// <param name="userId">登入ID</param>
		/// <param name="secret">密鑰</param>
		/// <param name="now">當前時間</param>
		/// <param name="expiresAt">過期時間</param>
		/// <returns>任務結果，包含受影響的行數</returns>
		Task<int> BeginEnableTwoFactor(int userId, string secret, DateTime now, DateTime expiresAt);

		/// <summary>
		/// 確認啟用雙重認證
		/// </summary>
		/// <param name="userId">登入ID</param>
		/// <param name="now">當前時間</param>
		/// <returns>任務結果，包含受影響的行數</returns>
		Task<int> ConfirmEnableTwoFactor(int userId, DateTime now);

		/// <summary>
		/// 更新密碼哈希值並提升令牌版本
		/// </summary>
		/// <param name="userId">登入ID</param>
		/// <param name="newPwdHash">新密码哈希</param>
		/// <param name="now">當前時間</param>
		/// <returns>任务结果，包含受影响的行数</returns>
		Task<int> UpdatePasswordHashAndBumpTokenVersion(int userId, string newPwdHash, DateTime now);

		/// <summary>
		/// 禁用雙重認證
		/// </summary>
		/// <param name="customerId">登入ID</param>
		/// <param name="updatedAt">更新時間</param>
		/// <returns>任务结果，包含受影响的行数</returns>
		Task<int> DisableTwoFactor(int customerId, DateTime updatedAt);
	}

	/// <summary>
	/// 客戶會話存儲庫接口
	/// </summary>
	public interface ICustomerSessionRepository
    {
		/// <summary>
		/// 數據訪問對象，用於執行數據庫操作
		/// </summary>
		IDataAccess DataAccess { set; }

		/// <summary>
		/// 創建新會話
		/// </summary>
		/// <param name="userId">登入ID</param>
		/// <param name="device">机器</param>
		/// <param name="ip">ip地址</param>
		/// <param name="now">當前時間</param>
		/// <param name="expiredAt">過期時間</param>
		/// <returns>新會話</returns>
		Task<Guid?> CreateSession(int userId, string? device, string? ip, DateTime now, DateTime expiredAt);

		/// <summary>
		/// 使會話無效
		/// </summary>
		/// <param name="sessionId">會話ID</param>
		/// <param name="now">當前時間</param>
		/// <returns>更新結果數量</returns>
		Task<int> InvalidateSession(Guid sessionId, DateTime now);

		/// <summary>
		/// 使所有會話無效
		/// </summary>
		/// <param name="userId">登入ID</param>
		/// <param name="now">當前時間</param>
		/// <returns>被使無效的會話數量</returns>
		Task<int> InvalidateAllSessions(int userId, DateTime now);

		/// <summary>
		/// 更新會話的最後活動時間
		/// </summary>
		/// <param name="sessionId">會話ID</param>
		/// <param name="now">當前時間</param>
		/// <param name="newExpiredAt">新的過期時間</param>
		/// <returns>更新結果數量</returns>
		Task<int> Touch(Guid sessionId, DateTime now, DateTime newExpiredAt);

		/// <summary>
		/// 根據會話ID獲取會話信息
		/// </summary>
		/// <param name="sessionId">會話ID</param>
		/// <returns>客戶會話模型</returns>
		Task<CustomerSessionEntity?> GetSessionById(Guid sessionId);
	}

	/// <summary>
	/// 刷新令牌存儲庫接口
	/// </summary>
	public interface IRefreshTokenRepository
    {
		/// <summary>
		/// 數據訪問對象，用於執行數據庫操作
		/// </summary>
		IDataAccess DataAccess { set; }

		/// <summary>
		/// 存儲刷新令牌
		/// </summary>
		/// <param name="userId">登入ID</param>
		/// <param name="tokenHash">令牌哈希值</param>
		/// <param name="issuedAt">發行時間</param>
		/// <param name="expiresAt">過期時間</param>
		/// <param name="device">机器</param>
		/// <param name="ip">ip地址</param>
		/// <param name="tokenVersion">令牌版本</param>
		/// <returns>令牌ID</returns>
		Task<Guid?> StoreRefreshToken(
                                    	int userId,
                                    	string tokenHash,
                                    	DateTime issuedAt,
                                    	DateTime expiresAt,
                                    	string? device,
                                    	string? ip,
										int tokenVersion
                                    );

		/// <summary>
		/// 撤銷刷新令牌
		/// </summary>
		/// <param name="tokenId">令牌ID</param>
		/// <param name="now">當前時間</param>
		/// <returns>true：撤銷成功/false：撤銷失败</returns>
		Task<int> RevokeRefreshToken(Guid tokenId, DateTime now);

		/// <summary>
		/// 撤銷所有刷新令牌
		/// </summary>
		/// <param name="userId">登入ID</param>
		/// <param name="now">當前時間</param>
		/// <returns>被撤銷的令牌數量</returns>
		Task<int> RevokeAllRefreshTokens(int userId, DateTime now);

		/// <summary>
		/// 提升令牌版本
		/// </summary>
		/// <param name="userId">登入ID</param>
		/// <param name="now">當前時間</param>
		/// <returns>更新結果數量</returns>
		Task<int> BumpTokenVersion(int userId, DateTime now);

		/// <summary>
		/// 根據刷新令牌哈希值獲取刷新令牌信息
		/// </summary>
		/// <param name="refreshTokenHash">刷新令牌哈希值</param>
		/// <returns>刷新令牌信息</returns>
		Task<RefreshTokenEntity?> GetRefreshTokenByTokenHash(string refreshTokenHash);
	}

	/// <summary>
	/// 密碼重置令牌存儲庫接口
	/// </summary>
	public interface IPasswordResetTokenRepository 
	{
		/// <summary>
		/// 數據訪問對象，用於執行數據庫操作
		/// </summary>
		IDataAccess DataAccess { set; }

		/// <summary>
		/// 存储密碼重置令牌
		/// </summary>
		/// <param name="customerId">登入ID</param>
		/// <param name="tokenHash">令牌哈希值</param>
		/// <param name="createdAt">創建時間</param>
		/// <param name="expiresAt">過期時間</param>
		/// <param name="createdByIp">創建者IP地址</param>
		/// <param name="createdByDevice">創建者設備信息</param>
		/// <returns>令牌ID</returns>
		Task<Guid?> StoreResetToken(
			int customerId,
	        string tokenHash,
	        DateTime createdAt,
	        DateTime expiresAt,
	        string? createdByIp,
	        string? createdByDevice);

		/// <summary>
		/// 撤銷所有有效的密碼重置令牌
		/// </summary>
		/// <param name="customerId">登入ID</param>
		/// <param name="now">當前時間</param>
		/// <returns>被撤銷的令牌數量</returns>
		Task<int> RevokeActiveResetTokens(int customerId, DateTime now);

		/// <summary>
		/// 根據令牌哈希值獲取密碼重置令牌信息
		/// </summary>
		/// <param name="tokenHash">令牌哈希值</param>
		/// <returns>密碼重置令牌信息</returns>
		Task<PasswordResetTokenEntity?> GetResetTokenByTokenHash(string tokenHash);

		/// <summary>
		/// 根據令牌ID標記密碼重置令牌為已使用
		/// </summary>
		/// <param name="tokenId">令牌ID</param>
		/// <param name="usedAt">使用時間</param>
		/// <returns>更新結果數量</returns>
		Task<int> MarkResetTokenAsUsed(Guid tokenId, DateTime usedAt);
	}
}
