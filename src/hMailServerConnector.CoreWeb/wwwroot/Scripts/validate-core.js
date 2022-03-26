// please use defer in script tag
// version 1.0 (dom)

var ValidateCallback; // user defined!!

const timeout = 1000;
const donetypingEvent = new Event("donetyping", { "bubbles": true, "cancelable": true });
const clickEvent = new Event("click", { "bubbles": true, "cancelable": true });
const changeEvent = new Event("change", { "bubbles": true, "cancelable": true });

var timeoutReference;

function typing(event)
{
	event = event || window.event;

	if (event.target === window)
		return;

	var el = event.target.closest("[data-mask]");
	if (!el)
		return;

	//console.log("typing:" + event.type + " el:" + el.outerHTML);

	if (event.type === 'blur')
	{
		if (el.dataset.oldvalue !== el.value)
			el.dispatchEvent(donetypingEvent);
		return;
	}

	//console.log("key:" + event.key);

	if (!event.key)
		return;

	if (event.type === 'keydown' && el.dataset.regexp && event.key.length === 1)
	{
		if (new RegExp(el.dataset.regexp).test(event.key) === false)
		{
			if (event.preventDefault)
				event.preventDefault();
			else
				event.returnValue = false;
		}
	}

	if (event.key.length > 1)
		return;

	if (el.dataset.mask.indexOf("dt") < 0)
		return;

	// here, single key having dt, start new timeout (clearing old)

	if (timeoutReference)
		clearTimeout(timeoutReference);
	
	timeoutReference = setTimeout(function ()
	{
		el.setAttribute("data-oldvalue", el.value);
		el.dispatchEvent(donetypingEvent);
	}, timeout);
}

function validator(event)
{
	event = event || window.event;

	if (event.target === window)
		return;

	var el = event.target.closest("[data-mask]");
	if (!el)
		return;

	var context = el.closest("[data-context]");
	var contextid = context ? context.id : "";

	var vall = event.target.closest("[data-value]");
	if (vall)
	{
		el.dataset.selectedvalue = vall.dataset.value;
	}
	else
	{
		vall = event.target.closest("[value]");
		if (vall)
		{
			el.dataset.selectedvalue = vall.getAttribute("value");
		}
		else
		{
			if (event.target.getAttribute("type") === "checkbox")
				el.dataset.selectedvalue = event.target.checked;
			else
				el.dataset.selectedvalue = event.target.value;// WTF?
		}
	}

	if (el.dataset.mask === "rbl")
	{
		if (event.type !== "change")
			return;
		//mask staat op de wrapper div. De id van de el = name van de radiobuttongroup
		el.querySelectorAll("label").forEach(function (label) { label.classList.remove("active"); });
		var rb = el.querySelector("input:checked");
		if (rb)
			rb.parentElement.classList.add("active"); // is rb.parent() a label?
		Validate(contextid, el);
		return;
	}

	if (el.dataset.mask === "options")
	{
		if (event.type !== "click")
			return;
		var valelement = event.target.closest("[data-value]");
		if (valelement)
		{
			el.querySelectorAll("[data-value]").forEach(function (x) { x.classList.remove("active"); });
			valelement.classList.add("active");
		}
		else
		{
			valelement = event.target.closest("[value]");
			if (valelement)
			{
				el.querySelectorAll("[value]").forEach(function (x) { x.classList.remove("active"); });
				valelement.classList.add("active");
			}
			else
			{
				// Wtf?
			}
		}
		Validate(contextid, el);
		return;
	}

	if (event.type === "click")
		return;

	// no change event

	if (el.dataset.autocomplete)
		(new Function('return ' + el.dataset.autocomplete)())(contextid, src);
	else
		Validate(contextid, el);
}

// el = maskedelement
function GetValidateArguments(el)
{
	if (el === window)
		return null;

	var name = el.getAttribute("name");

	var returnValue =
	{
		id: name ? name : el.id,
		value: el.dataset.selectedvalue,
		mask: el.dataset.mask
	};
	return returnValue;
}

function Safe(context, id, value)
{
	var element = context.querySelector("#" + id);
	if (element)
	{
		element.setAttribute("data-valid", "true");
		element.value = value;
	}
	var error = context.querySelector("#" + id + "Error");
	if (error)
	{
		error.innerText = "";
	}
}

function ValidateResult(contextid, args, data)
{
	IsValidateResult = true;

	var context = document.getElementById(contextid);
	if (!context)
		context = document;

	// el = maskedelement
	var el = context.querySelector('#' + args.id);
	if (!el)
		el = context.querySelector("[name='" + args.id + "']");

	if (!el)
	{
		IsValidateResult = null;
		return;
	}

	el.setAttribute("data-selectedvalue", data.Value);

	if (data.Valid)
		el.setAttribute("data-valid", "true");
	else
		el.removeAttribute("data-valid");

	var errorelement = context.querySelector('#' + args.id + "Error");
	if (errorelement)
		errorelement.innerHTML = data.Msg;

	data["Id"] = args.id;
	data["Mask"] = args.mask;

	if (data.Resource)
	{
		Safe(context, "PostalCode" + data.Postfix, data.Resource.PostalCode);
 		Safe(context, "Street" + data.Postfix, data.Resource.Street);
		Safe(context, "City" + data.Postfix, data.Resource.City);
		Safe(context, "Municipality" + data.Postfix, data.Resource.Municipality);
		Safe(context, "Region" + data.Postfix, data.Resource.Region);
		Safe(context, "Country" + data.Postfix, data.Resource.Country);
	}

	switch (args.mask)
	{
		default: // text,textarea
			el.value = data.Value;
			//el.dispatchEvent(changeEvent);
			break;
		case "ddl":
			el.value = data.Value;
			//el.dispatchEvent(changeEvent);
			break;
		case "options":
			var opt = el.querySelector("[value='" + data.Value + "']");
			//opt.dispatchEvent(clickEvent);
			break;
		case "rbl":
			var rbl = el.querySelector("input[type='radio'][value='" + data.Value + "']");
			if (!rbl) //radio button within wrapper having data-value === data.Value
				rbl = el.querySelector("[data-value='" + data.Value + "'] input[type='radio']");
			if (rbl)
				rbl.checked = true;
			break;
	}

	if (ValidateCallback)
		ValidateCallback.call(this, contextid, args, data);

	IsValidateResult = null;
}

function DoneTyping(contextid, el)
{
	if (!timeoutReference)
		return;

	timeoutReference = null;

	Validate(contextid, el);
}

function Validate(contextid, el)
{
	if (typeof IsValidateResult === 'undefined')
		IsValidateResult = null;

	// dont start new validate when on return (prevents validation loops)
	if (IsValidateResult)
		return;

	// el = maskedelement
	var args = GetValidateArguments(el);

	if (args === null)
		return;

	args["contextid"] = contextid;

	netproxy("./api/Validate", args, function ()
	{
		ValidateResult(contextid, args, this);
	});
}


function MultiValidate(contextid, validates, validcallback, invalidcallback)
{
	if (validates.length === 0)
		return;

	netproxy("./api/MultiValidate", { contextid: contextid, validates: validates }, function ()
	{
		var data = {
			contextid: contextid,
			validates: validates,
			results: this.Results
		};

		for (intI = 0; intI < validates.length; intI++)
			ValidateResult(contextid, validates[intI], this.Results[intI]);

		if (this.AllValid)
		{
			if (validcallback)
				validcallback.call(data);
		}
		else
		{
			if (invalidcallback)
				invalidcallback.call(data);
		}
	});
}

function FillPanel(contextid, data, validcallback, invalidcallback)
{
	var context = document.getElementById(contextid);
	if (!context)
		context = document;

	var validates = [];
	if (Array.isArray(data))
	{
		data.forEach(function (item)
		{
			var el = context.querySelector("#" + item.Key);
			if (!el)
				el = context.querySelector("[name='" + item.Key + "']");
			if (!el)
				return; // next

			// el = maskedelement

			el.removeAttribute("data-valid"); // only after multivalidate
			el.dataset.selectedvalue = item.Value;

			switch (el.dataset.mask)
			{
				default:
					if (el.getAttribute("type") === "checkbox" && (item.Value === "true" || item.Value === "on"))
						el.setAttribute("checked", "checked");
					else
						el.value = item.Value;
					break;
				case "ddl":
					el.value = item.Value;
					break;
				case "options":
					var dd = el.querySelector("[data-value='" + item.Value + "']");
					if (dd)
					{
						dd.classList.add("active");
					}
					else
					{
						dd = el.querySelector("[value='" + item.Value + "']");
						if (dd)
							dd.classList.add("active");
					}
					break;
				case "rbl":
					var inp = el.closest("input[data-value='" + item.Value + "']");
					if (!inp)
						inp = el.querySelector("input[value='" + item.Value + "']");
					if (inp)
						inp.checked = true;
					break;
				case "eur":
					el.value = FormatDutch(item.Value);
					break;
			}
			if (el.style.visibility === "visible" || el.style.visibility === "")
				validates.push(GetValidateArguments(el));
		});
	}

	MultiValidate(contextid, validates, validcallback, invalidcallback);
}




// =======================================
// Functions below can be called directly
// =======================================



// get all values which has to be validated and calls multivalidate (NO returnvalue)
function ValidateContext(contextid, validcallback, invalidcallback)
{
	var context = document.getElementById(contextid);
	if (!context)
		return;

	var wrongs = context.querySelectorAll("[data-mask]:not([data-valid])");

	// todo, de disabled eruit filteren

	if (wrongs.length === 0)
	{
		if (validcallback)
		{
			var data = {
				contextid: contextid,
				validates: [],
				results: []
			};
			validcallback.call(data);
		}
		return;
	}

	var validates = [];
	wrongs.forEach(function (el)
	{
		validates.push(GetValidateArguments(el));
	});

	MultiValidate(contextid, validates, validcallback, invalidcallback);
}



// Gets all cached values and does a FillPanel (which starts Multivalidate)
function InitContext(contextid, validcallback, invalidcallback)
{
	netproxy("./api/InitPanel", { contextid: contextid }, function ()
	{
		FillPanel(contextid, this.Dict, validcallback, invalidcallback);
	});
}

function ClearContext(contextid)
{
	var context = document.getElementById(contextid);
	context.querySelectorAll("input,textarea").forEach(function (item) { item.value = ''; });
	context.querySelectorAll("select").forEach(function (item) { item.value = '-1'; });
}

(function ()
{
	'use strict';

	// hook donetyping events
	document.addEventListener('keydown', typing, false);
	document.addEventListener('paste', typing, false);
	document.addEventListener('blur', typing, false);

	document.addEventListener('donetyping', validator, false);
	document.addEventListener('change', validator, false); // also rbl
	document.addEventListener('click', validator, false); // only options
})();
