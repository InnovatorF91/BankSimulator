using ShareProject.Common;

namespace ServerProject.DTOs
{
	/// <summary>
	/// 客戶資料傳輸物件
	/// </summary>
	public class CustomerDto : DtoBase
	{
		public int CustomerId { get; set; } // 客戶ID
		public string Name { get; set; } = string.Empty; // 客戶名稱
		public Gender? Gender { get; set; } // 性別
		public DateTime BirthDate { get; set; } // 出生日期
		public IDType? IDType { get; set; } // 身份證明類型
		public string? IDNumber { get; set; } = string.Empty; // 身份證號碼
		public string? Address { get; set; } = string.Empty; // 地址
		public string? Phone { get; set; } = string.Empty; // 電話號碼
		public string? Email { get; set; } = string.Empty; // 電子郵件
		public KYCStatus? KYCStatus { get; set; } // KYC狀態
		public DateTime? CreatedAt { get; set; } // 創建時間
		public DateTime? UpdateAt { get; set; } // 更新日期
		public DateTime? DeletedAt { get; set; } // 刪除日期
		public bool IsDeleted { get; set; } // 是否已刪除
		public string? DeletedReason { get; set; } = string.Empty; // 刪除原因

		/// <summary>
		/// 建立一個成功的 CustomerDto 實例
		/// </summary>
		/// <param name="customerId">客戶ID</param>
		/// <param name="name">客戶名稱</param>
		/// <param name="birthDate">出生日期</param>
		/// <param name="gender">性別</param>
		/// <param name="idType">身份證明類型</param>
		/// <param name="idNumber">身份證號碼</param>
		/// <param name="address">地址</param>
		/// <param name="phone">電話號碼</param>
		/// <param name="email">電子郵件</param>
		/// <param name="kycStatus">KYC狀態</param>
		/// <param name="createdAt">創建時間</param>
		/// <param name="updateAt">更新日期</param>
		/// <param name="deletedAt">刪除日期</param>
		/// <param name="isDeleted">是否已刪除</param>
		/// <param name="deletedReason">刪除原因</param>
		/// <returns>成功的 CustomerDto 實例</returns>
		public static CustomerDto SuccessDto(
									int customerId,
									string name,
									DateTime birthDate,
									Gender? gender = null,
									IDType? idType = null,
									string? idNumber = null,
									string? address = null,
									string? phone = null,
									string? email = null,
									KYCStatus? kycStatus = null,
									DateTime? createdAt = null,
									DateTime? updateAt = null,
									DateTime? deletedAt = null,
									bool isDeleted = false,
									string? deletedReason = null)
		{
			var dto = new CustomerDto
			{
				CustomerId = customerId,
				Name = name,
				BirthDate = birthDate,
				Gender = gender,
				IDType = idType,
				IDNumber = idNumber,
				Address = address,
				Phone = phone,
				Email = email,
				KYCStatus = kycStatus,
				CreatedAt = createdAt,
				UpdateAt = updateAt,
				DeletedAt = deletedAt,
				IsDeleted = isDeleted,
				DeletedReason = deletedReason
			};
			dto.MarkSuccess();
			return dto;
		}

		public static CustomerDto Fail(
									int code,
									string message)
		{
			var dto = new CustomerDto();
			dto.MarkFail(code, message);
			return dto;
		}
	}
}
