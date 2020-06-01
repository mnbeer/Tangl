dotnet build --configuration Release
nuget pack -Properties Configuration=Release -OutputDirectory "bin\Release"
nuget init bin\Release C:\NuGetPackages