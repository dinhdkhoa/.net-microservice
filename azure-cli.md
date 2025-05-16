Create an Azure resource group 
```bash 
appname = "playeconomy"
az group create --name $appname --location eastus

```
Create Cosmos DB Account
```bash 
az cosmosdb create --name $appname --resource-group $appname --kind MongoDB --enable-free-tier
```