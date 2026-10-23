# Kubernetes References

## Port-forwarding a Kubernetes Service

```powershell
kubectl -n emojivoto port-forward svc/web-svc 8080:50
```

This creates a temporary tunnel from the local computer to the `web-svc` Kubernetes Service in the `emojivoto` namespace.

- `-n emojivoto` selects the namespace.
- `port-forward` starts the local tunnel and keeps the terminal occupied while it runs.
- `svc/web-svc` targets the Service, which forwards to one of its backing Pods.
- `8080:50` maps local port `8080` to port `50` behind that Service.

When the command succeeds, the service is available locally at:

```text
http://localhost:8080
```

Port forwarding does not expose the service publicly or modify the cluster. The tunnel is available only from the local computer while the command is running.

Verify the ports exposed by the Service with:

```powershell
kubectl -n emojivoto get svc web-svc
```

If the Service exposes port `80`, use `8080:80` rather than `8080:50`.

## Linkerd and Kubernetes

Linkerd is a service mesh that runs on top of Kubernetes. Kubernetes deploys, schedules, and connects application Pods; Linkerd adds secure, observable, and resilient communication between services.

Linkerd typically injects a small proxy sidecar into each application Pod. Service-to-service traffic flows through these proxies, enabling:

- Mutual TLS (mTLS) to encrypt and authenticate service-to-service traffic.
- Observability for request success rate, latency, throughput, and service topology without application-code changes.
- Reliability features such as retries, timeouts, and load balancing where configured.
- Traffic policy controls, including authorization and traffic management.

For example, Kubernetes runs services such as `web-svc` and routes their traffic to backing Pods in the `emojivoto` namespace. Linkerd can secure and observe traffic from the web service to backend services. In contrast, `kubectl port-forward` provides local access to a Kubernetes workload; Linkerd primarily manages traffic between meshed workloads inside the cluster.

## Prometheus and Kubernetes

Prometheus is a monitoring system commonly deployed in Kubernetes to collect and query operational metrics. Kubernetes runs and exposes workloads; Prometheus periodically scrapes metrics endpoints, stores the resulting time-series data, and supports dashboards and alerts.

Prometheus can monitor:

- Kubernetes control-plane components, such as the API server, scheduler, controller manager, and kubelet.
- Nodes for CPU, memory, disk, and network metrics, commonly through `node-exporter`.
- Containers and Pods for resource and runtime metrics, commonly from kubelet/cAdvisor.
- Applications that expose an HTTP metrics endpoint, usually `/metrics`.
- Linkerd for request volume, success rate, latency, and TCP connection metrics.

Kubernetes service discovery lets Prometheus discover Pods and Services dynamically from labels and annotations, rather than requiring a static list of servers.

| Tool | Responsibility |
|---|---|
| Kubernetes | Runs, scales, and heals containers |
| Linkerd | Secures and observes traffic between services |
| Prometheus | Collects, stores, queries, and alerts on metrics |
| Grafana | Visualizes Prometheus metrics in dashboards |

For example, Prometheus can collect Linkerd metrics for the `emojivoto` namespace and identify whether requests handled by `web-svc` are succeeding, slowing down, or failing.

## Orchestrating Prometheus in Kubernetes

The usual Kubernetes-native approach is the **Prometheus Operator**, commonly installed through the `kube-prometheus-stack` Helm chart. The operator manages Prometheus as a Kubernetes workload and adds custom resources such as `ServiceMonitor`, `PodMonitor`, `PrometheusRule`, and `Alertmanager`.

The orchestration flow is:

1. Deploy an application that exposes a metrics endpoint.
2. Create a Kubernetes Service with a named port for that endpoint.
3. Create a `ServiceMonitor` that selects the Service by label and specifies its metrics port and path.
4. Let the Prometheus Operator reconcile the resource into Prometheus scrape configuration.
5. Define `PrometheusRule` resources for alerts and route them through Alertmanager.
6. Query the metrics in Prometheus or display them in Grafana.

Install the operator stack:

```powershell
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm repo update
helm upgrade --install prometheus prometheus-community/kube-prometheus-stack `
  --namespace monitoring --create-namespace
```

The following example assumes an application is labeled `app.kubernetes.io/name: web` and exposes metrics at `/metrics` on a container port named `metrics`:

```yaml
apiVersion: v1
kind: Service
metadata:
  name: web-metrics
  namespace: emojivoto
  labels:
    app.kubernetes.io/name: web
spec:
  selector:
    app.kubernetes.io/name: web
  ports:
    - name: metrics
      port: 9090
      targetPort: metrics
---
apiVersion: monitoring.coreos.com/v1
kind: ServiceMonitor
metadata:
  name: web
  namespace: monitoring
  labels:
    release: prometheus
spec:
  namespaceSelector:
    matchNames:
      - emojivoto
  selector:
    matchLabels:
      app.kubernetes.io/name: web
  endpoints:
    - port: metrics
      path: /metrics
      interval: 30s
```

The `release: prometheus` label must match the Prometheus instance's configured `serviceMonitorSelector`; check the Helm release values if a different selector is used. Apply the manifest after the application and the monitoring stack are running:

```powershell
kubectl apply -f web-monitoring.yaml
kubectl -n monitoring get servicemonitor
```

The Operator continuously reconciles these resources. When matching application Pods scale up, down, or are replaced, Prometheus updates its discovery targets automatically.

## Grafana and Kubernetes

Grafana is a visualization and observability platform. It queries data sources, such as Prometheus, and presents the results as dashboards, charts, tables, and alerts. Grafana usually does not collect Kubernetes metrics itself; instead, it visualizes metrics that Prometheus has collected from the cluster, nodes, Pods, applications, and Linkerd.

In Kubernetes, Grafana runs as a Deployment behind a Service. It can use Kubernetes Secrets for credentials, a PersistentVolumeClaim (PVC) for its database and state, ConfigMaps for provisioned dashboards, and an Ingress or Gateway for controlled external access.

For example, a Grafana dashboard can show Prometheus and Linkerd metrics for the `emojivoto` namespace, including request rate, success rate, latency, Pod resource usage, and application availability.

## Orchestrating Grafana in Kubernetes

The `kube-prometheus-stack` Helm chart installed in the previous section deploys Grafana by default. It configures Grafana with Prometheus as a data source and includes Kubernetes and Prometheus dashboards. This is the recommended starting point because the Prometheus, Alertmanager, Grafana, and dashboard configuration are managed as one monitoring stack.

Check the Grafana workload and Service:

```powershell
kubectl -n monitoring get deployment,service -l app.kubernetes.io/name=grafana
```

For temporary local access, forward a local port to the Grafana Service:

```powershell
kubectl -n monitoring port-forward svc/prometheus-grafana 3000:80
```

Then open:

```text
http://localhost:3000
```

The release name determines the Service name. If the Helm release uses a name other than `prometheus`, list the Services in the `monitoring` namespace to identify the Grafana Service:

```powershell
kubectl -n monitoring get svc
```

### Kubernetes-managed Grafana configuration

Operate Grafana configuration declaratively rather than making important dashboard changes only through its UI:

1. Store Grafana administrator credentials in a Kubernetes Secret or configure an external identity provider.
2. Enable persistent storage so dashboards, users, and settings survive Pod replacement.
3. Provision data sources and dashboards from version-controlled Kubernetes manifests or Helm values.
4. Expose Grafana through an Ingress or Gateway protected by TLS and authentication when users require access outside the cluster.
5. Use resource requests, limits, health probes, and a controlled upgrade process as with other Kubernetes workloads.

The chart's dashboard sidecar can discover dashboard ConfigMaps with the `grafana_dashboard: "1"` label. The following ConfigMap provisions a dashboard into Grafana when that sidecar is enabled:

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: emojivoto-dashboard
  namespace: monitoring
  labels:
    grafana_dashboard: "1"
data:
  emojivoto.json: |
    {
      "title": "Emojivoto",
      "panels": [],
      "schemaVersion": 39,
      "version": 1
    }
```

Apply it with:

```powershell
kubectl apply -f emojivoto-dashboard.yaml
```

Replace the empty `panels` array with a reviewed dashboard definition that uses the Prometheus data source. Keep dashboard JSON and Helm values in source control so that Grafana can be recreated consistently after a cluster change or disaster recovery event.

## OpenTelemetry and Kubernetes

OpenTelemetry (OTel) is an open standard and set of tools for producing, collecting, and exporting observability data. It standardizes three primary telemetry signals:

| Signal | Question it answers |
|---|---|
| Metrics | How much? For example, request rate, error count, and latency percentiles |
| Logs | What happened? Timestamped application and system events |
| Traces | Where and why did a request become slow or fail? The path of one request across services |

Kubernetes runs the workloads but does not provide complete application-level distributed tracing. Applications use an OpenTelemetry SDK or auto-instrumentation to emit telemetry, and an OpenTelemetry Collector receives, processes, and exports it to observability backends.

```text
Application Pods
  -> OpenTelemetry SDK or auto-instrumentation
  -> OpenTelemetry Collector in Kubernetes
  -> Prometheus, Grafana, Tempo, Jaeger, Loki, Azure Monitor, or another backend
```

OpenTelemetry complements the other tools in this document:

- **Prometheus** stores and queries metrics. OpenTelemetry metrics can be exposed for Prometheus scraping or exported to a compatible backend.
- **Grafana** visualizes metrics, logs, and traces from configured backends.
- **Linkerd** automatically emits service-mesh telemetry. OpenTelemetry adds application-level operations, traces, and business-relevant measurements.
- **Kubernetes** orchestrates both the application and the collectors.

## Orchestrating OpenTelemetry for a file-handling web application

For a web application that receives, validates, scans, transforms, and stores files, instrument the complete request flow. A trace can show whether a slow upload was caused by request handling, virus scanning, object storage, a database update, or a downstream notification.

Useful telemetry includes:

- HTTP request duration, response status, request size, and error count.
- Spans for `file.validate`, `file.scan`, `file.transform`, `file.store`, and `file.notify`.
- Queue delay and processing duration for asynchronous file-processing jobs.
- Metrics for files received, rejected, scanned, processed, failed, and total bytes processed.
- Structured logs correlated with the current trace and span IDs.

Do **not** record file contents, access tokens, session cookies, raw file names, or personally identifying information in telemetry. Prefer a generated file identifier, file type, size bucket, status, and error category. Apply sampling and retention rules so high-volume uploads do not produce unnecessary cost or sensitive data exposure.

### Collector deployment pattern

Deploy an OpenTelemetry Collector as a Kubernetes `Deployment` for application telemetry. Application Pods export OTLP data to the Collector Service, which batches and enriches it before exporting it to metrics, tracing, and logging backends. Use a `DaemonSet` when the Collector must also capture node-local logs or host-level telemetry.

Install the OpenTelemetry Operator, which manages Collector workloads and instrumentation resources:

```powershell
helm repo add open-telemetry https://open-telemetry.github.io/opentelemetry-helm-charts
helm repo update
helm upgrade --install opentelemetry-operator open-telemetry/opentelemetry-operator `
  --namespace opentelemetry-operator-system --create-namespace
```

The following example creates a centralized Collector. It receives OTLP telemetry from application Pods, exposes received metrics for Prometheus scraping, and exports traces to a Tempo-compatible OTLP endpoint. Replace the Tempo endpoint with the tracing backend used by the cluster.

```yaml
apiVersion: opentelemetry.io/v1beta1
kind: OpenTelemetryCollector
metadata:
  name: otel-collector
  namespace: monitoring
spec:
  mode: deployment
  config:
    receivers:
      otlp:
        protocols:
          grpc:
          http:
    processors:
      memory_limiter:
        check_interval: 1s
        limit_mib: 512
      batch:
    exporters:
      prometheus:
        endpoint: 0.0.0.0:8889
      otlp:
        endpoint: tempo.monitoring.svc.cluster.local:4317
        tls:
          insecure: true
    service:
      pipelines:
        metrics:
          receivers: [otlp]
          processors: [memory_limiter, batch]
          exporters: [prometheus]
        traces:
          receivers: [otlp]
          processors: [memory_limiter, batch]
          exporters: [otlp]
```

Apply the Collector manifest:

```powershell
kubectl apply -f otel-collector.yaml
kubectl -n monitoring get opentelemetrycollector,pods,svc
```

### Configuring the file application

The application must include an OpenTelemetry SDK or use compatible auto-instrumentation. Configure it to export OTLP data to the Collector Service created above. The exact environment-variable names are part of the OpenTelemetry specification and work with many SDKs:

```yaml
env:
  - name: OTEL_SERVICE_NAME
    value: file-web
  - name: OTEL_EXPORTER_OTLP_ENDPOINT
    value: http://otel-collector-collector.monitoring.svc.cluster.local:4318
  - name: OTEL_EXPORTER_OTLP_PROTOCOL
    value: http/protobuf
  - name: OTEL_RESOURCE_ATTRIBUTES
    value: deployment.environment=production,service.namespace=emojivoto
```

The generated Collector Service name can vary with the Operator version and resource configuration. Confirm it before deploying the application:

```powershell
kubectl -n monitoring get svc
```

Add the environment variables to the file application's Deployment after confirming the Service name. Configure application instrumentation to create spans around the file-processing operations and to propagate trace context to background workers and downstream HTTP calls. This preserves one trace across the full lifecycle of an uploaded file without exposing the file itself.

Finally, create a `ServiceMonitor` for the Collector's Prometheus metrics endpoint if it is not already selected by the installed monitoring stack. Query trace data in a tracing backend such as Tempo or Jaeger, and use Grafana to correlate traces, logs, and Prometheus metrics during an upload failure or latency investigation.
