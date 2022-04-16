
using System.Diagnostics;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;

using hMailServerConnector.CoreWeb.DataService;

namespace hMailServerConnector.CoreWeb.LogicControllers
{
	public class ErrorController : ControllerBase
	{
		private const int PageSize = 25;
		
		readonly IErrorManager errrorManager;

		public ErrorController(IErrorManager errrorManager)
		{
			this.errrorManager = errrorManager;
		}

		private static string? GetClientIPAddress(HttpRequest request)
		{
			var ipAddresses = request.HttpContext.Connection.RemoteIpAddress;//.Headers..ServerVariables["HTTP_X_FORWARDED_FOR"];
			if (ipAddresses != null)
			{
				var ipAddress = ipAddresses.ToString();
				var addresses = ipAddress.Split(',');
				return addresses[0];
			}
			return null; // request.ServerVariables["REMOTE_ADDR"];
		}

		private static string GetUserAgent(HttpRequest request)
		{
			return string.Empty + request.Headers["User-Agent"];
		}


		public async Task<bool> ErrorLogAsync(ErrorManager.ErrorLog errorlog)
		{
			try
			{
				await Task.Yield();

				var id = errrorManager.Add(errorlog);

				return id != 0;
			}
			catch
			{
				return true;
			}
		}

		[DebuggerNonUserCode]
		[Route("~/internalerror")]
		public async Task<IActionResult> LogInternalError()
		{
			var context = HttpContext.Features.Get<IExceptionHandlerFeature>();

			if (context == null)
				return StatusCode(500);

			var exception = context.Error; // Your exception

			var message = exception.Message;

			if (exception.InnerException != null)
				message += Environment.NewLine + exception.InnerException.Message;

			var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

			var errorlog = new ErrorManager.ErrorLog()
			{
				DateTime = DateTime.Now,
				ErrorType = ErrorManager.ErrorTypeEnum.Exception,
				Name = "HTTP 500",
				Message = message,
				ExceptionType = exception.GetType().Name,
				StackTrace = exception.StackTrace ?? "no-stacktrace",
				Url = exceptionHandlerPathFeature?.Path ?? "no-path",
				Ip = GetClientIPAddress(HttpContext.Request) ?? "no-ip",
				UserAgent = GetUserAgent(HttpContext.Request)
			};

			await ErrorLogAsync(errorlog);

			return StatusCode(500, exception.Message);
		}

		[HttpPost]
		[Route("~/error/ErrorLog")]
		public async Task<IActionResult> ErrorLog(string Message, string ExceptionType, string Referer, string Url, string Status)
		{
			if (Status == "0")
				return Ok();
			try
			{
				var errorlog = new ErrorManager.ErrorLog()
				{
					DateTime = DateTime.Now,
					Name = "HTTP " + Status,
					ErrorType = ErrorManager.ErrorTypeEnum.Info,
					Message = Message,
					ExceptionType = ExceptionType,
					Referer = Referer,
					Url = Url,
					Ip = GetClientIPAddress(HttpContext.Request) ?? "no-ip",
					UserAgent = GetUserAgent(HttpContext.Request),
					StackTrace = "netproxy"
				};

				await ErrorLogAsync(errorlog);
			}
			catch
			{
			}

			return Ok();
		}


		[HttpPost]
		[Route("~/error/MakeError")]
		public async Task<IActionResult> MakeError()
		{
			await Task.Yield();

			var i = 1;
			if (i == 1)
				throw new Exception("This is an error from ErrorLogController");

			return Ok();

		}


		private async Task<List<ErrorManager.ErrorLog>> GetErrorsInternalAsync(int Page, string Search)
		{
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
			return (await errrorManager.ErrorLogsAsync()).Where(x =>
			(x.Message != null && x.Message.Contains(Search)) ||
			(x.Name != null && x.Name.Contains(Search)) ||
			(x.Referer != null && x.Referer.ToString().Contains(Search)) ||
			(x.Url != null && x.Url.ToString().Contains(Search))
			).OrderBy(x => x.Id)
				.Skip(Page * PageSize)
				.Take(PageSize)
				.ToList();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
		}

		[HttpPost]
		[Route("~/error/GetErrors")]
		public async Task<IActionResult> GetErrors(int Page, string Search)
		{
			if (Page < 0)
				Page = 0;

			var List = await GetErrorsInternalAsync(Page, Search);

			if (List.Count == 0)
			{
				if (Page > 0)
				{
					Page--;
					List = await GetErrorsInternalAsync(Page, Search);
				}
			}

			return Ok(new
			{
				Page,
				List
			});
		}


	}
}

