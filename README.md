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

Le joueur explore des salles, combat des monstres et accumule des points. Le projet utilise une architecture microservices pour gérer les différentes fonctionnalités.

## Architecture

Le projet est divisé en plusieurs applications :

- **BlazorGame.Client** : Frontend en Blazor WebAssembly
- **BlazorGame.Gateway** : API Gateway qui gère les requêtes
- **AuthenticationServices** : Gestion des utilisateurs et connexion
- **BlazorGame.Core** : Logique du jeu
- **SharedModels** : Modèles partagés entre les services

Nous avons choisi une architecture microservices car c’est l’une des plus modulaires et scalables.
Pour pouvoir potentiellement accueillir un grand nombre de joueurs, cette architecture permet une montée en charge facile.
Elle nous offrira également une grande flexibilité pour ajouter de nouvelles fonctionnalités au jeu de base.

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

Les tests seront implémentés avec xUnit.

- Tests unitaires pour les différentes méthodes des classes du backend, par exemple le calcul des scores. Une base de données InMemory sera également utilisée afin de tester les opérations d’ajout, de modification et de suppression de données.

- Tests d’intégration pour les interactions entre les différents services, comme l’API Gateway, le service d’authentification ou la gestion des exceptions.
