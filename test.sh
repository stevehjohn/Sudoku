dotnet build src --no-incremental

dotnet test -l "console;verbosity=detailed" src
