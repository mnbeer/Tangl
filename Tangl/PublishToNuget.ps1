$apiKey=$args[0]
dotnet pack --Configuration Release
dotnet nuget push bin\MCD\release\Tangl.1.1.0.nupkg -k $apiKey -s https://api.nuget.org/v3/index.json