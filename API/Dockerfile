ARG DOTNET_IMAGE_TAG=10.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_IMAGE_TAG} AS build
WORKDIR /src

COPY AgoraRivas.slnx ./
COPY API/API.csproj API/
COPY Core/Core.csproj Core/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
RUN dotnet restore AgoraRivas.slnx

COPY .editorconfig ./
COPY Core/ Core/
COPY Infrastructure/ Infrastructure/
COPY API/ API/
RUN dotnet build AgoraRivas.slnx -c Release --no-restore
RUN dotnet publish API/API.csproj -c Release --no-restore -o /app/publish -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_IMAGE_TAG} AS final
WORKDIR /app
COPY --from=build /app/publish ./

EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "API.dll"]
