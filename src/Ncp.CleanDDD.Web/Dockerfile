#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "src/Ncp.CleanDDD.Web/Ncp.CleanDDD.Web.csproj"
WORKDIR "/src/src/Ncp.CleanDDD.Web"
RUN dotnet build "Ncp.CleanDDD.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ncp.CleanDDD.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ncp.CleanDDD.Web.dll"]
