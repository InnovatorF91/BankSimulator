namespace ShareProject.Request
{
	public class RemoveCustomerRequest
	{
		public int Id { get; set; }
		public bool IsDeleted { get; set; }
		public string? DeletedReason { get; set; }
	}
}
