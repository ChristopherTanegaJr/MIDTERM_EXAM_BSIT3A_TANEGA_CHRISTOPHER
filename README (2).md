# Bowling Scoring Application - COMPLETE VERSION

## Features
- Strike logic
- Spare logic
- 10th frame bonus rolls
- Perfect game (300 supported)
- SQLite persistence
- Swagger enabled

## Run Backend
cd BowlingApp.API
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run

## Run Frontend
cd BowlingApp.Web
npm install
npm run dev
