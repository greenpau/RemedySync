DEL C:\Users\Administrator\Tools\RemedySync\Build\remedysync.exe
REM "C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\csc.exe" /lib:C:\ARAPI.NET /reference:BMC.ARSystem.dll, /define:DEBUG /optimize /out:Build/rmdsync.exe rmdsync.cs
REM "C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\csc.exe" /reference:"C:\ARAPI.NET\BMC.ARSystem.dll","CommandLine.dll" /define:DEBUG /optimize /out:Build/rmdsync.exe rmdsync.cs
REM "C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\csc.exe" /reference:"C:\libs\Remedy\BMC.ARSystem.dll"; /optimize /out:mdsync.exe rmdsync.cs
"C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\csc.exe" /reference:BMC.ARSystem.dll;BMC.arnettoc.dll;NDesk.Options.dll /optimize /out:remedysync.exe remedysync.cs
