az group delete -g HUB -y --verbose \
az group create -g HUB --location 'UK West' \
az deployment group validate -g HUB -f .\IdentityStorage.json --verbose \
az deployment group create -g HUB -f .\IdentityStorage.json --verbose \