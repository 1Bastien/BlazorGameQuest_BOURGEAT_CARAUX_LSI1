# BlazorGameQuest

Bastien BOURGEAT - Ghislain CARAUX - LSI 1 APP 2 Promo 2027

## Prérequis

- Docker & Docker Compose

## Installation et démarrage

```bash
# Lancer avec Docker
docker-compose up --build

# Pour arrêter
docker-compose down
```

Les services seront disponibles sur :

- Frontend : http://localhost:5000
- API Gateway : http://localhost:5001

## Description du projet

Le joueur explore un nombre aléatoire de salles générées automatiquement.
À chaque salle, il peut choisir entre trois actions : **Combattre**, **Fouiller** ou **Fuir**.

- **Combat** : 50% de chance de victoire (gagne des points) ou de défaite (perd des points et de la vie)
- **Fouille** : 33% de trouver un trésor (gagne des points), 33% une potion (gagne de la vie), 33% un piège (perd des points et de la vie)
- **Fuite** : perd des points mais ne subit aucun dégât

Les points et la vie gagnés ou perdus varient aléatoirement dans des plages configurables.
La vie est plafonnée à 200 et la partie se termine si le joueur meurt (vie à 0) ou termine toutes les salles.

L'administrateur peut créer, modifier ou supprimer des modèles de salles avec des descriptions personnalisées,
ainsi que modifier la configuration globale des récompenses et pénalités du jeu.

## Architecture

Le projet est divisé en plusieurs applications :

- **BlazorGame.Client** : Frontend en Blazor WebAssembly
- **BlazorGame.Gateway** : API Gateway qui gère les requêtes
- **AuthenticationServices** : Gestion des utilisateurs et connexion
- **BlazorGame.Core** : Logique du jeu
- **SharedModels** : Modèles partagés entre les services
- **BlazorGame.Tests** : Projet dédié aux tests unitaires et d’intégration de la solution.

Nous avons choisi une architecture microservices car c’est l’une des plus modulaires et scalables.
Pour pouvoir potentiellement accueillir un grand nombre de joueurs, cette architecture permet une montée en charge facile.
Elle nous offrira également une grande flexibilité pour ajouter de nouvelles fonctionnalités au jeu de base.

### Configuration de la Gateway (YARP)

La gateway utilise **YARP (Yet Another Reverse Proxy)** pour router les requêtes vers les différents services.

**Routes configurées :**

- `/api/GameSessions/*` → Core Service
- `/api/GameRewards/*` → Core Service
- `/api/RoomTemplates/*` → Core Service
- `/api/auth/*` → Auth Service
- `/*` (tout le reste) → Client Blazor

## Frontend (BlazorGame.Client)

### Pages principales

- `/` : Accueil
- `/new-adventure` : Démarrer une partie
- `/classement` : Scores des joueurs
- `/history` : Historique des parties
- `/admin-dashboard` : Dashboard admin
- `/account`: Mon compte

Notre frontend est organisé en trois dossiers principaux :

- **Pages/** : Contient les différentes pages de l'application (`/`, `/new-adventure`, `/classement`, `/history`, `/admin-dashboard`)
- **Components/** : Regroupe les éléments réutilisables comme la salle de jeu ou les boutons
- **Layout/** : Définit la structure commune à toutes les pages (header)

## Stratégie de tests

Les tests sont implémentés avec xUnit, Moq et une base de données InMemory. Le projet de tests est organisé en deux catégories :

**Tests Core (BlazorGame.Core)** :

- **GameSessionServiceTests** : création, mise à jour et abandon de sessions, gestion des états de jeu
- **GameActionServiceTests** : traitement des actions (combat, fouille, fuite), calcul des points et de la vie, transitions d'état
- **RoomTemplateServiceTests** : opérations CRUD sur les modèles de salles
- **GameRewardsServiceTests** : récupération et mise à jour de la configuration des récompenses
- **GameSessionsControllerTests** : tests des endpoints de gestion des sessions
- **GameRewardsControllerTests** : tests des endpoints de configuration des récompenses
- **RoomTemplatesControllerTests** : tests des endpoints CRUD des modèles de salles

**Tests Client (BlazorGame.Client)** :

- **GameServiceTests** : tests du service de gestion des parties côté client
- **AdminServiceTests** : tests du service d'administration côté client

La validation des données est assurée par les annotations Data Annotation sur les entités. Un workflow CI/CD GitHub Actions exécute automatiquement les tests à chaque push sur les branches master/main.

### Couverture de code

Pour exécuter les tests avec couverture de code et générer un rapport HTML :

```bash
# Exécuter les tests avec collecte de couverture
dotnet test --collect:"XPlat Code Coverage"

# Générer le rapport HTML (exclut les fichiers UI Blazor)
dotnet reportgenerator -reports:"BlazorGame.Tests/TestResults/*/coverage.cobertura.xml" -targetdir:"BlazorGame.Tests/TestResults/coverage-report" -reporttypes:Html -classfilters:"-BlazorGame.Client.Components.*;-BlazorGame.Client.Pages.*;-BlazorGame.Client.Layout.*"
```

Le rapport sera disponible dans `BlazorGame.Tests/TestResults/coverage-report/index.html`.

Les fichiers UI (composants Blazor, pages et layouts) sont exclus du calcul de couverture car ils nécessitent des tests d'intégration spécifiques avec un navigateur.
