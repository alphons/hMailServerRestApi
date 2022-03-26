namespace hMailServerConnector.CoreWeb.DataModel
{
	public class MessageClass
	{
		public long ID { get; set; }
		public string? Filename { get; set; }
		public string? Subject { get; set; }
		public string? From { get; set; }
		public string? Date { get; set; }
		public string? Body { get; set; }
		public string? HTMLBody { get; set; }
		public AttachmentsClass? Attachments { get; set; }
		public string? To { get; set; }
		public string? FromAddress { get; set; }
		public int State { get; set; }
		public int Size { get; set; }
		public string? CC { get; set; }
		public RecipientsClass? Recipients { get; set; }
		public string? HeaderValue { get; set; }
		public bool EncodeFields { get; set; }
		public bool Flag { get; set; }
		public object? InternalDate { get; set; }
		public MessageHeadersClass? Headers { get; set; }
		public int DeliveryAttempt { get; set; }
		public string? Charset { get; set; }
		public int UID { get; set; }
	}
}
