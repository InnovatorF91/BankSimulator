namespace ShareProject.Common
{
	/// <summary>
	/// 卡片功能相关的错误代码
	/// </summary>
	public enum CardErrorCode
	{
		CardNotFoundOrAccessDenied = 5001, // 卡片不存在或无权访问
		CardStatusInvalid = 5002,         // 当前卡片状态不允许操作
		CardExpired = 5003,               // 卡片已超过有效期
		CardStateConflict = 5004,         // 卡片状态已变化，条件更新失败

		ValidationFailed = 5098,          // 请求参数验证失败

		UnableToOperateDb = 5099          // 无法操作数据库
	}
}
