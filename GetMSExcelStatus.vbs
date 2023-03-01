Dim excelStatus
excelStatus = 0 

Dim excel
On Error Resume Next
Set excel = GetObject(, "Excel.Application")

If Err.Number = 0 Then 
    excelStatus = 1
    
End If
On Error GoTo 0 

WScript.Quit(excelStatus)