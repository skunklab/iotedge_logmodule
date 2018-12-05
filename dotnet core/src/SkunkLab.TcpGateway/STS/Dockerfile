FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 6095
EXPOSE 44381

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY STS/STS.csproj STS/
RUN dotnet restore STS/STS.csproj
COPY . .
WORKDIR /src/STS
RUN dotnet build STS.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish STS.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "STS.dll"]
