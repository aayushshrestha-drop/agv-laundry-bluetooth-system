#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
#COPY ["nuget.config", "."]
COPY ["src/AGV.Laundry.HttpApi.Host/AGV.Laundry.HttpApi.Host.csproj", "src/AGV.Laundry.HttpApi.Host/"]
COPY ["src/AGV.Laundry.Application/AGV.Laundry.Application.csproj", "src/AGV.Laundry.Application/"]
COPY ["src/AGV.Laundry.Application.Contracts/AGV.Laundry.Application.Contracts.csproj", "src/AGV.Laundry.Application.Contracts/"]
COPY ["src/AGV.Laundry.Domain.Shared/AGV.Laundry.Domain.Shared.csproj", "src/AGV.Laundry.Domain.Shared/"]
COPY ["src/AGV.Laundry.Domain/AGV.Laundry.Domain.csproj", "src/AGV.Laundry.Domain/"]
COPY ["src/AGV.Laundry.MongoDB/AGV.Laundry.MongoDB.csproj", "src/AGV.Laundry.MongoDB/"]
COPY ["src/AGV.Laundry.HttpApi/AGV.Laundry.HttpApi.csproj", "src/AGV.Laundry.HttpApi/"]
RUN dotnet restore "src/AGV.Laundry.HttpApi.Host/AGV.Laundry.HttpApi.Host.csproj"
COPY . .
WORKDIR "/src/src/AGV.Laundry.HttpApi.Host"
RUN mv appsettings.Production.json appsettings.json
RUN dotnet build "AGV.Laundry.HttpApi.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AGV.Laundry.HttpApi.Host.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AGV.Laundry.HttpApi.Host.dll"]
