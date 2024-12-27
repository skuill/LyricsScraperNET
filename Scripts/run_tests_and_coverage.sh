#!/bin/bash

# Change to the root directory of the project
cd "$(dirname "$0")/.."

# Run the unit tests with coverlet and output the results in Cobertura format
dotnet test Tests/LyricsScraperNET.UnitTest \
  --configuration Release \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults \
  --settings coverlet.runsettings
