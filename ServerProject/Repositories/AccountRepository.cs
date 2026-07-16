using Dapper;
using ServerProject.Common;
using ServerProject.Entities;
using ShareProject.Common;

namespace ServerProject.Repositories
{
    /// <summary>
    /// 帳戶服務實現類，提供帳戶相關的操作方法
    /// </summary>
    public class AccountRepository : RepositoryBase, IAccountRepository
    {
		public AccountRepository(IDataAccess dataAccess) : base(dataAccess)
		{
		}

		/// <summary>
		/// 在余额足够时减少账户余额。
		/// 主要给 TransactionService 在事务中调用。
		/// </summary>
		/// <param name="accountId">账户ID</param>
		/// <param name="amount">减少的金额，必须为正数且不超过当前余额</param>
		/// <param name="updatedAt">更新时刻</param>
		public async Task<int> DecreaseBalanceIfEnoughBalance(long accountId, decimal amount, DateTime updatedAt)
		{
			var sql = LoadSql("Account.DecreaseBalanceIfEnoughBalance");
			return await _dataAccess.DbConnection.ExecuteAsync(sql, new
			{
				AccountId = accountId,
				Amount = amount,
				UpdatedAt = updatedAt
			}, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 检查客户是否已经拥有同币种的有效账户。
		/// </summary>
		/// <param name="customerId">客户ID</param>
		/// <param name="currency">币种</param>
		public async Task<bool> ExistsNotClosedAccountByCustomerIdAndCurrency(int customerId, string currency)
		{
			var sql = LoadSql("Account.ExistsActiveAccountByCustomerIdAndCurrency");
			return await _dataAccess.DbConnection.ExecuteScalarAsync<bool>(sql, new
			{
				CustomerId = customerId,
				Currency = currency
			}, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 根据账户ID取得账户。
		/// 管理员功能或内部处理可用。
		/// </summary>
		/// <param name="accountId">账户ID</param>
		public async Task<AccountEntity?> GetAccountById(long accountId)
		{
			var sql = LoadSql("Account.GetAccountById");
			return await _dataAccess.DbConnection.QueryFirstOrDefaultAsync<AccountEntity>(sql, new
			{
				AccountId = accountId
			}, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 根据账户ID和客户ID取得账户。
		/// 普通用户功能建议优先使用这个方法确认账户归属。
		/// </summary>
		/// <param name="accountId">账户ID</param>
		/// <param name="customerId">客户ID</param>
		public async Task<AccountEntity?> GetAccountByIdAndCustomerId(long accountId, int customerId)
		{
			var sql = LoadSql("Account.GetAccountByIdAndCustomerId");
			return await _dataAccess.DbConnection.QueryFirstOrDefaultAsync<AccountEntity>(sql, new
			{
				AccountId = accountId,
				CustomerId = customerId
			}, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 根据操作日志ID查找该条日志
		/// </summary>
		/// <param name="operationLogId">操作日志ID</param>
		/// <returns>账户操作日志记录</returns>
		public async Task<AccountOperationLogEntity?> GetAccountOperationLogById(long operationLogId)
		{
			var sql = LoadSql("Account.GetAccountOperationLogById");

			return await _dataAccess.DbConnection.QueryFirstOrDefaultAsync<AccountOperationLogEntity>(sql,new
			{
				OperationLogId = operationLogId
			},_dataAccess.DbTransaction);
		}

		/// <summary>
		/// 根据账户ID查找所有相关的账户操作日志记录
		/// </summary>
		/// <param name="accountId">账户ID</param>
		/// <returns>所有相关的账户操作日志记录</returns>
		public async Task<List<AccountOperationLogEntity>> GetAccountOperationLogsByAccountId(long accountId)
		{
			var sql = LoadSql("Account.GetAccountOperationLogsByAccountId");

			return (await _dataAccess.DbConnection.QueryAsync<AccountOperationLogEntity>(sql,new
			{
				AccountId = accountId
			},_dataAccess.DbTransaction)).AsList();
		}

		/// <summary>
		/// 根据客户ID取得该客户名下所有账户。
		/// 管理员后台、审计、账户历史查询使用。
		/// </summary>
		/// <param name="customerId">客户ID</param> 
		public async Task<List<AccountEntity>> GetAllAccountsByCustomerId(int customerId)
		{
			var sql = LoadSql("Account.GetAllAccountsByCustomerId");
			return (await _dataAccess.DbConnection.QueryAsync<AccountEntity>(sql, new
			{
				CustomerId = customerId
			}, _dataAccess.DbTransaction)).AsList();
		}

		/// <summary>
		/// 根据客户ID取得该客户名下账户。
		/// </summary>
		/// <param name="customerId">客户ID</param>
		public async Task<List<AccountEntity>> GetNonClosedAccountsByCustomerId(int customerId)
		{
			var sql = LoadSql("Account.GetNonClosedAccountsByCustomerId");
			return (await _dataAccess.DbConnection.QueryAsync<AccountEntity>(sql, new
			{
				CustomerId = customerId
			}, _dataAccess.DbTransaction)).AsList();
		}

		/// <summary>
		/// 增加账户余额。
		/// 主要给 TransactionService 在事务中调用。
		/// </summary>
		/// <param name="accountId">账户ID</param>
		/// <param name="amount">增加的金额，必须为正数</param>
		/// <param name="updatedAt">更新时刻</param>
		public async Task<int> IncreaseBalance(long accountId, decimal amount, DateTime updatedAt)
		{
			var sql = LoadSql("Account.IncreaseBalance");
			return await _dataAccess.DbConnection.ExecuteAsync(sql, new
			{
				AccountId = accountId,
				Amount = amount,
				UpdatedAt = updatedAt
			}, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 新增账户，并返回新生成的账户ID。
		/// </summary>
		/// <param name="account">账户实体对象，AccountId 属性会被忽略</param>
		public async Task<long> InsertAccount(AccountEntity account)
		{
			var sql = LoadSql("Account.InsertAccount");
			return await _dataAccess.DbConnection.ExecuteScalarAsync<long>(sql, new
			{
				CustomerId = account.CustomerId,
				AccountType = account.AccountType,
				Balance = account.Balance,
				Currency = account.Currency,
				Status = account.Status,
				OpenDate = account.OpenDate,
				CloseDate = account.CloseDate,
				UpdateDate = account.UpdateDate
			}, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 插入新的账户操作日志
		/// </summary>
		/// <param name="accountOperationLog">账户操作日志</param>
		/// <returns>操作日志ID</returns>
		public async Task<long> InsertAccountOperationLog(AccountOperationLogEntity accountOperationLog)
		{
			var sql = LoadSql("Account.InsertAccountOperationLog");

			return await _dataAccess.DbConnection.ExecuteScalarAsync<long>(
				sql,
				new
				{
					AccountId = accountOperationLog.AccountId,
					CustomerId = accountOperationLog.CustomerId,
					OperationType = (short)accountOperationLog.OperationType,
					OldStatus = accountOperationLog.OldStatus.HasValue ? (short?)accountOperationLog.OldStatus.Value : null,
					NewStatus = accountOperationLog.NewStatus.HasValue ? (short?)accountOperationLog.NewStatus.Value : null,
					Reason = accountOperationLog.Reason,
					OperatedBy = accountOperationLog.OperatedBy,
					OperatedAt = accountOperationLog.OperatedAt
				},
				_dataAccess.DbTransaction);
		}

		/// <summary>
		/// 更新账户状态。
		/// </summary>
		/// <param name="accountId">账户ID</param>
		/// <param name="newStatus">新状态</param>
		/// <param name="updatedAt">更新时刻</param>
		public async Task<int> UpdateAccountStatus(long accountId, AccountStatus newStatus, DateTime updatedAt)
		{
			var sql = LoadSql("Account.UpdateAccountStatus");
			return await _dataAccess.DbConnection.ExecuteAsync(sql, new
			{
				AccountId = accountId,
				NewStatus = newStatus,
				UpdatedAt = updatedAt
			}, _dataAccess.DbTransaction);
		}

		/// <summary>
		/// 仅当当前状态符合预期时才更新账户状态。
		/// 用于 Close / Freeze / Unfreeze 这种状态迁移。
		/// </summary>
		/// <param name="accountId">账户ID</param>
		/// <param name="customerId">客户ID</param>
		/// <param name="currentStatus">当前状态</param>
		/// <param name="newStatus">新状态</param>
		/// <param name="updatedAt">更新时刻</param>
		/// <param name="requireZeroBalance">是否要求账户余额为零</param>
		public async Task<int> UpdateAccountStatusIfCurrentStatus(long accountId,int customerId ,AccountStatus currentStatus, AccountStatus newStatus, DateTime updatedAt, bool requireZeroBalance)
		{
			var sql = LoadSql("Account.UpdateAccountStatusIfCurrentStatus");
			return await _dataAccess.DbConnection.ExecuteAsync(sql, new
			{
				AccountId = accountId,
				CustomerId = customerId,
				CurrentStatus = currentStatus,
				NewStatus = newStatus,
				UpdatedAt = updatedAt,
				RequireZeroBalance = requireZeroBalance
			}, _dataAccess.DbTransaction);
		}
	}

    /// <summary>
    /// 帳戶服務接口，提供帳戶相關的操作方法
    /// </summary>
    public interface IAccountRepository
    {
        /// <summary>
        /// 數據訪問對象屬性，用於設置和獲取數據訪問對象
        /// </summary>
        IDataAccess DataAccess { set; }

		/// <summary>
		/// 根据账户ID取得账户。
		/// 管理员功能或内部处理可用。
		/// </summary>
		/// <param name="accountId">账户ID</param>
		Task<AccountEntity?> GetAccountById(long accountId);

		/// <summary>
		/// 根据账户ID和客户ID取得账户。
		/// 普通用户功能建议优先使用这个方法确认账户归属。
		/// </summary>
		/// <param name="accountId">账户ID</param>
		/// <param name="customerId">客户ID</param>
		Task<AccountEntity?> GetAccountByIdAndCustomerId(long accountId, int customerId);

		/// <summary>
		/// 根据客户ID取得该客户名下未关闭账户。
		/// 普通用户侧使用。
		/// Active / Frozen 会返回，Closed 不返回。
		/// </summary>
		/// <param name="customerId">客户ID</param>
		Task<List<AccountEntity>> GetNonClosedAccountsByCustomerId(
			int customerId);

		/// <summary>
		/// 根据客户ID取得该客户名下所有账户。
		/// 管理员后台、审计、账户历史查询使用。
		/// </summary>
		/// <param name="customerId">客户ID</param> 
		Task<List<AccountEntity>> GetAllAccountsByCustomerId(int customerId);

		/// <summary>
		/// 新增账户，并返回新生成的账户ID。
		/// </summary>
		/// <param name="account">账户实体对象，AccountId 属性会被忽略</param>
		Task<long> InsertAccount(AccountEntity account);

		/// <summary>
		/// 更新账户状态。
		/// </summary>
		/// <param name="accountId">账户ID</param>
		/// <param name="newStatus">新状态</param>
		/// <param name="updatedAt">更新时刻</param>
		Task<int> UpdateAccountStatus(
			long accountId,
			AccountStatus newStatus,
			DateTime updatedAt);

		/// <summary>
		/// 仅当当前状态符合预期时才更新账户状态。
		/// 用于 Close / Freeze / Unfreeze 这种状态迁移。
		/// </summary>
		/// <param name="accountId">账户ID</param>
		/// <param name="customerId">客户ID</param>
		/// <param name="currentStatus">当前状态</param>
		/// <param name="newStatus">新状态</param>
		/// <param name="updatedAt">更新时刻</param>
		/// <param name="requireZeroBalance">是否要求账户余额为零</param>
		Task<int> UpdateAccountStatusIfCurrentStatus(
			long accountId,
			int customerId,
			AccountStatus currentStatus,
			AccountStatus newStatus,
			DateTime updatedAt,
			bool requireZeroBalance);

		/// <summary>
		/// 检查客户是否已经拥有同币种的有效账户。
		/// </summary>
		/// <param name="customerId">客户ID</param>
		/// <param name="currency">币种</param>
		Task<bool> ExistsNotClosedAccountByCustomerIdAndCurrency(
			int customerId,
			string currency);

		/// <summary>
		/// 增加账户余额。
		/// 主要给 TransactionService 在事务中调用。
		/// </summary>
		/// <param name="accountId">账户ID</param>
		/// <param name="amount">增加的金额，必须为正数</param>
		/// <param name="updatedAt">更新时刻</param>
		Task<int> IncreaseBalance(
			long accountId,
			decimal amount,
			DateTime updatedAt);

		/// <summary>
		/// 在余额足够时减少账户余额。
		/// 主要给 TransactionService 在事务中调用。
		/// </summary>
		/// <param name="accountId">账户ID</param>
		/// <param name="amount">减少的金额，必须为正数且不超过当前余额</param>
		/// <param name="updatedAt">更新时刻</param>
		Task<int> DecreaseBalanceIfEnoughBalance(
			long accountId,
			decimal amount,
			DateTime updatedAt);

		/// <summary>
		/// 插入新的账户操作日志
		/// </summary>
		/// <param name="accountOperationLog">账户操作日志</param>
		/// <returns>操作日志ID</returns>
		Task<long> InsertAccountOperationLog(AccountOperationLogEntity accountOperationLog);

		/// <summary>
		/// 根据账户ID查找所有相关的账户操作日志记录
		/// </summary>
		/// <param name="accountId">账户ID</param>
		/// <returns>所有相关的账户操作日志记录</returns>
		Task<List<AccountOperationLogEntity>> GetAccountOperationLogsByAccountId(long accountId);

		/// <summary>
		/// 根据操作日志ID查找该条日志
		/// </summary>
		/// <param name="operationLogId">操作日志ID</param>
		/// <returns>账户操作日志记录</returns>
		Task<AccountOperationLogEntity?> GetAccountOperationLogById(long operationLogId);
	}
}