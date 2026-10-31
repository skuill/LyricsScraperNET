#!/bin/bash

set -e

# The tag is the version: plain SemVer without "v" (3.0.0, 3.1.0-beta.1).
if [[ ! "$RELEASE_VERSION" =~ ^[0-9]+\.[0-9]+\.[0-9]+(-[0-9A-Za-z.-]+)?$ ]]; then
  echo "Tag '$RELEASE_VERSION' is not a version like 3.0.0 or 3.1.0-beta.1" >&2
  exit 1
fi

echo "Publishing $RELEASE_VERSION..."

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
