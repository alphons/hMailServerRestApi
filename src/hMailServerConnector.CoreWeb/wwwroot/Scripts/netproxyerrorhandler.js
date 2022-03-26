// version 0.2.5 (last revision Nov, 2021)
window.NetProxyErrorHandler = ErrorHandler;
window.onerror = GlobalErrorhandler;

function ErrorHandler(url, status, Message, ExceptionType) 
{
	try
	{
		if (Message.indexOf("exceeded") > 0)
			return;
		if (url.indexOf("api/ErrorLog") < 0)
			window.netproxy("./api/ErrorLog",
				{
					"Message": Message,
					"ExceptionType": ExceptionType,
					"Referer": location.href,
					"Url": url,
					"Status": status
				});
	}
	catch (err) { }
}


function GlobalErrorhandler(Message, source, lineno, colno, error)
{
	try
	{
		if (url.indexOf("api/ErrorLog") < 0)
			window.netproxy("./api/ErrorLog",
				{
					"Message": Message,
					"ExceptionType": error,
					"Referer": "line:" + lineno + " col:" + colno,
					"Url": source,
					"Status": "javascript"
				});
	}
	catch (err) { }
}

