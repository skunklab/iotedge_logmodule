
#FROM microsoft/dotnet:sdk AS build-env
FROM microsoft/dotnet:2.1-sdk AS build-env
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out

#FROM microsoft/dotnet:2.1-runtime
#FROM microsoft/dotnet:aspnetcore-runtime
FROM microsoft/dotnet:2.1.1-aspnetcore-runtime
WORKDIR /app
COPY --from=build-env /app/out ./

ARG UID=1000
RUN useradd -u $UID -ms /bin/bash moduleuser
USER moduleuser


# Expose ports
EXPOSE 8889/tcp

#VOLUME /app/config

ENTRYPOINT ["dotnet", "EchoMicroservice.dll"]