dotnet build src --no-incremental

dotnet test --no-build -l "console;verbosity=detailed" src
