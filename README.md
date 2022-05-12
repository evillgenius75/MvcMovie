

Based off the [ASP.NET Core MVC Tutorial](https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-mvc-app&tabs=visual-studio-code)

```bash
dotnet tool install --global dotnet-ef
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.SQLite

# You will also need to make sure that you set $DOTNET_ROOT to the installation location of .NET
export DOTNET_ROOT=$HOME/.dotnet
# And add the tools directory to your path as well

export PATH=$DOTNET_ROOT:$DOTNET_ROOT/tools:$PATH
```

Create the database:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Run with:
```bash
dotnet run --urls http://+:5000
```