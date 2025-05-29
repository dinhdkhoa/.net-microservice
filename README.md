
Add Local Nuget Source: 
```bash
dotnet nuget add source "D:\*\.net-microservice\packages" --name Play.Economy
```

Check Nuget Source: 
```bash
dotnet nuget list source 
```

Remove Nuget Source: 
```bash
dotnet nuget remove source Play.Economy
```

Clear nuget cache: 
```bash
dotnet nuget locals all --list
dotnet nuget locals global-packages --clear
```

Create Common Package: 
```bash
dotnet pack -o ../../../packages -p:PackageVersion=1.0.x
```

Pack And Push To GitHub Registry: 
```bash
version="1.0.6"
owner="net-microservices"
gh_pat="[YOUR_GITHUB_PAT]"

dotnet pack src/Play.Common \
  --configuration Release \
  -p:PackageVersion=$version \
  -p:RepositoryUrl=https://github.com/$owner/Play.Common \
  -o ../packages

dotnet nuget push ../packages/Play.Common.$version.nupkg \
  --api-key "$gh_pat" \
  --source "github"
```
Add Source For Shared Nuget Packages
```bash

owner="net-microservices"
gh_pat="[YOUR_GITHUB_PAT]"

dotnet nuget add source \
  --username USERNAME \
  --password "$gh_pat" \
  --store-password-in-clear-text \
  --name github \
  "https://nuget.pkg.github.com/$owner/index.json"
```
Build docker image with secrets
```bash

version="1.0.0"
export GH_OWNER="net-microservices" # must correspond to the ids used in the Dockerfile
export GH_PAT="[PAT HERE]"

docker build --secret id=GH_OWNER --secret id=GH_PAT -t play.identity:$version .
#build with acr tag
acrname="playeconomycrname"
docker build --secret id=GH_OWNER --secret id=GH_PAT -t "$acrname.azurecr.io/play.identity:$version" .

```

Run Play.Identity docker image with infra network
```bash
adminpass="PASS"

docker run -it --rm -p 5211:5211 --name Play.Identity \ 
-e MongoDbSettings__Host=mongo \
-e RabbitMQSettings__Host=rabbitmq \
-e IdentitySettings__AdminUserPassword=$adminpass \
--network playinfra_default play.identity:1.0.x
```

Run Play.Identity docker image with Azure Service Bus - Play.Common 1.0.8
```bash
adminpass="PASS"
serviceBusConnectionString="connect"

docker run -it --rm -p 5211:5211 --name Play.Identity \ 
-e MongoDbSettings__ConnectionString=mongo \
-e ServiceBusSettings__ConnectionString=$serviceBusConnectionString \
-e ServiceSettings__MessageBroker="SERVICE_BUS" \ 
-e IdentitySettings__AdminUserPassword=$adminpass \
play.identity:1.0.x
```

Push Docker Image To ACR
```bash
appname="play.economy"
acrname="playeconomycrname"

az acr login --name $acrname

# tag if needed
docker tag play.identity:$version "$acrname.azurecr.io/play.identity:$version"

docker push "$acrname.azurecr.io/play.identity:$version"

```
Create the Kubernetes namespace
```bash
namespace="identity"
kubectl create namespace $namespace
```
Create the Kubernetes secrets
```bash
kubectl create secret generic identity-secrets \
  --from-literal="mongo-connectionstring=$mongoDbConnectionString" \
  --from-literal="servicebus-connectionstring=$serviceBusConnectionString" \
  --from-literal="admin-password=$adminpass" -n $namespace
```
Check Kubernetes secrets
```bash
kubectl get secrets -n $namespace
```
Delete Kubernetes secrets
```bash
kubectl delete secrets <secret's-name> -n $namespace
```
Apply the Kubernetes deployment manifest to create resources in the given namespace
```bash
kubectl apply -f ./kubernetes/identity.yml -n $namespace
```
List all pods in the namespace to verify deployment
```bash
kubectl get pods -n $namespace
```
List all services  in the namespace
```bash
kubectl get services -n $namespace
```
View logs of a specific pod (replace <pod-name> with the actual pod name)
```bash
kubectl logs <pod-name> -n $namespace
```
Describe pod details: status, events, IP, container conditions, etc.
```bash
kubectl describe pod <pod-name> -n $namespace
```

Thêm Helm repo
```bash
helm repo add datawire https://app.getambassador.io
helm repo update

#Cài CRDs cho Emissary
kubectl apply -f https://app.getambassador.io/yaml/emissary/3.3.0/emissary-crds.yaml

#Đợi CRDs sẵn sàng
kubectl wait --timeout=90s --for=condition=available deployment emissary-apiext -n emissary-system

#Khai báo biến
namespace="emissary"
appname="your-dns-label" # <-- đổi tên phù hợp với Azure DNS label của bạn

#Cài emissary-ingress với DNS annotation
helm install emissary-ingress datawire/emissary-ingress \
  --set service.annotations."service\.beta\.kubernetes\.io/azure-dns-label-name"=$appname \
  -n $namespace --create-namespace

#Đợi ingress triển khai xong
kubectl rollout status deployment/emissary-ingress -n $namespace -w

#List helm releases
helm list -n emissary
```

Configuring Emissary-ingress routing
```bash
kubectl apply -f .\emissary-ingress\listener.yaml -n $namespace
kubectl apply -f .\emissary-ingress\mappings.yaml -n $namespace
```
Installing cert-manager
```bash
namespace="cert-manager"

helm repo add jetstack https://charts.jetstack.io
helm repo update

helm install cert-manager jetstack/cert-manager --namespace emissary --version v1.17.2 --set crds.enabled=true 
```
Creating the cluster issuer
```bash
namespace="emissary" 

kubectl apply -f ./cert-manager/cluster-issuer.yaml -n "$namespace"
kubectl apply -f ./cert-manager/acme-challenge.yaml -n "$namespace"

```
Creating the tls certificate
```bash
namespace="emissary" 

kubectl apply -f ./emissary-ingress/tls-certificate.yaml -n $namespace

kubectl get certificate -n $namespace
```