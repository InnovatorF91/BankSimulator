using System.Security.Cryptography;

namespace ServerProject.Common
{
	/// <summary>
	/// 生成16位纯数字模拟卡号。
	/// 唯一性由数据库约束保障。
	/// </summary>
	public class CardNumberGenerator : ICardNumberGenerator
	{
		public string Generate()
		{
			var digits = new char[16];

			// 第一位不为0。
			digits[0] = (char)('0' +
				RandomNumberGenerator.GetInt32(1, 10));

			for (var i = 1; i < digits.Length; i++)
			{
				digits[i] = (char)('0' +
					RandomNumberGenerator.GetInt32(0, 10));
			}

			return new string(digits);
		}
	}

	public interface ICardNumberGenerator
	{
		string Generate();
	}
}
