namespace hMailServerConnector.CoreWeb.DataModel
{
	public class FetchAccountClass
	{
		public int ID { get; set; }
		public string? Name { get; set; }
		public string? ServerAddress { get; set; }
		public int Port { get; set; }
		public int ServerType { get; set; }
		public string? Username { get; set; }
		public string? Password { get; set; }
		public int MinutesBetweenFetch { get; set; }
		public int DaysToKeepMessages { get; set; }
		public int AccountID { get; set; }
		public bool Enabled { get; set; }
		public bool ProcessMIMERecipients { get; set; }
		public bool ProcessMIMEDate { get; set; }
		public bool UseSSL { get; set; }
		public string? NextDownloadTime { get; set; }
		public bool UseAntiSpam { get; set; }
		public bool UseAntiVirus { get; set; }
		public bool EnableRouteRecipients { get; set; }
		public bool IsLocked { get; set; }
	}
}
