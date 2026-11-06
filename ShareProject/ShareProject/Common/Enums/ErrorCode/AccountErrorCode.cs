namespace ShareProject.Common
{
	public enum AccountErrorCode
	{
		AccountNotFound = 2001,                  // 账户不存在
		AccountAccessDenied = 2002,              // 无权访问账户
		AccountAlreadyExists = 2003,             // 账户已存在
		InvalidCurrency = 2004,                  // 无效的币种
		InvalidAccountType = 2005,               // 无效的账户类型
		AccountAlreadyClosed = 2006,             // 账户已经关闭
		AccountBalanceNotZero = 2007,            // 账户余额不为零
		AccountStatusInvalid = 2008,             // 账户状态无效
		AccountNotAvailable = 2009,              // 账户不可用
		AccountOperationLogNotFound = 2010,      // 账户操作日志不存在

		InvalidInitialDepositAmount = 2100,      // 无效的初始存款金额
		AccountCreationFailed = 2101,            // 开户失败
		InitialDepositFailed = 2102,             // 初始存款失败

		ValidationFailed = 2998,                 // 请求验证失败
		UnableToOperateDb = 2999                 // 数据库操作失败
	}
}