using ShareProject.Common;

namespace ShareProject.Request
{
	public class RegisterCustomerRequest
	{
		public string Name { get; set; } = string.Empty; // 客戶

		public Gender Gender { get; set; } // 性別

		public string Address { get; set; } = string.Empty; // 地址

		public DateTime BirthDate { get; set; } // 出生日期

		public short? IDType { get; set; } // 身份證明類型

		public string? IDNumber { get; set; } = string.Empty; // 身份證號碼

		public string? Phone { get; set; } = string.Empty; // 電話號碼

		public string? Email { get; set; } = string.Empty; // 電子郵件

		public KYCStatus KYCStatus { get; set; } // KYC狀態

		public string Password { get; set; } = string.Empty;// 密碼
	}
}
