
using System.Text.Json;
using System.Diagnostics;

namespace hMailServerConnector.CoreWeb.DataService
{
	public interface IErrorManager : IDisposable
	{
		Task<List<ErrorManager.ErrorLog?>> ErrorLogsAsync();
		int Add(ErrorManager.ErrorLog errorlog);
		void Info(string m);
	}



	public class ErrorManager : IErrorManager
	{
		public enum ErrorTypeEnum
		{
			Unknown = 0,
			Info = 1,
			Exception = 2
		}

		public  class ErrorLog
		{
			public int Id { get; set; }

			//[Column(TypeName = "datetime2")]
			public DateTime DateTime { get; set; }
			public string? Name { get; set; }
			public string? UserAgent { get; set; }
			public string? Ip { get; set; }
			public string? Url { get; set; }

			// ClientNotify
			public string? Referer { get; set; }
			public ErrorTypeEnum ErrorType { get; set; }

			// Exception
			public string? Message { get; set; }
			public string? ExceptionType { get; set; }
			public string? StackTrace { get; set; }
		}

		private readonly List<ErrorLog> errorlogDb;

		private bool disposedValue;

		private int LastId;

		private bool IsDirty;

		private readonly string m_DirectoryPath;

		private DateTime? m_logFileDate;

		private readonly object lockObject = new();
		public ErrorManager()
		{
			this.errorlogDb = new();

			m_DirectoryPath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) + @"\Logs\";

			try
			{
				if (!Directory.Exists(m_DirectoryPath))
					Directory.CreateDirectory(m_DirectoryPath);
				System.Timers.Timer timer = new(5000);
				timer.Elapsed += (sender, e) => SaveChanges();
				timer.Start();
			}
			catch
			{

			}
		}

		public async Task<List<ErrorLog?>> ErrorLogsAsync()
		{
			List<ErrorLog?> list = new();

			var sr = new StreamReader(GetFilePath());

			while(!sr.EndOfStream)
			{ 
				var line = await sr.ReadLineAsync();
				if (string.IsNullOrWhiteSpace(line))
					break;
				list.Add(JsonSerializer.Deserialize<ErrorLog>(line));
			}
			return list;
		}

		private string GetFilePath()
		{
			if (m_logFileDate == null || m_logFileDate.Value != DateTime.Today)
				m_logFileDate = DateTime.Today;

			return $"{m_DirectoryPath}{DateTime.Now.Year}-{DateTime.Now.Month:00}-{DateTime.Now.Day:00}.log";

		}

		private void SaveChanges()
		{
			if (this.IsDirty == false)
				return;

			try
			{
				lock (lockObject)
				{
					using var sw = new StreamWriter(GetFilePath(), true); // append
					foreach (var l in this.errorlogDb)
						sw.WriteLine(JsonSerializer.Serialize(l));
					this.errorlogDb.Clear();
					this.IsDirty = false;
				}
			}
			catch
			{

			}
		}

		public int Add(ErrorLog errorlog)
		{
			lock (lockObject)
			{
				this.LastId++;

				errorlog.Id = this.LastId;

				this.errorlogDb.Add(errorlog);

				this.IsDirty = true;

				return errorlog.Id;
			}
		}

		public void Info(string m)
		{
			Add(new ErrorLog()
			{
				DateTime = DateTime.Now,
				ErrorType = ErrorTypeEnum.Info,
				Message = m
			});
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
					SaveChanges();

				this.errorlogDb.Clear();
				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Debug.WriteLine("ErrorManager.Dispose()");
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
