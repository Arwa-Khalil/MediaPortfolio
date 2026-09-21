FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src
COPY ["src/MediaPortfolio.API/MediaPortfolio.API.csproj", "src/MediaPortfolio.API/"]
COPY ["src/MediaPortfolio.Application/MediaPortfolio.Application.csproj", "src/MediaPortfolio.Application/"]
COPY ["src/MediaPortfolio.Domain/MediaPortfolio.Domain.csproj", "src/MediaPortfolio.Domain/"]
COPY ["src/MediaPortfolio.Infrastructure/MediaPortfolio.Infrastructure.csproj", "src/MediaPortfolio.Infrastructure/"]
COPY ["Directory.Packages.props", "."]
RUN dotnet restore "src/MediaPortfolio.API/MediaPortfolio.API.csproj"
COPY . .
WORKDIR "/src/src/MediaPortfolio.API"
RUN dotnet build "MediaPortfolio.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MediaPortfolio.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app
EXPOSE 5079
ENV ASPNETCORE_URLS=http://+:5079
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MediaPortfolio.API.dll"]
