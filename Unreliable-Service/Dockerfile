FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Unreliable-Service/Unreliable-Service.csproj", "Unreliable-Service/"]
RUN dotnet restore "Unreliable-Service/Unreliable-Service.csproj"
COPY . .
WORKDIR "/src/Unreliable-Service"
RUN dotnet build "Unreliable-Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Unreliable-Service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Unreliable-Service.dll"]
