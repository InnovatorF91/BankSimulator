namespace ShareProject.Common
{
	/// <summary>
	/// 认证错误代码
	/// </summary>
	public enum AuthErrorCode
	{
		LoginIdNotFound = 1001, // 用户不存在
		AccountLocked = 1002, // 账号被锁定
		PasswordInvalid = 1003, // 密码不通过
		FailedCountUpdateFailed = 1004, // DB返回0行（非异常）
		LockAccountFailed = 1005, // DB返回0行（非异常）
		TwoFactorRequired = 1006, // 需要2FA
		TwoFactorCodeInvalid = 1007, // 2FA码不通过
		CreateSessionFailed = 1010, // session == null
		CreateRefreshTokenFailed = 1020, // tokenId == null
		AuthTypeInvalid = 1099,
		LoginIdOrPasswordRequired = 1100, // loginId或password为空

		TwoFactorBeginFailed = 1101, // BeginEnableTwoFactor rows<=0
		TwoFactorConfirmInvalidState = 1102, // 非Pending/secret空/过期/auth不存在
		TwoFactorConfirmCodeInvalid = 1103, // TOTP不通过
		TwoFactorConfirmUpdateFailed = 1104, // Confirm rows<=0

		LogoutUserNotFound = 1201, // logout时用户不存在

		SessionNotFound = 1301, // session==null
		SessionExpired = 1302, // 过期（ExpiresAt<now）
		SessionInvalid = 1303, // is_valid=false
		SessionTouchRejected = 1310, // Touch rows==0（过期/无效/不存在）

		RefreshTokenInvalid = 1401,        // token为空 / 格式不正确
		RefreshTokenNotFound = 1402,       // 数据库中不存在
		RefreshTokenExpired = 1403,        // expires_at < now
		RefreshTokenRevoked = 1404,        // revoked_at != null
		RefreshTokenReuseDetected = 1405,  // 已被使用（Token Rotation检测）
		RefreshTokenUserNotFound = 1406,   // 找不到对应用户(auth不存在)
		RefreshTokenAuthInvalid = 1407,    // auth状态异常（如被删除/禁用）
		RefreshTokenCreateFailed = 1410,   // 新token写入失败
		RefreshTokenRevokeFailed = 1411,   // 撤销旧token失败
		RefreshTokenRotationFailed = 1412, // 整体刷新流程失败（事务失败）
		RefreshTokenTokenVersionMismatch = 1413, // TokenVersion不一致（密码修改后旧token）

		ChangePasswordUserNotFound = 1501,              // 根据userId找不到用户/auth
		ChangePasswordOldPasswordRequired = 1502,       // oldPwd为空
		ChangePasswordNewPasswordRequired = 1503,       // newPwd为空
		ChangePasswordOldPasswordInvalid = 1504,        // 旧密码验证失败
		ChangePasswordNewPasswordSameAsOld = 1505,      // 新旧密码相同
		ChangePasswordNewPasswordPolicyInvalid = 1506,  // 新密码不符合规则
		ChangePasswordUpdatePasswordFailed = 1510,      // 更新password_hash失败(rows<=0)
		ChangePasswordBumpTokenVersionFailed = 1511,    // 提升TokenVersion失败(rows<=0)
		ChangePasswordRevokeRefreshTokensFailed = 1512, // 撤销RefreshToken失败(rows<=0)
		ChangePasswordInvalidateSessionsFailed = 1513,  // 失效Session失败(rows<=0)
		ChangePasswordTransactionFailed = 1514,         // 整体事务失败

		UpdateAuthTypeUserNotFound = 1601, // 根据userId找不到用户/auth
		UpdateAuthTypeFailed = 1602, // 更新AuthType失败(rows<=0)

		DisableTwoFactorUserNotFound = 1701, // 根据userId找不到用户/auth
		DisableTwoFactorInvalidState = 1702, // 2FA未启用/过期/auth不存在
		DisableTwoFactorFailed = 1703, // 禁用2FA失败(rows<=0)

		ResetPasswordLoginIdRequired = 1801, // loginId为空
		ResetPasswordFailed = 1802, // 重置密码失败（如找不到用户/不符合规则/更新失败）

		ConfirmResetPasswordTokenInvalid = 1901, // token无效（如格式错误/数据库中不存在/过期/已使用）
		ConfirmResetPasswordTokenExpired = 1902, // token过期
		ConfirmResetPasswordNewPasswordRequired = 1903, // newPwd为空
		ConfirmResetPasswordNewPasswordPolicyInvalid = 1904, // 新密码不符合规则
		ConfirmResetPasswordUserNotFound = 1905, // 根据token找不到用户/auth
		ConfirmResetPasswordFailed = 1906, // 确认重置密码失败（如更新密码失败/撤销token失败）
		ConfirmResetPasswordTokenRequired = 1907, // token为空

		UnableToOperateDb = 1999, // DB操作失败（如异常/超时/rows<=0）
		ValidationFailed = 2000, // 通用验证失败
	}
}
