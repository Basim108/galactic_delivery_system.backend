# Getting started

This guide covers running the SpaceTruckers API locally and smoke-testing the main endpoints.

For deeper development workflows (EF Core migrations, testing with coverage, SonarQube, Structurizr), see [`docs/development.md`](development.md).

## Run locally with Docker Compose (API + PostgreSQL)

A local compose setup is provided in `docker-compose.yml`.

```bash
docker compose up --build
```

- API: `http://localhost:8080`
- PostgreSQL: `localhost:5432` (service name is `db` inside the compose network)

Tip: in Development, the API also exposes an OpenAPI document at `http://localhost:8080/openapi/v1.json`.

## Reset the database volume (when you need a clean database)

The compose file uses a named volume called `db_data`. To stop the stack and remove the database volume:

```bash
docker compose down -v
```

Then start everything again:

```bash
docker compose up --build
```

## Test the API with curl

All examples assume:

```bash
export BASE_URL='http://localhost:8080'
```

Notes about enums in request payloads:

- `ResourceStatus`: `0 = Available`, `1 = Unavailable`
- `IncidentSeverity`: `0 = Informational`, `1 = Warning`, `2 = Catastrophic`

### Create a driver

```bash
curl -i -X POST "$BASE_URL/api/drivers/" \
  -H 'Content-Type: application/json' \
  -d '{"name":"Han Solo","status":0}'
```

### Create a vehicle

```bash
curl -i -X POST "$BASE_URL/api/vehicles/" \
  -H 'Content-Type: application/json' \
  -d '{"name":"Millennium Falcon","type":"SpaceFreighter","cargoCapacity":100,"status":0}'
```

### Create a route

```bash
curl -i -X POST "$BASE_URL/api/routes/" \
  -H 'Content-Type: application/json' \
  -d '{"name":"Kessel Run","checkpoints":["Tatooine","Kessel","Corellia"]}'
```

### Create a trip

Replace `{{driverId}}`, `{{vehicleId}}`, and `{{routeId}}` with the GUIDs returned by the previous calls.

```bash
curl -i -X POST "$BASE_URL/api/trips/" \
  -H 'Content-Type: application/json' \
  -d '{"driverId":"{{driverId}}","vehicleId":"{{vehicleId}}","routeId":"{{routeId}}","cargoRequirement":10}'
```

### Start a trip

Replace `{{tripId}}` with the trip GUID.

```bash
curl -i -X POST "$BASE_URL/api/trips/{{tripId}}/start" \
  -H 'Content-Type: application/json' \
  -d '{"requestId":"req-1"}'
```

### Reach a checkpoint

```bash
curl -i -X POST "$BASE_URL/api/trips/{{tripId}}/checkpoints/reach" \
  -H 'Content-Type: application/json' \
  -d '{"checkpointName":"Tatooine"}'
```

### Report an incident

```bash
curl -i -X POST "$BASE_URL/api/trips/{{tripId}}/incidents" \
  -H 'Content-Type: application/json' \
  -d '{"type":"AsteroidField","severity":1,"description":"Minor hull damage"}'
```

### Complete a trip

```bash
curl -i -X POST "$BASE_URL/api/trips/{{tripId}}/complete"
```

### Fetch trip details and summary

```bash
curl -i "$BASE_URL/api/trips/{{tripId}}"
```

```bash
curl -i "$BASE_URL/api/trips/{{tripId}}/summary"
```
