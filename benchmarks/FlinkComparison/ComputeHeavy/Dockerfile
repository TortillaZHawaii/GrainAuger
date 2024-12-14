FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["benchmarks/FlinkComparison/ComputeHeavy/ComputeHeavy.csproj", "benchmarks/FlinkComparison/ComputeHeavy/"]
RUN dotnet restore "benchmarks/FlinkComparison/ComputeHeavy/ComputeHeavy.csproj"
COPY . .
WORKDIR "/src/benchmarks/FlinkComparison/ComputeHeavy"
RUN dotnet build "ComputeHeavy.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ComputeHeavy.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ComputeHeavy.dll"]
