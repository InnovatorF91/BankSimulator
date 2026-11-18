using ShareProject.Common;

namespace ShareProject.Response
{
	/// <summary>
	/// 当前登录客户的资料回应。
	/// </summary>
	public class GetMyProfileResponse : BaseResponse
	{
		/// <summary>
		/// 客户 ID。
		/// </summary>
		public int CustomerId { get; set; }

		/// <summary>
		/// 客户姓名。
		/// </summary>
		public string Name { get; set; } = string.Empty;

		/// <summary>
		/// 性别。
		/// </summary>
		public Gender? Gender { get; set; }

		/// <summary>
		/// 出生日期。
		/// </summary>
		public DateTime BirthDate { get; set; }

		/// <summary>
		/// 身份证件类型。
		/// </summary>
		public IDType? IDType { get; set; }

		/// <summary>
		/// 身份证件号码。
		/// </summary>
		public string? IDNumber { get; set; }

		/// <summary>
		/// 地址。
		/// </summary>
		public string? Address { get; set; }

		/// <summary>
		/// 电话号码。
		/// </summary>
		public string? Phone { get; set; }

		/// <summary>
		/// 电子邮箱。
		/// </summary>
		public string? Email { get; set; }

		/// <summary>
		/// KYC 审核状态。
		/// </summary>
		public KYCStatus? KYCStatus { get; set; }

		/// <summary>
		/// 客户创建时间。
		/// </summary>
		public DateTime? CreatedAt { get; set; }

		/// <summary>
		/// 客户资料更新时间。
		/// </summary>
		public DateTime? UpdateAt { get; set; }
	}
}