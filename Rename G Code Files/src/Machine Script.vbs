'************************* Machine Script *************************
'PROGRAMMER: PATRICK HURST PATHURST05@GMAIL.COM
'DATE IMPLPEMENTED: 10/30/2024

'******************************************************************
'Writes the current running version and output path to the registry.
'Calls an executable to rename and move output files.

Sub Main()

    Dim objShell
    Dim regPathVersion
    Dim regPathOutput

    regPathVersion = "HKEY_CURRENT_USER\Software\Hurst Software Engineering\G Code Post Processor\RunningVersion"
    regPathOutput = "HKEY_CURRENT_USER\Software\Hurst Software Engineering\G Code Post Processor\OutputPath"


    Set objShell = CreateObject("Wscript.Shell")

    Call objShell.RegWrite(regPathVersion, Left(Right(NCPath,5),4), "REG_SZ")
    Call objShell.RegWrite(regPathOutput, CVOutputPath, "REG_SZ")
    Call objShell.Run(Chr(34) & "C:\Program Files\Hurst Software Engineering\Rename G Code Files\Rename G Code Files.exe")

End Sub
