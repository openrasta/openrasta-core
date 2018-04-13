#!/bin/bash
msbuild ../core/openrasta-core.sln
nuget pack ../core/src/OpenRasta/OpenRasta.csproj -version "2.6.0-ci-$1"
mv "openrasta-core.2.6.0-ci-$1.nupkg" ~/Dev/nuget 
nuget update src/openrasta-hosting-aspnet.sln -pre -source LocalStuff 


