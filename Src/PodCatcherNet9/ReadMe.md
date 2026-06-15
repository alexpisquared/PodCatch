# PodCatcherNet9: direct inplace port from the 4.8
## 2025-01-02
## 2025-01-03  Removing Fody saved tha day: runs, loads data.


## 2025-01-03  Tried this in one copy/paste/run:
dotnet new classlib -n Db.PodcastMgt.EF.Net9
cd .\Db.PodcastMgt.EF.Net9\
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet ef dbcontext scaffold "Server=.\SqlExpress;Database=PodcastMgt;Trusted_Connection=True;TrustServerCertificate=Yes;" Microsoft.EntityFrameworkCore.SqlServer -o Models --force
  ..still not used anywhere yet.

## 2025-01-03  Replaced .Net 4 with .Net 9 in Scheduler:
C:\g\PodCatch\Src\PodCatcher\bin\x64\Release\PodCatcher.exe
C:\g\PodCatch\Src\PodCatcherNet9\bin\x64\Release\net9.0-windows\PodCatcherNet9.exe