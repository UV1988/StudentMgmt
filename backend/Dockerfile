FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/StudentManager.Api/StudentManager.Api.csproj src/StudentManager.Api/
RUN dotnet restore src/StudentManager.Api/StudentManager.Api.csproj
COPY src/ src/
RUN dotnet publish src/StudentManager.Api/StudentManager.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENV ConnectionStrings__Default="Data Source=/data/students.db"
RUN mkdir /data && chown $APP_UID /data
USER $APP_UID
VOLUME /data
EXPOSE 8080
ENTRYPOINT ["dotnet", "StudentManager.Api.dll"]
