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

Le joueur explore différentes salles, affronte des monstres et accumule des points.
Les salles peuvent être de type « combat » ou « fouille ».
Selon le type de salle, le joueur peut choisir de combattre, de fouiller ou de fuir, ce qui lui fera gagner ou perdre des points et de la vie.
Les règles détaillées sont disponibles sur la page « Nouvelle Aventure ».

L’administrateur peut créer, modifier ou supprimer des salles en définissant une description et des récompenses différentes (nombre de points à gagner ou perdre),
tout en respectant le type de chaque salle.

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

- Tests unitaires pour les différentes méthodes des classes du backend, par exemple le calcul des scores automatiques apres une partie,
  le calcul du score et de la vie dans le jeu apres les decisions dans les salles, le test des chaque méthode CRUD des entités, etc.
  Une base de données InMemory sera également utilisée afin de tester les opérations d’ajout, de modification et de suppression de données.
  Enfin, la validation des données sera faite dans les DTO et entités avec les annotations Data Annotation.
