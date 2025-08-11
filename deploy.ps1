param(
  [string]$resourceGroup = "WellbeingHubRG",
  [string]$appServicePlan = "WellbeingHubPlan",
  [string]$webAppName = "wellbeinghub-api-uk",
  [string]$location = "westeurope"
)

az login
az group create --name $resourceGroup --location $location
az appservice plan create --name $appServicePlan --resource-group $resourceGroup --sku B1 --is-linux
az webapp create --resource-group $resourceGroup --plan $appServicePlan --name $webAppName --runtime "DOTNETCORE|8.0"

dotnet publish -c Release -o publish
cd publish
Compress-Archive * ../app.zip -Force
cd ..
az webapp deployment source config-zip --resource-group $resourceGroup --name $webAppName --src app.zip

az webapp identity assign --name $webAppName --resource-group $resourceGroup
az webapp config appsettings set --resource-group $resourceGroup --name $webAppName --settings CosmosDb__Account="https://<your-cosmos-account>.documents.azure.com:443/" Jwt__Key="<create-a-strong-key>"