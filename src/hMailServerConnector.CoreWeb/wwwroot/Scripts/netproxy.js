/**
 * 
 *	@name		pure-dom netproxy and template and api
 * 
 *	@author     Alphons van der Heijden <alphons@heijden.com>
 *	@version    0.2.7 (last revision Nov, 2021)
 *	@copyright  (c) 2019-2021 Alphons van der Heijden
 *	@license    LGPL-3.0
 *	@alias      netproxy
 *	@class
 * 
 */

// please use defer in script tag
(function ()
{
	'use strict';

	window.netproxy = function (url, data, onsuccess, onerror, onprogress)
	{
		if (typeof window.XMLHttpRequest === 'undefined')
			return;

		var spinner = document.getElementById("NetProxySpinner");

		if (typeof remote !== 'undefined')
			url = remote + url;

		var cross = url.indexOf(window.location.host) < 0 && url[0] !== '/';

		var defaults =
		{
			async: true,
			url: url,
			method: data ? 'POST' : 'GET',
			cross: cross,
			onsuccess: onsuccess,
			onerror: onerror ? onerror : window.NetProxyErrorHandler,
			spinner: spinner,
			data: data instanceof FormData ? data : JSON.stringify(data),
			timeoutSpinner: spinner ? window.setTimeout(function () { spinner.style.display = 'block'; }, 1000) : null
		};

		var httpRequest;
		var response;

		var callback = function () 
		{
			if (httpRequest.readyState !== 4)
				return;

			if (defaults.timeoutSpinner !== null)
				window.clearTimeout(defaults.timeoutSpinner);

			if (defaults.spinner)
				defaults.spinner.style.display = 'none';

			try
			{
				response = JSON.parse(httpRequest.response);
				if (typeof response !== "object")
					response = httpRequest.response;
			}
			catch (e)
			{
				response = httpRequest.response;
			}

			if (httpRequest.status === 200)
			{
				var header = httpRequest.getResponseHeader('Content-Disposition');
				if (header)
				{
					var a = document.createElement('a');
					a.download = header.split('filename="')[1].split('"')[0];
					a.rel = 'noopener' // tabnabbing
					// a.target = '_blank'
					const blob = new Blob([httpRequest.response], { type: 'application/octet-stream' });
					a.href = URL.createObjectURL(blob)
					setTimeout(function () { URL.revokeObjectURL(a.href) }, 4E4) // 40s
					setTimeout(function () { a.click(); }, 0)
					return;
				}

				if (typeof defaults.onsuccess === 'function')
				{
					if (response.d)
						defaults.onsuccess.call(response.d, response.d);
					else
						defaults.onsuccess.call(response, response);
				}
			}
			else // !=200
			{
				var message = response;
				var exceptionType = "Html";
				if (typeof response === 'object')
				{
					message = response.Message;
					exceptionType = response.ExceptionType;
				}

				if (typeof defaults.onerror === 'function')
					defaults.onerror.call(null, defaults.url, httpRequest.status, message, exceptionType);
				else
				{
					//	alert(httpRequest.status + " ==> " + response);
				}
			}
		};

		httpRequest = new XMLHttpRequest();
		httpRequest.onreadystatechange = callback;
		httpRequest.open(defaults.method, defaults.url, defaults.async);
		httpRequest.withCredentials = defaults.cross;
		if (onprogress)
		{
			httpRequest.addEventListener('progress', onprogress, false);
			httpRequest.upload.addEventListener('progress', onprogress, false);
		}
		if (!(data instanceof FormData))
			httpRequest.setRequestHeader('Content-Type', 'application/json; charset=utf-8');
		httpRequest.send(defaults.data);
		return this;
	};

	Element.prototype.Template = function (template, data, append)
	{
		var strHtml = window.TemplateHtml(template, data);
		if (append)
		{
			var temp = document.createElement("span");
			this.insertAdjacentElement('beforeend', temp);
			temp.outerHTML = strHtml;
			return this;
		}
		else
			return this.innerHTML = strHtml;
	};

	window.TemplateHtml = function (template, data)
	{
		try
		{
			var element = typeof template === "string" ?
				document.getElementById(template) : template;
			if (!element)
				return null;
			if (!element.jscache)
			{
				var html = element.innerHTML.replace(/[\t\r\n]/g, " ");
				var js = "var _='';";
				var direct, intI, intJ = 0;
				while (intJ < html.length)
				{
					intI = html.indexOf("{{", intJ);
					if (intI < 0)
						break;
					js += "_+='" + html.substring(intJ, intI).replace(/\'/g, "\\'") + "';";
					intJ = intI + 2;
					direct = (html[intJ] === "=");
					if (direct)
						intJ++;
					intI = html.indexOf("}}", intJ);
					if (intI < 0)
						break;
					if (direct)
						js += "_+=";
					js += html.substring(intJ, intI).trim() + ";";
					intJ = intI + 2;
				}
				js += "_+='" + html.substring(intJ).replace(/\'/g, "\\'") + "'; return _;";
				element.jscache = new Function(js);
				element.innerHTML = '';
			}
			return element.jscache.call(data);
		}
		catch (err)
		{
			if (console)
				console.log("Error: " + element.outerHTML + ": " + err.message);
			return "";
		}
	};

	window.tmpl = function (url, data, outputid)
	{
		window.netproxy(url, data, function ()
		{
			if (!outputid)
				return this;
			var t = document.getElementById(outputid);
			if (t === null)
			{
				alert('Output element does not exist');
				return;
			}

			if (t.attributes['type'] === undefined)
			{
				t.innerHTML = this.Html;
			}
			else
			{
				if (t.attributes['type'].value === "text/template")
				{
					var el = document.getElementById(outputid + "__");
					if (el === null)
					{
						var newNode = document.createElement("div");
						newNode.setAttribute("id", outputid + "__");
						t.parentNode.insertBefore(newNode, t.nextSibling);
						el = document.getElementById(outputid + "__");
					}
					el.innerHTML = window.TemplateHtml(t, this);
				}
			}
		});
	}

	window.api = function (Url, Datain, Output, Script)
	{
		var url = Url;
		var datain = Datain
		var output = Output;
		var js = Script;
		var jscache;

		if (event !== undefined)
		{
			event.preventDefault();
			var el = event.target;
			if (el.dataset)
			{
				url = url ? url : el.dataset.url;
				datain = datain ? datain : el.dataset.in;
				js = js ? js : el.dataset.script;

				if (el.dataset.out)
					output = el.dataset.out;
			}

			if (output === undefined)
				output = el;
		}

		if (datain)
		{
			if (typeof datain === "string")
				datain = JSON.parse(datain.replace(/\'/g, '\"'));
		}
		else
			datain = null; // get

		window.netproxy(url, datain, function ()
		{
			if (js)
			{
				if (typeof js === "string")
				{
					jscache = new Function(js)
					jscache.call(this);
					jscache = undefined;
				}
				else
				{
					js.call(this);
				}
				return;
			}
			if (output)
			{
				if (output === "null")
					return;

				if (typeof output === "object")
				{
					output.innerHTML = this;
					return;
				}
				var t = document.getElementById(output);
				if (t === null)
				{
					alert('Output element does not exist');
					return;
				}

				if (t.attributes['type'] === undefined && this.Template)
				{
					if (this.Template)
					{
						jscache = new Function(this.Template);
						if (this.Output)
							this.Output.innerHTML += jscache.call(this);
						else
							t.innerHTML = jscache.call(this);
						jscache = undefined;
					}
					else
					{
						t.innerHTML = this.Html;
					}
				}
				else
				{
					if (t.attributes['type'].value === "text/template")
					{
						var el = document.getElementById(output + "__");
						if (el === null)
						{
							var newNode = document.createElement("div");
							newNode.setAttribute("id", output + "__");
							t.parentNode.insertBefore(newNode, t.nextSibling);
							el = document.getElementById(output + "__");
						}
						el.innerHTML = window.TemplateHtml(t, this);
					}
					else
					{
						alert('Unknown type attribute on output element');
					}
				}
			}
		});
	}

	window.rem = function (Url, Datain)
	{
		var url = Url;
		var datain = Datain

		if (datain)
		{
			if (typeof datain === "string")
				datain = JSON.parse(datain.replace(/\'/g, '\"'));
		}
		else
			datain = null; // get

		window.netproxy(url, datain, function ()
		{
			var jscache = new Function(this.Template);
			var html = jscache.call(this);
			jscache = undefined;
			return html;
		});
	}

})();