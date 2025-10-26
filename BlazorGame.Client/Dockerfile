# Utilisation de l'image .NET 9 avec Node.js
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS dotnet-build

# Installation de Node.js
RUN apt-get update && apt-get install -y curl
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash -
RUN apt-get install -y nodejs

WORKDIR /src

# Copie du code source du client
COPY BlazorGame.Client/ ./BlazorGame.Client/

# Installation des dépendances npm et build du CSS
WORKDIR /src/BlazorGame.Client
RUN npm install
RUN npx tailwindcss -i ./wwwroot/css/app.css -o ./wwwroot/css/app.min.css --minify

# Copier le projet SharedModels
COPY ["SharedModels/SharedModels.csproj", "../SharedModels/"]
COPY ["SharedModels/", "../SharedModels/"]

# Restauration des dépendances
RUN dotnet restore

# Exposition du port
EXPOSE 80

# Configuration de l'environnement
ENV ASPNETCORE_URLS=http://+:80

# Lancement de l'application
CMD ["dotnet", "run", "--no-launch-profile"]