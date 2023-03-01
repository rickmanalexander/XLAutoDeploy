Dim closeExcel
closeExcel = False

Dim excel
On Error Resume Next
Set excel = GetObject(, "Excel.Application")

If Err.Number <> 0 Then 
    Set excel = CreateObject("Excel.Application")
    excel.Visible = False
    
    closeExcel = True 
    
End If
Err.Clear
On Error GoTo 0 

Dim officeBitness
On Error Resume Next
Dim handle
handle = excel.Hinstance
If Err.Number = 0 Then 
    officeBitness = 32

Else 
    officeBitness = 64
    
End If 
On Error GoTo 0
    
If closeExcel Then 
    excel.Quit
    Set excel = Nothing
    
End If 

WScript.Quit(officeBitness)
