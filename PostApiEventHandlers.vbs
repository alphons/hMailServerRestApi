Option Explicit

' (C) Alphons van der Heijden
' 2022-03-24 Version 1.0b

Function CallApi(Url, oClient, oMessage, sRecipient, sErrorMessage, Result)
  Dim http, j, status, response
  Dim ii,jj,kk,ll
  Result.Value = 0
  Result.Message = ""
  j = "{"
  If Not(oClient is Nothing) Then
    j = j & """Client"" : {"
    j = j & """IPAddress"": """ & oClient.IPAddress & ""","
    j = j & """Username"": """ & oClient.Username & ""","
    j = j & """HELO"": """ & oClient.HELO & ""","
    j = j & """Port"": """ & oClient.Port & """"
	j = j & "}"
  End If
  If Not(oClient is Nothing) AND Not(oMessage is Nothing) Then
    j = j & ","
  End If
  If Not(oMessage is Nothing) Then
    j = j & """Message"" : {"
    j = j & """FromAddress"": """ & oMessage.FromAddress & """," &_
      """Recipients"":["
    For ii = 0 to oMessage.Recipients.Count - 1
      If ii>0 Then
        j = j & ","
      End If
      j = j & " { ""Address"" : """ & oMessage.Recipients(ii).Address & """ }"
    Next
    j = j & "] }"
  End If
  If Len(sRecipient & "") > 0 Then
    j = j & ", ""Recipient"":""" & sRecipient & """"
  End If
  If Len(sErrorMessage & "") > 0 Then
    j = j & ", ""ErrorMessage"":""" & sErrorMessage & """"
  End If
  j = j & "}"
  ' WScript.Echo j
  Set http = CreateObject("Msxml2.ServerXMLHTTP") 
  http.open "POST",Url, False 
  http.setRequestHeader "Content-Type", "application/json"
  On Error Resume Next
  http.send j
  If NOT(Err) Then
    status = http.status
    response = http.responseText
    ' WScript.Echo status & " " & response
    If status = 200 Then
      ii = InStr(1, response, ":") + 1
      jj = InStr(ii, response, ",")
      kk = InStr(jj, response, ":") + 2
      ll = InStr(kk, response , "}")
      Result.Value = Mid(response,ii,jj-ii)
      Result.Message = Mid(response,kk,ll-kk-1)
    End If
  End If
  On Error Goto 0
  Set http = Nothing
End Function



Sub OnClientConnect(oClient)
  Call CallApi("http://127.0.0.1:5022/api/OnClientConnect", oClient, Nothing, Null, Null, Result)
End Sub

Sub OnSMTPData(oClient, oMessage)
  Call CallApi("http://127.0.0.1:5022/api/OnSMTPData", oClient, oMessage, Null, Null, Result)
End Sub

Sub OnAcceptMessage(oClient, oMessage)
  Call CallApi("http://127.0.0.1:5022/api/OnAcceptMessage", oClient, oMessage, Null, Null,  Result)
End Sub

Sub OnDeliveryStart(oMessage)
  Call CallApi("http://127.0.0.1:5022/api/OnDeliveryStart", Nothing, oMessage, Null, Null,  Result)
End Sub

Sub OnDeliverMessage(oMessage)
  Call CallApi("http://127.0.0.1:5022/api/OnDeliverMessage", Nothing, oMessage, Null, Null,  Result)
End Sub

'   Sub OnBackupFailed(sReason)
'   End Sub

'   Sub OnBackupCompleted()
'   End Sub

'   Sub OnError(iSeverity, iCode, sSource, sDescription)
'   End Sub

Sub OnDeliveryFailed(oMessage, sRecipient, sErrorMessage)
  Call CallApi("http://127.0.0.1:5022/api/OnDeliveryFailed", Nothing, oMessage, sRecipient, sErrorMessage,  Result)
End Sub

'   Sub OnExternalAccountDownload(oFetchAccount, oMessage, sRemoteUID)
'   End Sub