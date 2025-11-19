'************************* Machine Script *************************
'PROGRAMMER: PATRICK HURST PATHURST05@GMAIL.COM
'DATE IMPLPEMENTED: 10/30/2024

'******************************************************************
'Calls an executable to rename and move output files.

Sub Main()

    Dim objShell : Set objShell = CreateObject("Wscript.Shell")
    Dim exePath : exePath = "C:\Program Files\Hurst Software\Rename G Code Files\Rename G Code Files.exe"

    'Call the rename utility with args
    '   arg 0 - running version
    '   arg 1 - output path
    Call objShell.Run(Chr(34) & exePath & Chr(34) & " " & Left(Right(NCPath,5),4) & " " & Chr(34) & CVOutputPath & Chr(34))

End Sub
