

# Backend Setup and Configuration

## Index
- [Environment Variables](#environment-variables)
- [SQL Server in Docker for development](#sql-server-in-docker-for-development)

## Environment Variables
The backend requires the following environment variables to be set in the `.env` file:

- `MSSQL_SA_PASSWORD`: Password for the SQL Server `sa` user.
- `REDIS_PASSWORD`: Password for the Redis instance.
- `JWT_SIGNING_KEY`: Signing key for JWT authentication.
- `GOOGLE_CLIENT_ID` and `GOOGLE_CLIENT_SECRET`: Optional Google OAuth2 credentials.
- `GITHUB_CLIENT_ID` and `GITHUB_CLIENT_SECRET`: Optional GitHub OAuth2 credentials.
- `AUTH_ALLOWED_RETURN_ORIGIN`: Optional SPA origin allowed as an OAuth return URL.
- `ASPNETCORE_ENVIRONMENT`: ASP.NET Core environment (default is `Development`).
- `BACKEND_PORT`: Port for the backend service (default is `5202`).
- `SQL_PORT`: Port for the SQL Server service (default is `1433`).
- `REDIS_PORT`: Port for the Redis service (default is `6379`).

## SQL Server in Docker for development

Run SQL Server as a container, persist its data in a volume, and point your .NET app's connection string at it.

## 1. Quick start with `docker run`

```bash
docker run -d --name sqlserver \
  -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=Your_Strong#Pass123" \
  -e "MSSQL_PID=Developer" \
  -p 1433:1433 \
  -v sqlserver_data:/var/opt/mssql \
  mcr.microsoft.com/mssql/server:2022-latest
```

- `ACCEPT_EULA=Y` is required.
- The `SA` password must be **strong** (8+ chars, with upper, lower, digit, and symbol), or the container starts and then exits.
- `MSSQL_PID=Developer` uses the free Developer edition, which has full features for non-production use.
- `-v sqlserver_data:/var/opt/mssql` keeps your databases when the container is removed or recreated.
- `-p 1433:1433` exposes it to your machine.

Check it's running:

```bash
docker ps
docker logs sqlserver
```

Look for "SQL Server is now ready for client connections."

## 2. Better: Docker Compose

```yaml
# docker-compose.yml
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: sqlserver
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: ${SA_PASSWORD}
      MSSQL_PID: Developer
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
    healthcheck:
      test: /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$$MSSQL_SA_PASSWORD" -Q "SELECT 1" || exit 1
      interval: 10s
      timeout: 5s
      retries: 10
      start_period: 20s

volumes:
  sqlserver_data:
```

Put the password in a `.env` file next to it (Compose reads it automatically, as discussed in the `.env` answer):

```env
SA_PASSWORD=Your_Strong#Pass123
```

Add `.env` to `.gitignore`, then:

```bash
docker compose up -d
docker compose down        # stop, keep data
docker compose down -v     # stop and DELETE data
```

## 3. Connection string

From your app running on your machine:

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost,1433;Database=MyAppDb;User Id=sa;Password=Your_Strong#Pass123;TrustServerCertificate=True"
  }
}
```

- Note the **comma** between host and port, not a colon.
- `TrustServerCertificate=True` is needed because the container uses a self-signed certificate. Use it for development only.
- Keep the real password in **User Secrets** rather than `appsettings.json`:

```bash
dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost,1433;Database=MyAppDb;User Id=sa;Password=Your_Strong#Pass123;TrustServerCertificate=True"
```

If your **API also runs in Docker Compose**, use the service name as the host instead of `localhost`:

```
Server=sqlserver,1433;Database=MyAppDb;...
```

## 4. EF Core: create the database and tables

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet tool install --global dotnet-ef
```

```csharp
// Program.cs
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
```

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

The database is created automatically on the first `database update`, so you don't need to create it by hand.

## 5. CRUD example

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
}

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
}
```

```csharp
var products = app.MapGroup("/products");

products.MapGet("/", async (AppDbContext db, CancellationToken ct) =>
    await db.Products.ToListAsync(ct));

products.MapGet("/{id}", async (int id, AppDbContext db, CancellationToken ct) =>
    await db.Products.FindAsync([id], ct) is { } p ? Results.Ok(p) : Results.NotFound());

products.MapPost("/", async (Product product, AppDbContext db, CancellationToken ct) =>
{
    db.Products.Add(product);
    await db.SaveChangesAsync(ct);
    return Results.Created($"/products/{product.Id}", product);
});

products.MapPut("/{id}", async (int id, Product input, AppDbContext db, CancellationToken ct) =>
{
    var product = await db.Products.FindAsync([id], ct);
    if (product is null) return Results.NotFound();

    product.Name = input.Name;
    product.Price = input.Price;
    await db.SaveChangesAsync(ct);
    return Results.NoContent();
});

products.MapDelete("/{id}", async (int id, AppDbContext db, CancellationToken ct) =>
{
    var product = await db.Products.FindAsync([id], ct);
    if (product is null) return Results.NotFound();

    db.Products.Remove(product);
    await db.SaveChangesAsync(ct);
    return Results.NoContent();
});
```

In a real project, use DTOs instead of binding entities directly, as discussed in the mapping answer.

## 6. Applying migrations automatically in development

```csharp
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}
```

Use this for development only. In production, apply migrations as a deliberate deployment step.

## 7. Browsing and querying the data

Connect with any of these using `localhost,1433`, user `sa`, and your password:

- **Azure Data Studio** or **SQL Server Management Studio** (Windows)
- **VS Code** with the **SQL Server (mssql)** extension
- **JetBrains Rider / DataGrip** database tool window
- **Command line** inside the container:
  ```bash
  docker exec -it sqlserver /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "Your_Strong#Pass123"
  ```

## Troubleshooting

- **Container exits immediately:** run `docker logs sqlserver`. The usual cause is a weak `SA` password or `ACCEPT_EULA` not set to `Y`.
- **Login failed for user 'sa':** the password only applies the **first** time the volume is created. If you change it later, delete the volume (`docker compose down -v`) and start again.
- **Can't connect from the app:** make sure the container is healthy, port 1433 isn't already used by a local SQL Server, and the connection string uses `localhost,1433`.
- **Certificate error:** add `TrustServerCertificate=True` to the connection string.
- **Apple Silicon (M1/M2/M3):** the SQL Server image is x86. It runs through emulation in Docker Desktop (enable "Use Rosetta for x86/amd64 emulation"), or use **Azure SQL Edge** as a lighter alternative, though that image has been retired, so check current status. Memory is usually the limiting factor; give Docker at least 2 GB, ideally 4.
- **Memory use:** SQL Server will take a lot of RAM. Limit it with `MSSQL_MEMORY_LIMIT_MB: 2048` in the environment.

## Tips

- **Use a named volume** so data survives restarts, and `down -v` when you want a clean slate.
- **Never reuse the `SA` account or this password in production.** For anything shared, create a dedicated login with limited permissions.
- **For tests**, use **Testcontainers** to start a throwaway SQL Server per test run (`Testcontainers.MsSql`), which pairs well with the NUnit setup from earlier.
- **Seed data** with EF Core's `HasData` or a seeding class run at startup in development.

Want a full `docker-compose.yml` that runs the .NET API and SQL Server together (with a multi-stage Dockerfile), or a Testcontainers setup for NUnit integration tests?
