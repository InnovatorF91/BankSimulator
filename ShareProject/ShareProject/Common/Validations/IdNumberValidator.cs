
using System.Text.RegularExpressions;

namespace ShareProject.Common
{
	/// <summary>
	/// 號碼驗證器
	/// </summary>
	public static class IdNumberValidator
	{
		/// <summary>
		/// 驗證號碼是否有效
		/// </summary>
		/// <param name="idType">身份證明類型</param>
		/// <param name="idNumber">身份證明號碼</param>
		/// <returns>如果號碼有效，則返回 true；否則返回 false</returns>
		public static bool IsValid(IDType idType, string? idNumber)
		{
			if (string.IsNullOrWhiteSpace(idNumber))
				return false;

			return idType switch
			{
				IDType.IDCard => IsValidIdCard(idNumber),
				IDType.Passport => IsValidPassport(idNumber),
				IDType.ResidenceCard => IsValidResidenceCard(idNumber),
				IDType.DriverLicense => IsValidDriverLicense(idNumber),
				_ => false
			};
		}

		/// <summary>
		/// 驗證駕駛執照號碼是否有效
		/// </summary>
		/// <param name="idNumber">駕駛執照號碼</param>
		/// <returns>如果號碼有效，則返回 true；否則返回 false</returns>
		private static bool IsValidDriverLicense(string idNumber)
		{
			if (string.IsNullOrWhiteSpace(idNumber))
				return false;

			return Regex.IsMatch(idNumber, @"^\d{12}$");
		}

		/// <summary>
		/// 驗證居留證號碼是否有效
		/// </summary>
		/// <param name="idNumber">居留證號碼</param>
		/// <returns>如果號碼有效，則返回 true；否則返回 false</returns>
		private static bool IsValidResidenceCard(string idNumber)
		{
			if (string.IsNullOrWhiteSpace(idNumber))
				return false;

			return Regex.IsMatch(idNumber, @"^[A-Z]{2}\d{8}$");
		}

		/// <summary>
		/// 驗證護照號碼是否有效
		/// </summary>
		/// <param name="idNumber">護照號碼</param>
		/// <returns>如果號碼有效，則返回 true；否則返回 false</returns>
		private static bool IsValidPassport(string idNumber)
		{
			if (string.IsNullOrWhiteSpace(idNumber))
				return false;

			return Regex.IsMatch(idNumber, @"^[A-Z0-9]{6,9}$");
		}

		/// <summary>
		/// 驗證身份證號碼是否有效
		/// </summary>
		/// <param name="idNumber">身份證號碼</param>
		/// <returns>如果號碼有效，則返回 true；否則返回 false</returns>
		private static bool IsValidIdCard(string idNumber)
		{
			if (string.IsNullOrWhiteSpace(idNumber))
				return false;

			idNumber = idNumber.ToUpper();

			if (!Regex.IsMatch(idNumber, @"^\d{17}[\dX]$"))
				return false;

			// 验证生日
			string birth = idNumber.Substring(6, 8);

			if (!DateTime.TryParseExact(
					birth,
					"yyyyMMdd",
					null,
					System.Globalization.DateTimeStyles.None,
					out _))
				return false;

			// 校验码
			int[] weights =
			{
				7, 9, 10, 5, 8, 4, 2,
		        1, 6, 3, 7, 9, 10, 5,
		        8, 4, 2
	        };

			char[] codes =
			{
				'1','0','X','9','8','7','6','5','4','3','2'
	        };

			int sum = 0;

			for (int i = 0; i < 17; i++)
			{
				sum += (idNumber[i] - '0') * weights[i];
			}

			char checkCode = codes[sum % 11];

			return idNumber[17] == checkCode;
		}
	}
}
