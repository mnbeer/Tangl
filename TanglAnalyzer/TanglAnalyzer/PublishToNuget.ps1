$apiKey=$args[0]
#dotnet pack --configuration Release
dotnet build --configuration Release
dotnet nuget push bin\release\TanglAnalyzer.1.2.1.nupkg -k $apiKey -s https://api.nuget.org/v3/index.json