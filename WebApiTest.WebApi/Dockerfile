#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["WebApiTest.WebApi/WebApiTest.WebApi.fsproj", "WebApiTest.WebApi/"]
RUN dotnet restore "WebApiTest.WebApi/WebApiTest.WebApi.fsproj"
COPY . .
WORKDIR "/src/WebApiTest.WebApi"
RUN dotnet build "WebApiTest.WebApi.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebApiTest.WebApi.fsproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApiTest.WebApi.dll"]