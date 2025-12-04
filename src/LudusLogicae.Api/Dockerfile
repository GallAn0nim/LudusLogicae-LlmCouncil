FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY LudusLogicae.sln ./
COPY src/LudusLogicae.Api/LudusLogicae.Api.csproj src/LudusLogicae.Api/
RUN dotnet restore src/LudusLogicae.Api/LudusLogicae.Api.csproj

COPY . .
RUN dotnet publish src/LudusLogicae.Api/LudusLogicae.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

RUN mkdir -p /https \
    && dotnet dev-certs https -ep /https/aspnetapp.pfx -p devcertpassword -q

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

EXPOSE 8080
EXPOSE 8443

ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=devcertpassword

ENV ASPNETCORE_URLS=http://+:8080;https://+:8443

ENV ASPNETCORE_ENVIRONMENT=Docker

COPY --from=build /app/publish .
COPY --from=build /https /https

ENTRYPOINT ["dotnet", "LudusLogicae.Api.dll"]
