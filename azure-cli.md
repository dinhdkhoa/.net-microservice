Create an Azure resource group 
```bash 
appname = "playeconomy"
az group create --name $appname --location eastus

```
Create Cosmos DB Account
```bash 
az cosmosdb create --name $appname --resource-group $appname --kind MongoDB --enable-free-tier
```
Create Service bus
```bash 
az servicebus namespace create --name $appname --resource-group $appname --sku Standard
```
Register Container Registry
```bash 
az provider register --namespace Microsoft.ContainerRegistry
```
Create Container Registry
```bash 
az acr create --name $appname --resource-group $appname --sku Basic
```
Creates an Azure Kubernetes Service (AKS) cluster with:

- 2 nodes using Standard_B2s VM size
- Workload identity and OIDC issuer enabled
- ACR (Azure Container Registry) attached
- SSH keys generated for node access

```bash 
az aks create -n $appname -g $appname \
  --node-vm-size Standard_B2s \
  --node-count 2 \
  --attach-acr $appname \
  --enable-oidc-issuer \
  --enable-workload-identity \
  --generate-ssh-keys

```
