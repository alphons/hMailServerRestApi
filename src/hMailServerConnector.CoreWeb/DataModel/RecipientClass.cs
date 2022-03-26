namespace hMailServerConnector.CoreWeb.DataModel
{
	public class RecipientClass
	{
		public string? Address { get; set; }
		public bool IsLocalUser { get; set; }
		public string? OriginalAddress { get; set; }
	}
}
