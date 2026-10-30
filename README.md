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

Copy `.env.example` to `.env` and set strong values for `MSSQL_SA_PASSWORD`, `REDIS_PASSWORD`, and `JWT_SIGNING_KEY` (see [Authentication](#authentication)). Then, from the repository root, build and start the backend, SQL Server, and Redis:

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

The API connects to SQL Server (ASP.NET Core Identity via the `Infrastructure` project) but does not yet use Redis; Compose still provides the Redis service and connection string for future wiring. Apply the schema before first use (the Docker SQL Server port is published on localhost):

```powershell
dotnet ef database update --project Infrastructure --startup-project API --connection "Server=localhost,1433;Database=AgoraRivas;User Id=sa;Password=<MSSQL_SA_PASSWORD>;TrustServerCertificate=True"
```

## Authentication

The API supports local email/password accounts and OAuth2 sign-in with Google and GitHub. Clients can authenticate with either an HttpOnly cookie session or a JWT bearer token.

### Configuration

| Setting (env var form) | Purpose |
| --- | --- |
| `Jwt__SigningKey` (`JWT_SIGNING_KEY` in `.env`) | Required HMAC key, at least 32 characters |
| `Jwt__Issuer`, `Jwt__Audience`, `Jwt__ExpiryMinutes` | Token settings (defaults in `appsettings.json`) |
| `Authentication__Google__ClientId` / `ClientSecret` | Enables Google sign-in when both are set |
| `Authentication__GitHub__ClientId` / `ClientSecret` | Enables GitHub sign-in when both are set |
| `Authentication__AllowedReturnOrigins__0` | SPA origin (no trailing slash) allowed as an OAuth return URL; `http://localhost:4003` in Development |

Never commit secrets. For local `dotnet run`, use user secrets, for example `dotnet user-secrets set "Jwt:SigningKey" "<32+ chars>" --project API`; with Docker, set the values in `.env`. A provider with only one of its two values set makes the API fail at startup.

Register the OAuth apps with these callback URLs (replace the host with your API origin):

- Google (Google Cloud Console > OAuth client, Web application): `https://localhost:7041/signin-google`
- GitHub (Settings > Developer settings > OAuth Apps): `https://localhost:7041/signin-github`

### Endpoints

| Endpoint | Description |
| --- | --- |
| `POST /api/auth/register` | Create a local account (`email`, `password`) |
| `POST /api/auth/login` | `mode` is `Cookie` (default, sets the session cookie) or `Jwt` (returns `accessToken`) |
| `POST /api/auth/logout` | Clears the session cookie (JWTs are stateless and expire on their own) |
| `GET /api/auth/me` | Current user; accepts the cookie or `Authorization: Bearer <token>` |
| `GET /api/auth/external/{provider}?returnUrl=&mode=` | Starts Google or GitHub sign-in (`google`, `github`) |

After an external sign-in the API redirects to `returnUrl`: in `Cookie` mode the session cookie is set; in `Jwt` mode the token is returned in the URL fragment (`#access_token=...&token_type=Bearer&expires_at=...`). Failures redirect with `?error=` (`verified_email_required`, `locked_out`, `external_login_failed`). `returnUrl` must be a local path or an allowed origin.

External accounts are linked to an existing user only when the provider reports a verified email. If the existing local account's email was never confirmed, its password is removed on linking so a pre-registered password cannot be used to take over the account. Cookies are `Secure`, `HttpOnly`, and `SameSite=Lax`, and state-changing endpoints accept JSON only, which mitigates CSRF. Login endpoints are rate limited per client IP.

### Tests

```powershell
dotnet test AgoraRivas.slnx
```

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
