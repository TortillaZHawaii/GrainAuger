FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Examples/Kubernetes/CloudNative.Workload/CloudNative.Workload.csproj", "Examples/Kubernetes/CloudNative.Workload/"]
RUN dotnet restore "Examples/Kubernetes/CloudNative.Workload/CloudNative.Workload.csproj"
COPY . .
WORKDIR "/src/Examples/Kubernetes/CloudNative.Workload"
RUN dotnet build "CloudNative.Workload.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CloudNative.Workload.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CloudNative.Workload.dll"]
