<h1 align="center">Hi 👋, I'm Leo</h1>

<p align="center">
  <strong>A software engineer who finds cooperation as a path to connect with other beings.</strong><br/>
  Leo enjoys playing the harmonica, reading, and whatever challenge brings a reward to his brain.
</p>

---

## 🎯 About AgoraRivas

**AgoraRivas** is a web application designed to organize and manage all your documents in one centralized place. Built with clean architecture principles, this platform provides a seamless experience for document organization, storage, and retrieval.

### Key Features
- 📁 **Document Organization** - Organize documents in a structured, intuitive interface
- 🔍 **Easy Retrieval** - Quickly find and access your documents
- 🏗️ **Clean Architecture** - Built with scalable, maintainable code following SOLID principles
- 🚀 **.NET 10.0** - Leveraging the latest .NET Framework capabilities

---

## 🛠️ Tech Stack

### Backend
- **.NET 10.0** - Modern ASP.NET Core framework
- **Clean Architecture** - Organized into Core, Infrastructure, and API layers
- **RESTful API** - OpenAPI/Swagger support

### Frontend
- **Angular** - Dynamic, responsive user interface

---

## 📁 Project Structure

```
AgoraRivas/
├── Core/                 # Business logic and entities
├── Infrastructure/       # Data access and external services
└── API/                  # REST API controllers and configuration
```

---

## 🚀 Getting Started

### Prerequisites
- .NET 10.0 SDK
- Node.js (for Angular frontend)

### Build & Run
```bash
# Build the solution
dotnet build

# Run the API
dotnet run --project API
```

### Run with Docker

Prerequisites:

- Docker Desktop running Linux containers
- At least 2 GB of available memory for SQL Server

Copy `.env.example` to `.env` and set strong values for `MSSQL_SA_PASSWORD` and `REDIS_PASSWORD`. Then, from the repository root, build and start the backend, SQL Server, and Redis:

```powershell
Copy-Item .env.example .env
docker compose up --build
```

The API is available at <http://localhost:5202/weatherforecast>. The default host ports are 5202 (API), 1433 (SQL Server), and 6379 (Redis); all are bound to localhost. Override them in `.env` if a port is already in use.

The backend image is named `agora-rivas-backend`. The Dockerfile builds with the .NET 10 SDK from Microsoft Container Registry (MCR) and runs on the ASP.NET 10 runtime image. SQL Server uses the `2025-latest` image from MCR, and Redis uses `8-alpine` from Docker Hub. SQL Server and Redis persist their data in named Docker volumes.

Stop the services while keeping their data:

```powershell
docker compose down
```

To also remove the SQL Server and Redis data volumes:

```powershell
docker compose down --volumes
```

The current API does not yet connect to SQL Server or Redis: it does not reference the `Infrastructure` project or register a database context or Redis cache. Compose provides the services and connection-string environment variables for future application wiring; it does not add those integrations.

---

<h3 align="left">Connect with me:</h3>
<p align="left">
  <a href="https://www.linkedin.com/in/leo-rivas-5233b53a8/" target="_blank">
    <img align="center" src="https://raw.githubusercontent.com/rahuldkjain/github-profile-readme-generator/master/src/images/icons/Social/linked-in-alt.svg" alt="LinkedIn" width="40" height="40"/>
  </a>
</p>

---

<p align="center">
  <em>Building solutions, one document at a time. 📄✨</em>
</p>
