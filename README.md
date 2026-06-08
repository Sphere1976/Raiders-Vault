# Raiders Vault

## Student Information

**Course:** D424 – Software Engineering Capstone  
**Project:** Raiders Vault – ARC Raiders Companion Planner  
**Platform:** ASP.NET Core MVC (.NET 8) with SQLite  
**IDE Used:** Microsoft Visual Studio  
**Database:** SQLite

---

# Project Overview

Raiders Vault is a companion planning application created for ARC Raiders players.  
The application helps users organize gameplay preparation and progression through:

- Loadout tracking
- Quest management
- Blueprint collection tracking
- Farming route planning
- Map condition planning
- Skill recommendation support
- Favorites management
- Player profile tracking
- Summary reporting

The application uses a local SQLite database and follows a simple ASP.NET Core MVC structure designed for easy local execution and evaluation.

---

# Technologies Used

- ASP.NET Core MVC (.NET 8)
- Entity Framework Core
- SQLite
- Razor Views
- CSS
- Session-based authentication

NuGet packages used:

```
Microsoft.EntityFrameworkCore.Sqlite
Microsoft.EntityFrameworkCore.Tools
```

---

# Default Login

```
Username: admin
Password: password
```

---

# Features Included

## Authentication

- Session-based login system
- Basic password hashing
- Input sanitization
- Protected application pages

---

## Dashboard

The dashboard displays:

- Total quests
- Completed quests
- Blueprint collection progress
- Favorite tracked items
- Player profile information

---

## Loadout Management

Users can:

- Create loadouts
- Edit loadouts
- Delete loadouts
- Assign playstyles
- Store equipment notes

---

## Quest Tracking

Users can:

- Add quests
- Track completion status
- Set priorities
- Store completion notes
- Search quests

---

## Blueprint Tracking

Users can:

- Track blueprint collection progress
- Store farming notes
- Track materials
- View recommended farming routes
- View condition-based recommendations

The project includes updated blueprint support aligned with the current ARC Raiders blueprint list.

---

## Farming Planner

The farming planner provides:

- Map recommendations
- Recommended conditions
- Farming weight indicators
- Suggested routes
- Suggested loadouts

---

## Skill Recommendations

The skill system includes:

- PvE recommendations
- PvP recommendations
- Balanced recommendations
- Skill point tracking
- Skill category organization

---

## Favorites System

Users can:

- Save favorite items
- Track commonly used planning content
- Quickly revisit important records

---

## Reporting

The application includes summary report generation for:

- Quest progress
- Blueprint progress
- Profile information
- Planning summaries

---

# Database Information

Database file:

```
raidersvault.db
```

The database is automatically created and seeded on first launch.

Seed data includes:

- Demo user account
- Sample quests
- Sample blueprints
- Sample loadouts
- Sample player profile
- Sample skills

---

# Running the Application

## Requirements

- Windows operating system
- .NET 8 SDK
- Visual Studio with ASP.NET workload installed

---

## Steps

1. Extract the project zip
2. Open the solution file in Visual Studio
3. Restore NuGet packages if prompted
4. Build the solution
5. Run the application

The SQLite database will generate automatically on first launch.

---

# Project Structure

## Main Folders

```
Controllers/
Models/
Views/
ViewModels/
Services/
Data/
wwwroot/
```

---

# Security Features

The application includes:

- Session validation
- Input sanitization
- Anti-forgery validation
- Password hashing
- Protected routes

---

# Notes for Evaluator

- The application is intended for local execution.
- SQLite is embedded and does not require separate database installation.
- Seed data is automatically generated.
- The application uses a simplified UI focused on functionality and usability.
- Internet access is not required after package extraction and NuGet restore.

---

# Files Included in This Submission

## Core Project Files

```
RaidersVault.sln
RaidersVault.csproj
Program.cs
```

---

## Core Folders

```
Controllers/
Data/
Models/
Services/
ViewModels/
Views/
wwwroot/
```

---

## Documentation Files

```
README.md
```

---

# Final Notes

This submission represents the completed final version of the Raiders Vault capstone project.  
The project was developed incrementally using ASP.NET Core MVC and SQLite while following the requirements of D424 Software Engineering Capstone.

The application focuses on practical planning tools and progression tracking functionality for ARC Raiders players while maintaining a simple local deployment structure suitable for local execution and testing.