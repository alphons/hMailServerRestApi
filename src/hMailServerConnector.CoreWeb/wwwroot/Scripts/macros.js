// version 1.3 2020-08-30 (C) AAB van der Heijden

// poly begin IE11
if (!Element.prototype.matches)
{
	Element.prototype.matches = Element.prototype.msMatchesSelector ||
		Element.prototype.webkitMatchesSelector;
}

if (!Element.prototype.closest)
{
	Element.prototype.closest = function (s)
	{
		var el = this;

		do
		{
			if (Element.prototype.matches.call(el, s)) return el;
			el = el.parentElement || el.parentNode;
		} while (el !== null && el.nodeType === 1);
		return null;
	};
}

if (window.NodeList && !NodeList.prototype.forEach)
{
	NodeList.prototype.forEach = Array.prototype.forEach;
}
// Poly end 

function $(querySelector)
{
	return document.querySelector(querySelector);
}

function $id(id)
{
	return document.getElementById(id);
}

function $qs(querySelector)
{
	return document.querySelector(querySelector);
}

function $qsall(querySelector)
{
	return document.querySelectorAll(querySelector);
}

Element.prototype.qs = function(querySelector)
{
	return this.querySelector(querySelector);
}

Element.prototype.qsall = function (querySelector)
{
	return this.querySelectorAll(querySelector);
}

Element.prototype.show = function ()
{
	this.style.display = "inline-block";
	return this;
}

Element.prototype.hide = function ()
{
	this.style.display = "none";
	return this;
}

Element.prototype.on = function (type, listener)
{
	this.addEventListener(type, listener);
	return this;
};

Element.prototype.removeClass = function (name)
{
	this.classList.remove(name);
	return this;
};

Element.prototype.addClass = function (name)
{
	this.classList.add(name);
	return this;
};

Element.prototype.hasClass = function (name)
{
	return this.classList.contains(name);
};

Element.prototype.toggleClass = function (name)
{
	this.classList.toggle(name);
	return this;
};

NodeList.prototype.addClass = function (name)
{
	this.forEach(function (el)
	{
		el.classList.add(name);
	});
	return this;
};

NodeList.prototype.removeClass = function (name)
{
	this.forEach(function (el)
	{
		el.classList.remove(name);
	});
	return this;
};

NodeList.prototype.on = function (type, listener)
{
	this.forEach(function (el)
	{
		el.addEventListener(type, listener);
	});
	return this;
};

NodeList.prototype.show = function ()
{
	this.forEach(function (el)
	{
		el.style.display = "inline-block";
	});
	return this;
};

NodeList.prototype.hide = function ()
{
	this.forEach(function (el)
	{
		el.style.display = "none";
	});
	return this;
};