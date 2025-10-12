# Utilisation de l'image Node 20
FROM node:20 AS node-build
WORKDIR /app

# Copie des fichiers package.json et package-lock.json
COPY BlazorGame.Client/package*.json ./

# Installation des dépendances
RUN npm install

# Copie du code source
COPY BlazorGame.Client/ .

# Construction du CSS
RUN npx tailwindcss -i ./wwwroot/css/app.css -o ./wwwroot/css/app.min.css --minify

# Utilisation de l'image .NET 9
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS dotnet-build
WORKDIR /src

# Copier les fichiers du build node dans le dossier client
COPY --from=node-build /app ./BlazorGame.Client

# Copier le projet SharedModels
COPY ["SharedModels/SharedModels.csproj", "./SharedModels/"]
COPY ["SharedModels/", "./SharedModels/"]
WORKDIR /src/BlazorGame.Client

# Restauration des dépendances
RUN dotnet restore

# Exposition du port
EXPOSE 5000

# Lancement de l'application en mode développement
CMD ["dotnet", "run", "--urls", "http://0.0.0.0:5000"]