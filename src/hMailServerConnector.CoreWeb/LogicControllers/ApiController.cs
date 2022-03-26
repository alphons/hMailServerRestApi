using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using hMailServerConnector.CoreWeb.DataModel;

//using hMailServer; // Interop.hMailServer

namespace hMailServerConnector.CoreWeb.LogicControllers
{
	public class ApiController : ControllerBase
	{

		[HttpPost]
		[Route("~/api/GetList")]
		public async Task<IActionResult> GetList(int Length)
		{
			await Task.Yield();

			if (Length < 0)
				return NotFound();

			var r = new Random();

			var List = Enumerable.Range(0, Length).Select(x => r.Next()).ToList();

			return Ok(new
			{
				List
			});
		}

		[HttpPost]
		[Route("~/api/OnClientConnect")]
		public async Task<IActionResult> OnClientConnect(ClientClass Client)
		{
			await Task.Yield();

			Debug.WriteLine($"OnClientConnect Ip:{Client.IPAddress}");

			var value = 0; // 1 = Reject

			return Ok(new
			{
				Value = value,
				Message = string.Empty
			});
		}

		[HttpPost]
		[Route("~/api/OnSMTPData")]
		public async Task<IActionResult> OnSMTPData(ClientClass Client, MessageClass Message)
		{
			await Task.Yield();

			Debug.WriteLine($"OnSMTPData Ip:{Client.IPAddress} From:{Message.FromAddress} To:{Message.Recipients?[0].Address} Total-Recipients:{Message.Recipients?.Count}");

			var value = 0;
			var message = string.Empty;

			if (Message.FromAddress != null)
			{

				if (Message.FromAddress.Contains('1'))
				{
					value = 1; // 542 
					message = string.Empty;
				}
				if (Message.FromAddress.Contains('2'))
				{
					value = 2;
					message = "Error 2";
				}
				if (Message.FromAddress.Contains('3'))
				{
					value = 3;
					message = "Error 3";
				}
			}

			return Ok(new
			{
				Value = value,
				Message = message
			});
		}


		[HttpPost]
		[Route("~/api/OnAcceptMessage")]
		public async Task<IActionResult> OnAcceptMessage(ClientClass Client, MessageClass Message)
		{
			await Task.Yield();

			Debug.WriteLine($"OnAcceptMessage Ip:{Client.IPAddress} From:{Message.FromAddress} To:{Message.Recipients?[0].Address} Total-Recipients:{Message.Recipients?.Count}");

			var Value = 0;

			//0 - hMailServer accepts the message
			//1 - hMailServer rejects the message with the error 542 Rejected
			//2 - hMailServer rejects the message with a script-defined error.

			return Ok(new
			{
				Value
			});
		}

		[HttpPost]
		[Route("~/api/OnDeliveryStart")]
		public async Task<IActionResult> OnDeliveryStart(MessageClass Message)
		{
			await Task.Yield();

			Debug.WriteLine($"OnDeliveryStart From:{Message.FromAddress} To:{Message.Recipients?[0].Address} Total-Recipients:{Message.Recipients?.Count}");

			var Value = 0;

			//0 - Deliver the message
			//1 - Do not deliver the message

			return Ok(new
			{
				Value
			});
		}

		[HttpPost]
		[Route("~/api/OnDeliverMessage")]
		public async Task<IActionResult> OnDeliverMessage(MessageClass Message)
		{
			await Task.Yield();

			Debug.WriteLine($"OnDeliverMessage From:{Message.FromAddress} To:{Message.Recipients?[0].Address} Total-Recipients:{Message.Recipients?.Count}");

			var Value = 0;
			//0 - Deliver the message
			//1 - Do not deliver the message

			return Ok(new
			{
				Value
			});
		}

		[HttpPost]
		[Route("~/api/OnDeliveryFailed")]
		public async Task<IActionResult> OnDeliveryFailed(MessageClass Message, string Recipient, string ErrorMessage)
		{
			await Task.Yield();

			Debug.WriteLine($"OnDeliveryFailed From:{Message.FromAddress} Recipient:{Recipient} ErrorMessage:{ErrorMessage}");

			return Ok();
		}

		[HttpPost]
		[Route("~/api/OnBackupFailed")]
		public async Task<IActionResult> OnBackupFailed(string Reason)
		{
			await Task.Yield();

			return Ok();
		}

		[HttpPost]
		[Route("~/api/OnBackupCompleted")]
		public async Task<IActionResult> OnBackupCompleted()
		{
			await Task.Yield();

			return Ok();
		}

		[HttpPost]
		[Route("~/api/OnError")]
		public async Task<IActionResult> OnError(int Severity, int Code, string Source, string Description)
		{
			await Task.Yield();

			return Ok();
		}

		[HttpPost]
		[Route("~/api/OnExternalAccountDownload")]
		public async Task<IActionResult> OnExternalAccountDownload(FetchAccountClass FetchAccount, MessageClass Message, string RemoteUID)
		{
			await Task.Yield();

			var Value = 0;
			//1 - Delete the message from the remote server immediately.
			//2 - Delete the message after a specified number of days. Set the number of days to Result.Parameter variable.
			//3 - Never delete messages from the remote server.

			return Ok(new
			{
				Value
			});
		}


	}
}
