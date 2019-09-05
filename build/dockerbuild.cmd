
msbuild ..\src\\LogModule.Core\LogModule.Core.csproj /t:Clean,Rebuild,restore /p:OutputPath=..\..\build\LogModule.Core\Output /p:Configuration=Release /fl1 /fl2 /fl3 /flp1:logfile=.\BuildOutput\LogModule.Core.log_errors;errorsonly /flp2:logfile=.\BuildOutput\LogModule.Core_warnings.log;warningsonly /flp3:logfile=.\BuildOutput\LogModule.Core.log

msbuild ..\src\LogModule\LogModule.csproj /t:Clean,Rebuild,restore /p:OutputPath=..\..\build\LogModule\Output /p:Configuration=Release /fl1 /fl2 /fl3 /flp1:logfile=.\BuildOutput\LogModule.log_errors;errorsonly /flp2:logfile=.\BuildOutput\LogModule_warnings.log;warningsonly /flp3:logfile=.\BuildOutput\LogModule.log


dotnet publish "..\src\LogModule.Core\LogModule.Core.csproj" -c Release -o "..\..\build\LogModule.Core-Out"

dotnet publish "..\src\LogModule\LogModule.csproj" -c Release -o "..\..\build\LogModule-Out"



docker rmi skunklab/iotedge-logmodule
docker rmi iotedge-logmodule


docker build -t iotedge-logmodule ./LogModule-Out

docker tag iotedge-logmodule skunklab/iotedge-logmodule

docker push skunklab/iotedge-logmodule








