using ShareProject.Common;

namespace ShareProject.Request
{
	public class UpdateBasicProfileRequest
	{
		public int Id { get; set; } // 客戶ID

		public string NewName { get; set; } = string.Empty; // 客戶

		public Gender Transgender { get; set; } // 性別

		public DateTime NewBirthDate { get; set; } // 出生日期
	}
}
