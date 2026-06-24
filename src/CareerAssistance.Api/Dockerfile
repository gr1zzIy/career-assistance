FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/CareerAssistance.Api/CareerAssistance.Api.csproj", "src/CareerAssistance.Api/"]
COPY ["src/CareerAssistance.Application/CareerAssistance.Application.csproj", "src/CareerAssistance.Application/"]
COPY ["src/CareerAssistance.Domain/CareerAssistance.Domain.csproj", "src/CareerAssistance.Domain/"]
COPY ["src/CareerAssistance.Infrastructure/CareerAssistance.Infrastructure.csproj", "src/CareerAssistance.Infrastructure/"]
COPY ["src/CareerAssistance.Shared/CareerAssistance.Shared.csproj", "src/CareerAssistance.Shared/"]

RUN dotnet restore "src/CareerAssistance.Api/CareerAssistance.Api.csproj"
COPY . .
WORKDIR "/src/src/CareerAssistance.Api"
RUN dotnet build "./CareerAssistance.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CareerAssistance.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CareerAssistance.Api.dll"]
