#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["RayTracing.Web/RayTracing.Web.csproj", "RayTracing.Web/"]
COPY ["RayTracing.CalculationModel/RayTracing.CalculationModel.csproj", "RayTracing.CalculationModel/"]
RUN dotnet restore "RayTracing.Web/RayTracing.Web.csproj"
COPY . .
WORKDIR "/src/RayTracing.Web"
RUN dotnet build "RayTracing.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RayTracing.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RayTracing.Web.dll"]