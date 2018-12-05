
FROM microsoft/dotnet:2.1-sdk AS build-env
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out

FROM microsoft/dotnet:2.1-runtime 

WORKDIR /app

COPY --from=build-env /app/out ./
ENV VRTU_CONFIG_SAS_URI https://virtualrtu.blob.core.windows.net/config/vconfig.json?st=2018-08-24T16%3A39%3A07Z&se=2020-08-25T16%3A39%3A00Z&sp=r&sv=2018-03-28&sr=b&sig=kc4kPollVYoQ3LFNkPoERTaHsRgzEhnaecKa%2FCUDt2g%3D
ENV RTU_MAP_SAS_URI https://virtualrtu.blob.core.windows.net/map/rtumap.json?st=2018-08-24T16%3A38%3A35Z&se=2020-08-25T16%3A38%3A00Z&sp=r&sv=2018-03-28&sr=b&sig=oZFNuWNRq9124fWindACVhnN9ZNYI7PKI66KVgFuB%2Fc%3D

ENTRYPOINT ["dotnet", "SkunkLab.VirtualRtu.Server.dll"]