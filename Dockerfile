FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY FiapDonateCampaign.API/FiapDonateCampaign.API.csproj FiapDonateCampaign.API/
COPY FiapDonateCampaign.Application/FiapDonateCampaign.Application.csproj FiapDonateCampaign.Application/
COPY FiapDonateCampaign.Domain/FiapDonateCampaign.Domain.csproj FiapDonateCampaign.Domain/
COPY FiapDonateCampaign.Infrastructure/FiapDonateCampaign.Infrastructure.csproj FiapDonateCampaign.Infrastructure/
RUN dotnet restore FiapDonateCampaign.API/FiapDonateCampaign.API.csproj

COPY . .
RUN dotnet publish FiapDonateCampaign.API/FiapDonateCampaign.API.csproj --configuration Release --output /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "FiapDonateCampaign.API.dll"]