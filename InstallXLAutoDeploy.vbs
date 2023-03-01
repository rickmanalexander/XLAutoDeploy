Dim continueScript 
continueScript = False 

Dim closeExcel
closeExcel = False 

Dim excel
On Error Resume Next
Set excel = GetObject(, "Excel.Application")

If Err.Number <> 0 Then 
    Set excel = CreateObject("Excel.Application")
    excel.Visible = False
    
    closeExcel = True 
    
    continueScript = (Err.Number = 0)
End If
Err.Clear
On Error GoTo 0 

If continueScript Then 
    Dim addInTitle 
    addInTitle = "XLAutoDeploy"

    Dim addInfolderPath 
    addInfolderPath = Replace(excel.UserLibraryPath, "Microsoft\AddIns\", "XLAutoDeploy\")

    Dim fso
    Set fso = CreateObject("Scripting.FileSystemObject")

    Dim addInFilePath 
    If fso.FileExists(addInfolderPath & addInTitle "-AddIn.xll") Then
        addInFilePath = addInfolderPath & addInTitle "-AddIn.xll"
        
    ElseIf fso.FileExists(addInfolderPath & addInTitle "-AddIn64.xll") Then
        addInFilePath = addInfolderPath & addInTitle "-AddIn64.xll"
        
    End If

    If Len(addInFilePath) > 0 Then 
        On Error Resume Next 
        Dim eventsSetting
        eventsSetting = excel.EnableEvents

        Dim alertsSetting
        alertsSetting = excel.DisplayAlerts

        excel.DisplayAlerts = False
        excel.EnableEvents = False

        Dim addIn 
        On Error Resume Next
        Set addIn = excel.AddIns(addinTitle)
        On Error GoTo 0 
        
        If Err.Number <> 0 Then 
            Err.Clear
            On Error Resume Next
            excel.AddIns.Add addInFilePath, False
            
            continueScript = (Err.Number = 0)
            On Error GoTo 0 
            
        End If

        If continueScript Then 
            On Error Resume Next
            excel.AddIns(addinTitle).Installed = False
            excel.AddIns(addinTitle).Installed = True
            
            excel.EnableEvents = eventsSetting
            excel.DisplayAlerts = alertsSetting
            On Error GoTo 0 
            
            continueScript = (Err.Number = 0)
            
        End If 
        
    End If 

End If 

On Error Resume Next
If closeExcel Then 
    excel.Quit
    Set excel = Nothing
    
End If 
On Error GoTo 0 

If continueScript Then 
    MsgBox "Install Complete!!!", vbInformation, vbNullString
    
End If

WScript.Quit(1)








