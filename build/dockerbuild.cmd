
::msbuild ..\src\\LogModule.Core\LogModule.Core.csproj /t:Clean,Rebuild,restore /p:OutputPath=..\..\build\LogModule.Core\Output /p:Configuration=Release /fl1 /fl2 /fl3 /flp1:logfile=.\BuildOutput\LogModule.Core.log_errors;errorsonly /flp2:logfile=.\BuildOutput\LogModule.Core_warnings.log;warningsonly /flp3:logfile=.\BuildOutput\LogModule.Core.log

::msbuild ..\src\LogModule\LogModule.csproj /t:Clean,Rebuild,restore /p:OutputPath=..\..\build\LogModule\Output /p:Configuration=Release /fl1 /fl2 /fl3 /flp1:logfile=.\BuildOutput\LogModule.log_errors;errorsonly /flp2:logfile=.\BuildOutput\LogModule_warnings.log;warningsonly /flp3:logfile=.\BuildOutput\LogModule.log


::dotnet publish "..\src\LogModule\LogModule.csproj" -c Release -f netcoreapp3.1 -o "LogModule"

dotnet publish "..\src\LogModule.Core\LogModule.Core.csproj" -c Release -o "LogModule.Core"

dotnet publish "..\src\LogModule\LogModule.csproj" -c Release -o "LogModule"

docker rmi skunklab/iotedge-logmodule:v3.1

docker build -t skunklab/iotedge-logmodule:v3.1 ./LogModule

docker push skunklab/iotedge-logmodule:v3.1









