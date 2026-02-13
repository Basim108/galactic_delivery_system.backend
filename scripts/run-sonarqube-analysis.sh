#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

dotnet tool restore

dotnet sonarscanner begin /k:"galactic_delivery_system.backend" \
  /o:"space_trucker" \
  /d:sonar.host.url="http://localhost:9000" \
  /d:sonar.token="${SONAR_TOKEN}" \
  /d:sonar.exclusions="**/bin/**,**/obj/**" \
  /d:sonar.coverage.exclusions="**/*Tests.cs" \
  /d:sonar.cs.opencover.reportsPaths="**/TestResults/**/coverage.opencover.xml"

dotnet build -c Release

dotnet test  -c Release \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover

dotnet sonarscanner end /d:sonar.token="${SONAR_TOKEN}"
