#!/bin/bash

set -e

echo "Publishing..."

cd ./LyricsScraperNET
# Version sets the package version and the assembly and file versions.
dotnet pack -c Release LyricsScraperNET.csproj -p:Version="$RELEASE_VERSION"
# dotnet nuget: the nuget CLI is not installed on ubuntu-24.04 runners.
# --skip-duplicate lets a failed workflow be re-run after the package is already pushed.
dotnet nuget push "./bin/Release/LyricsScraperNET.$RELEASE_VERSION.nupkg" \
  --api-key "$NUGET_TOKEN" \
  --source https://api.nuget.org/v3/index.json \
  --skip-duplicate

cd ..
