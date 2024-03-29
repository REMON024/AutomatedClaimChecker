#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["AutomatedClaimChecker/AutomatedClaimChecker.csproj", "AutomatedClaimChecker/"]
COPY ["AzureCognitiveService/AzureCognitiveService.csproj", "AzureCognitiveService/"]
RUN dotnet restore "AutomatedClaimChecker/AutomatedClaimChecker.csproj"
COPY . .
WORKDIR "/src/AutomatedClaimChecker"
RUN dotnet build "AutomatedClaimChecker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AutomatedClaimChecker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AutomatedClaimChecker.dll"]