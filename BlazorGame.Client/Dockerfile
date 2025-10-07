# Utilisation de l'image .NET SDK 9.0
FROM mcr.microsoft.com/dotnet/sdk:9.0

WORKDIR /app

# Copie des fichiers du projet
COPY BlazorGame.Client/*.csproj ./BlazorGame.Client/
COPY SharedModels/*.csproj ./SharedModels/

# Restauration des dépendances
WORKDIR /app/BlazorGame.Client
RUN dotnet restore

# Copie du code source
WORKDIR /app
COPY BlazorGame.Client/ ./BlazorGame.Client/
COPY SharedModels/ ./SharedModels/

# Définition du dossier du projet à exécuter
WORKDIR /app/BlazorGame.Client

# Exposition du port 80
EXPOSE 80

# Lancement de l'application en mode développement
CMD ["dotnet", "run", "--urls", "http://0.0.0.0:80"]