FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["Docker.Web.Server/Docker.Web.Server.csproj", "Docker.Web.Server/"]
RUN dotnet restore "Docker.Web.Server/Docker.Web.Server.csproj"
COPY . .
WORKDIR "/src/Docker.Web.Server"
RUN dotnet build "Docker.Web.Server.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Docker.Web.Server.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Docker.Web.Server.dll"]