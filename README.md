# RemedySync

Remedy API Client

## Installation

First, install .Net Framework 2.0 SDK and copy relevant libraries to `C:\libs\`.

Seconds, update or add the following environment variables:

```
LIB="C:\libs\Remedy"
PATH="C:\libs\Remedy"
ARTCPPORT=8000
```

Third, register relevant libraries:

```
C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\regasm.exe "C:\libs\Remedy\BMC.ARSystem.dll" /codebase
C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\regasm.exe "C:\libs\CommandLine\CommandLine.dll" /codebase
```

Finally, add relevant libraries, i.e. BMC.ARSystem, BMC.arnettoc, CommandLine, to Assembly Cache
in .Net Configuration menu.
