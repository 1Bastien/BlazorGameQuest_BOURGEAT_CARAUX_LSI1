# Utilisation de l'image .NET SDK 9.0
FROM mcr.microsoft.com/dotnet/sdk:9.0

WORKDIR /app

# Copie des fichiers du projet
COPY BlazorGame.Gateway/*.csproj ./BlazorGame.Gateway/
COPY SharedModels/*.csproj ./SharedModels/

# Restauration des dépendances
WORKDIR /app/BlazorGame.Gateway
RUN dotnet restore

# Copie du code source
WORKDIR /app
COPY BlazorGame.Gateway/ ./BlazorGame.Gateway/
COPY SharedModels/ ./SharedModels/

# Exposition du port
WORKDIR /app/BlazorGame.Gateway
EXPOSE 5000

# Configuration de l'environnement Docker
ENV ASPNETCORE_ENVIRONMENT=Docker

# Supprimer les launchSettings.json qui écrasent les variables d'environnement
RUN rm -f Properties/launchSettings.json

# Lancement de l'application
CMD ["dotnet", "run", "--urls", "http://0.0.0.0:5000"]
