$apiKey=$args[0]
dotnet pack --configuration Release
dotnet nuget push bin\MCD\release\TanglAnalyzer.1.2.0.nupkg -k $apiKey -s https://api.nuget.org/v3/index.json