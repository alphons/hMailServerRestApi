'use strict';

(function ()
{
	PageEvents();
	Init();
})();


function Init()
{

}

function PageEvents()
{
	document.addEventListener("click", function (e)
	{
		if (typeof window[e.target.id] === "function")
			window[e.target.id].call(e, e);
	});

	$id("Button1").on("click", function (e)
	{
		GetList();
	});
}

function GetList()
{
	netproxy("./api/GetList", { Length : 5 }, function ()
	{
		$id("Output").Template("TemplateList", this, false);
	});
}