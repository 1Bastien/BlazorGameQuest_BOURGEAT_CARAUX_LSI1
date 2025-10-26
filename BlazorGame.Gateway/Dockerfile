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

# Renommer appsettings.Docker.json en appsettings.json pour l'environnement Docker
WORKDIR /app/BlazorGame.Gateway
RUN if [ -f "appsettings.Docker.json" ]; then \
    mv appsettings.Docker.json appsettings.json; \
    fi

# Exposition du port
WORKDIR /app/BlazorGame.Gateway
EXPOSE 5000

# Configuration de l'environnement Docker
ENV ASPNETCORE_ENVIRONMENT=Docker

# Lancement de l'application
CMD ["dotnet", "run", "--urls", "http://0.0.0.0:5000"]
