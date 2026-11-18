namespace ShareProject.Response
{
	/// <summary>
	/// 客户注册成功的回应。
	/// </summary>
	public class RegisterCustomerResponse : BaseResponse
	{
		public int CustomerId { get; set; }
	}
}