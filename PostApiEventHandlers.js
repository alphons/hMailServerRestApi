// (C) Alphons van der Heijden
// 2022-03-24 Version 1.1

var APIBASE = "http://127.0.0.1:5022";

function CallApi(Method, json, Result)
{
	var i, j, k, l, http, status, response;
	var http = new ActiveXObject("Msxml2.ServerXMLHTTP");
	try	
	{
		http.open("POST", APIBASE + Method, false);
		http.setRequestHeader("Content-Type", "application/json");
		http.send(json);
	} catch (e)
	{
		return;
	}
	if (Result == undefined)
		return;
	status = http.status;
	response = http.responseText;
	http = undefined;
	//WScript.Echo(status + " " + response);
	if (status == 200)
	{
		i = response.indexOf(":", 1) + 1;
		j = response.indexOf(",", i);
		k = response.indexOf(":", j) + 2;
		l = response.indexOf("}", k);
		if (i > 0 && j > 0)
			Result.value = response.substring(i, j);
		if (k > 0 && l > 0)
			Result.message = response.substring(k, l - 1);
	}
}

function Log(s)
{
	var fso = new ActiveXObject("Scripting.FileSystemObject");
	var tf = fso.CreateTextFile("d:\\temp\\debug.txt", true);
	tf.WriteLine(s);
	tf.Close();
}

function SerializeClient(oClient)
{
	var j = '"Client": {';
	j += '"IPAddress": "' + oClient.IPAddress + '",';
	j += '"Username": "' + oClient.Username + '",';
	j += '"HELO": "' + oClient.HELO + '",';
	j += '"Port": "' + oClient.Port + '"';
	j += '}';
	return j;
}

function SerializeMessage(oMessage)
{
	var i, j = '"Message": {';
	j += '"FromAddress": "' + oMessage.FromAddress + '",' +
		'"Recipients":[';
	for (i = 0; i < oMessage.Recipients.Count; i++)
	{
		if (i > 0)
			j += ',';
		j += '{ "Address": "' + oMessage.Recipients.item(i).Address + '" }';
	}
	j += '] }';
	return j;
}

function SerializeFetchAccount(oFetchAccount)
{
	var j = '"FetchAccount": {';
	j += '"ID": "' + oFetchAccount.ID + '",';
	j += '"Name": "' + oFetchAccount.Name + '",';
	// more to come
	j += '"IsLocked": "' + oFetchAccount.IsLocked + '"';
	j += '}';
	return j;
}

function Escape(str)
{
	return str
		.replace(/[\\]/g, '\\\\')
		.replace(/[\"]/g, '\\\"')
		.replace(/[\/]/g, '\\/')
		.replace(/[\b]/g, '\\b')
		.replace(/[\f]/g, '\\f')
		.replace(/[\n]/g, '\\n')
		.replace(/[\r]/g, '\\r')
		.replace(/[\t]/g, '\\t');
}

// All the events

function OnClientConnect(oClient)
{
	var json = '{' + SerializeClient(oClient) + '}';
	CallApi("/api/OnClientConnect", json, Result);
}

function OnSMTPData(oClient, oMessage)
{
	var json = '{' +
		SerializeClient(oClient) + ',' +
		SerializeMessage(oMessage) + '}';
	CallApi("/api/OnSMTPData", json, Result);
}

function OnAcceptMessage(oClient, oMessage)
{
	var json = '{' +
		SerializeClient(oClient) + ',' +
		SerializeMessage(oMessage) + '}';
	CallApi("/api/OnAcceptMessage", json, Result);
}

function OnDeliveryStart(oMessage)
{
	var json = '{' + SerializeMessage(oMessage) + '}';
	CallApi("/api/OnDeliveryStart", json, Result);
}

function OnDeliverMessage(oMessage)
{
	var json = '{' + SerializeMessage(oMessage) + '}';
	CallApi("/api/OnDeliverMessage", json, Result);
}

function OnBackupFailed(sReason)
{
	var json = '{ "Reason": "' + Escape(sReason) + '"}';
	CallApi("/api/OnBackupFailed", json);
}

function OnBackupCompleted()
{
	CallApi("/api/OnBackupCompleted", '{}');
}

function OnError(iSeverity, iCode, sSource, sDescription)
{
	var json = '{ "Severity": ' + iSeverity +
		', "Code": ' + iCode +
		', "Source": "' + Escape(sSource) +
		'", "Description": "' + Escape(sDescription) + '" }';
	CallApi("/api/OnError", json);
}

function OnDeliveryFailed(oMessage, sRecipient, sErrorMessage)
{
	var json = '{' + SerializeMessage(oMessage) +
		', "Recipient": "' + Escape(sRecipient) +
		'", "ErrorMessage":' + Escape(sErrorMessage) + '" }';
	CallApi("/api/OnDeliveryFailed", json);
}

function OnExternalAccountDownload(oFetchAccount, oMessage, sRemoteUID)
{
	var json = '{' +
		SerializeFetchAccount(oFetchAccount) + ',' +
		SerializeMessage(oMessage) +
		', "RemoteUID": "' + sRemoteUID + '" }';
	CallApi("/api/OnExternalAccountDownload", json, Result);
}


//var Result = {};

//Result.Value = 0;
//Result.Message = "";

//var oClient = {};
//var oMessage = {};

//oClient.IPAddress = "127.0.0.1";
//oClient.Username = "";
//oClient.HELO ="aaa";
//oClient.Port = 25;

//var Recipient = {};
//Recipient.Address = "recep@rrr.nl";

//oMessage.FromAddress = "a@hwh.nl";
//oMessage.Recipients = new Array( Recipient );


//OnSMTPData(oClient, oMessage);

//var json = '{' +
//		SerializeClient(oClient) + ',' +
//		SerializeMessage(oMessage) + '}';

//WScript.Echo(json);
// var Result = {};
// var response = '{"value":0,"message":"ondelivermessage tester2@djpodium.com"}';
// var i = response.indexOf(":", 1) + 1;
// var j = response.indexOf(",", i);
// var k = response.indexOf(":", j) + 2;
// var l = response.indexOf("}", k);
// if (i > 0 && j > 0)
	 // Result.value = response.substring(i, j);
// if (k > 0 && l > 0)
	// Result.message = response.substring(k, l - 1);

// WScript.Echo("["+Result.value + "][" + Result.message+"]");