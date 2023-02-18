#!/bin/bash

set -e

echo "Publishing..."

cd ./LyricsScraperNET
dotnet pack -c Release LyricsScraperNET.csproj -p:PackageVersion="$RELEASE_VERSION"
nuget push "./bin/Release/LyricsScraperNET.$RELEASE_VERSION.nupkg"\
  -ApiKey "$NUGET_TOKEN"\
  -NonInteractive\
  -Source https://www.nuget.org/api/v2/package

cd ..
