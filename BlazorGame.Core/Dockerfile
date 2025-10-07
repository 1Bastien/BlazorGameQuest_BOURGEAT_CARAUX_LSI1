# Utilisation de l'image .NET SDK 9.0
FROM mcr.microsoft.com/dotnet/sdk:9.0

WORKDIR /app

# Copie des fichiers du projet
COPY BlazorGame.Core/*.csproj ./BlazorGame.Core/
COPY SharedModels/*.csproj ./SharedModels/

# Restauration des dépendances
WORKDIR /app/BlazorGame.Core
RUN dotnet restore

# Copie du code source
WORKDIR /app
COPY BlazorGame.Core/ ./BlazorGame.Core/
COPY SharedModels/ ./SharedModels/

# Définition du dossier du projet à exécuter
WORKDIR /app/BlazorGame.Core

# Exposition du port 80
EXPOSE 80

# Lancement de l'application en mode développement
CMD ["dotnet", "run", "--urls", "http://0.0.0.0:80"]
