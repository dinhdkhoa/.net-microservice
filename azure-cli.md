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
aksname="playeconomyaks"
az aks create -n $aksname -g $appname \
  --node-vm-size Standard_B2s \
  --node-count 2 \
  --attach-acr $acrname \
  --enable-oidc-issuer \
  --enable-workload-identity \
  --generate-ssh-keys
```
Add AKS to kube config:
```bash 
az aks get-credentials --name $aksname --resource-group $appname
```
Create az keyvault:
```bash 
$akzname=akz
 az keyvault create -n $akzname -g $appname
```
Creating the Azure Managed Identity and granting it access to Key Vault secrets
```bash 
#Create the managed identity
az identity create --resource-group $appname --name $namespace

#Get the client ID of the managed identity
IDENTITY_CLIENT_ID=$(az identity show -g $appname -n $namespace --query clientId -o tsv)

#Set Key Vault policy to allow get and list secret permissions
az keyvault set-policy -n $appname --secret-permissions get list --spn $IDENTITY_CLIENT_ID
```
Create the federated identity credential

```bash 
$aks_oidc_issuer=$( az aks show --name $akzname --resource-group $appname --query "oidcIssuerProfile.issuerUrl" -o tsv)

az identity federated-credential create \
  --name $namespace \
  --identity-name $namespace \
  --resource-group $appname \
  --issuer $AKS_OIDC_ISSUER \
  --subject "system:serviceaccount:${namespace}:${namespace}-serviceaccount"

```

