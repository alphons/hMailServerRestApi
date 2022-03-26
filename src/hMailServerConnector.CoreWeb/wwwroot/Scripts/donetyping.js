// donetyping-plugin 1.2 plugin (C) 2016 Moneywise, adapted by Alphons van der Heijden
(function ($)
{
	$.fn.donetyping = function (callback, timeout)
	{
		timeout = timeout || 1e3;
		var timeoutReference,
			doneTyping = function (el)
			{
				if (!timeoutReference) return;
				timeoutReference = null;
				callback.call(el);
			};
		return this.each(function (i, el)
		{
			var $el = $(el);
			$el.is(':input') && $el.on('keyup keypress paste input', function (e)
			{
				if (e.type == 'keyup' && e.keyCode != 8 && e.keyCode != 229) return;

				if (timeoutReference) clearTimeout(timeoutReference);
				timeoutReference = setTimeout(function ()
				{
					doneTyping(el);
				}, timeout);
			}).on('blur', function ()
			{
				doneTyping(el);
			});
		});
	};
}(jQuery));
