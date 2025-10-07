# Utilisation de l'image .NET SDK 9.0
FROM mcr.microsoft.com/dotnet/sdk:9.0

WORKDIR /app

# Copie des fichiers du projet
COPY AuthenticationServices/*.csproj ./AuthenticationServices/
COPY SharedModels/*.csproj ./SharedModels/

# Restauration des dépendances
WORKDIR /app/AuthenticationServices
RUN dotnet restore

# Copie du code source
WORKDIR /app
COPY AuthenticationServices/ ./AuthenticationServices/
COPY SharedModels/ ./SharedModels/

# Exposition du port
WORKDIR /app/AuthenticationServices
EXPOSE 80

# Lancement de l'application en mode développement
CMD ["dotnet", "run", "--urls", "http://0.0.0.0:80"]

