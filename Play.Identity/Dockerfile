FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5211

ENV ASPNETCORE_URLS=http://+:5211

USER root
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
COPY ["src/Play.Identity.Contracts/Play.Identity.Contracts.csproj", "src/Play.Identity.Contracts/"]
COPY ["src/Play.Identity.Service/Play.Identity.Service.csproj", "src/Play.Identity.Service/"]

RUN --mount=type=secret,id=GH_OWNER,dst=/GH_OWNER \
    --mount=type=secret,id=GH_PAT,dst=/GH_PAT \
    dotnet nuget add source \
    --username USERNAME \
    --password `cat /GH_PAT` \
    --store-password-in-clear-text \
    --name github \
    "https://nuget.pkg.github.com/`cat /GH_OWNER`/index.json"

RUN dotnet restore "src/Play.Identity.Service/Play.Identity.Service.csproj"
COPY ./src ./src
WORKDIR "/src/Play.Identity.Service"
RUN dotnet publish "Play.Identity.Service.csproj" -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Play.Identity.Service.dll"]



