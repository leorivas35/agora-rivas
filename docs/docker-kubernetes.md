# Docker and Kubernetes Deployment

## Architecture

Build the ASP.NET Core API and its referenced projects into one container image. Kubernetes runs that image in a Deployment and exposes it internally through a Service. An Ingress can route public HTTPS traffic to the Service.

```text
Client -> Ingress (TLS) -> Service -> API Pods
```

The API targets .NET 10. `API` references `Infrastructure`, which references `Core`, so build the image from the repository root. The current project has no database configuration or health-check endpoint. OpenAPI is enabled only in Development.

## Docker

Add a `Dockerfile` at the repository root:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY API/API.csproj API/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
COPY Core/Core.csproj Core/
RUN dotnet restore API/API.csproj

COPY . .
RUN dotnet publish API/API.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .
# USER $APP_UID

ENTRYPOINT ["dotnet", "API.dll"]
```

Add a `.dockerignore` at the repository root:

```dockerignore
.git
.vs
.vscode
**/bin
**/obj
```

Build and run locally from the repository root:

```bash
docker build -t agora-rivas-api:local .
docker run --rm -p 8080:8080 agora-rivas-api:local
```

## Kubernetes

Create `k8s/api.yaml`. Replace the example image with the image tag pushed to your registry:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: agora-rivas-api
spec:
  replicas: 2
  selector:
    matchLabels:
      app: agora-rivas-api
  template:
    metadata:
      labels:
        app: agora-rivas-api
    spec:
      containers:
        - name: api
          image: your-registry/agora-rivas-api:1.0.0
          ports:
            - name: http
              containerPort: 8080
          securityContext:
            runAsNonRoot: true
            allowPrivilegeEscalation: false
            capabilities:
              drop: ["ALL"]
          startupProbe:
            tcpSocket:
              port: http
            periodSeconds: 5
            failureThreshold: 30
          readinessProbe:
            tcpSocket:
              port: http
            periodSeconds: 10
          livenessProbe:
            tcpSocket:
              port: http
            periodSeconds: 20
          resources:
            requests:
              cpu: 100m
              memory: 128Mi
---
apiVersion: v1
kind: Service
metadata:
  name: agora-rivas-api
spec:
  selector:
    app: agora-rivas-api
  ports:
    - name: http
      port: 80
      targetPort: http
  type: ClusterIP
```

The TCP probes only confirm that the container port is open. For application-level checks, add an ASP.NET health-check endpoint and configure the probes to make HTTP requests to it.

## Build, Push, and Deploy

Push the image to a registry accessible by your Kubernetes cluster, then apply the manifest:

```bash
docker tag agora-rivas-api:local your-registry/agora-rivas-api:1.0.0
docker push your-registry/agora-rivas-api:1.0.0

kubectl apply -f k8s/api.yaml
kubectl rollout status deployment/agora-rivas-api
kubectl port-forward service/agora-rivas-api 8080:80
```

With port-forwarding active, access the API at `http://localhost:8080`. For public access, configure an Ingress with your cluster's ingress controller and TLS certificate. Keep the Service as `ClusterIP`.

## HTTPS and Configuration

The current API calls `UseHttpsRedirection()`, but the container listens on HTTP port 8080. A common production setup terminates TLS at the Ingress and forwards traffic to the Service over HTTP. Configure forwarded headers if the application needs to recognize the original HTTPS scheme.

Store environment-specific settings in Kubernetes ConfigMaps and sensitive values in Kubernetes Secrets; do not bake them into the image.