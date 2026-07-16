namespace ShareProject.Common
{
	/// <summary>
	/// KYC狀態轉換規則
	/// </summary>
	public static class KycStatusRule
	{
		/// <summary>
		/// 判斷是否可以從當前KYC狀態轉換到目標KYC狀態
		/// </summary>
		/// <param name="current">當前KYC狀態</param>
		/// <param name="target">目標KYC狀態</param>
		/// <returns>如果可以轉換則返回true，否則返回false</returns>
		public static bool CanTransit(KYCStatus current, KYCStatus target)
		{
			if (current == target)
				return false;

			return (current, target) switch
			{
				(KYCStatus.Unreviewed, KYCStatus.Passed) => true,
				(KYCStatus.Unreviewed, KYCStatus.Refused) => true,
				(KYCStatus.Passed, KYCStatus.Refused) => true,
				(KYCStatus.Refused, KYCStatus.Passed) => true,
				_ => false
			};
		}
	}
}
