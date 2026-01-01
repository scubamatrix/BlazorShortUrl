# BlazorShortUrl

This repo is a url shortener web application that is similar in purpose to tinyurl and other paid SaaS applications.

The code is using ASP.NET Core with Blazor server-side rendering (SSR) and SQL Server for Linux remote database that has been installed and configured separately.

This repo is primarily based on [1], [2], and [3].

The majority of the code was generated using the Visual Studio Professional and JeBrains Rider, and deployment to container using the tutorials in [7] and [8].

NOTE: When starting a new .NET project, it is often an exercise in patience to find the correct syntax for the SQL Server Connection String.

```bash
    dotnet dev-certs https --clean
    dotnet dev-certs https
    dotnet dev-certs https -ep ${HOME}/.aspnet/https/aspnetapp.pfx -p PASSWORD
    dotnet dev-certs https --trust
```

## User Management

The User Management feature is based on code from [3], [9], and [10].

## Details for Troubleshooting

Here is a summary of the main steps required to manually reproduce the project with may help in troubleshooting any problems.

### Install Entity Framework Core

To add EF Core to an application, install the NuGet package for the database provider you want to use [4].

To install or update NuGet packages, you can use the .NET Core command-line interface (CLI), the Visual Studio Package Manager Dialog, or the Visual Studio Package Manager Console [4].

```zsh
    dotnet add package Microsoft.EntityFrameworkCore
    dotnet add package Microsoft.EntityFrameworkCore.Sqlite
    dotnet add package Microsoft.EntityFrameworkCore.SqlServer
    dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
```

### Install the EF Core tools

The EF Core tools can be used for EF Core tasks in your project such as creating and applying database migrations or creating an EF Core model based on an existing database [4].

Two sets of tools are available:

- The .NET Core command-line interface (CLI) tools can be used on Windows, Linux, or macOS. These commands begin with dotnet ef.

- The Package Manager Console (PMC) tools run in Visual Studio on Windows. These commands start with a verb, for example Add-Migration, Update-Database.

```zsh
    # dotnet ef must be installed as a global or local tool.
    dotnet tool install --global dotnet-ef

    # Install the latest Microsoft.EntityFrameworkCore.Design package.
    dotnet add package Microsoft.EntityFrameworkCore.Design

    # Create Identity Scaffolding (see below)
    # Create EF Migrations (see below)

    # Apply migrations to create database and schema
    dotnet tool update
```

### Create Identity Scaffolding

Identity is typically configured using a SQL Server database to store user names, passwords, and profile data.

Here we add the `Register`, `Login`, `LogOut`, and `RegisterConfirmation` files [5]:

```zsh
    # Update to latest stable version of tool
    dotnet tool update -g dotnet-aspnet-codegenerator
```

```zsh
    dotnet add package Microsoft.VisualStudio.Web.CodeGeneration.Design
    dotnet add package Microsoft.EntityFrameworkCore.Design
    dotnet add package Microsoft.EntityFrameworkCore.SqlServer
    dotnet add package Microsoft.EntityFrameworkCore.Tools
    dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
    dotnet add package Microsoft.AspNetCore.Identity.UI
```

```zsh
    dotnet aspnet-codegenerator identity -h

    dotnet aspnet-codegenerator identity -dbProvider sqlserver -dc BlazorApp.Data.AppDbContext --files "Account.Register;Account.Login;Account.Logout"
```

### Create EF Migrations

Database migration files based on the Entities classes that are used to create the SQL database for the API and update the database when the entities are changed [5].

Migrations are generated using the Entity Framework Core CLI [5] and [6].

Keep in mind that you may need to recreate the EF migrations and the syntax can be a little different on Linux, macOS, or Windows.

SQL Server EF Core Migrations (Linux):

```zsh
    # drop database if exists
    dotnet ef database drop --context AppDbContext
    dotnet ef database drop --context DataContext

    # delete last migration
    dotnet ef migrations remove --context AppDbContext
    dotnet ef migrations remove --context DataContext

    # Requires admin privileges
    dotnet ef migrations add InitialCreate --context AppDbContext --output-dir Migrations
    dotnet ef migrations add InitialCreate --context DataContext --output-dir Migrations/DataMigrations

    dotnet ef migrations add AddUserRole --context AppDbContext --output-dir Data/Migrations

    # Requires admin privileges
    dotnet ef database update --context AppDbContext
    dotnet ef database update --context DataContext
```

Clean the development certificates before deployment, especially Docker container:

```zsh
  dotnet dev-certs https --clean
```

SQL Server EF Core Migrations (MacOS):

```zsh
    export ASPNETCORE_ENVIRONMENT=Production
    dotnet ef migrations add InitialCreate --context DataContext --output-dir Migrations/SqlServerMigrations
```

SQL Server EF Core Migrations (Windows):

```shell
    set ASPNETCORE_ENVIRONMENT=Production
    dotnet ef migrations add InitialCreate --context DataContext --output-dir Migrations/SqlServerMigrations
```

### Run the App

```zsh
    # using vscode
    dotnet watch
```

## References

[1]: C. Xu, "[Building a URL Shortener Web App using Minimal APIs in .NET 6](https://medium.com/geekculture/building-a-url-shortener-web-app-using-minimal-apis-in-net-6-99334ac6e98b)," Geek Culture, July 18, 2021.

[2]: C. McQuillan, "[Fast Builds: Make a URL Shortener With .NET](https://quill.codes/posts/fast-prototyping-url-shortener/)," Sept. 22, 2020.

[3]: [Identity Manager Blazor United](https://github.com/mguinness/IdentityManagerBlazorUnited)

[4]: [Installing Entity Framework Core](https://learn.microsoft.com/en-gb/ef/core/get-started/overview/install)

[5]: [Scaffold Identity in ASP.NET Core projects](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/scaffold-identity?view=aspnetcore-9.0&tabs=visual-studio)

[6]: [Managing Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/managing?tabs=dotnet-core-cli#remove-a-migration)

[7]: [ASP.NET Core in a container](https://code.visualstudio.com/docs/containers/quickstart-aspnet-core)

[8]: [ASP.NET Core Development with Docker](https://www.jetbrains.com/guide/dotnet/tutorials/docker-dotnet/aspnet-development-docker/)

[9]: [Implement API Key Authentication in ASP.NET Core](https://code-maze.com/aspnetcore-api-key-authentication/)

[10]: [Sending and Receiving JSON using HttpClient with System.Net.Http.Json](https://www.stevejgordon.co.uk/sending-and-receiving-json-using-httpclient-with-system-net-http-json)

[11]: [Hosting ASP.NET Core images with Docker over HTTPS](https://learn.microsoft.com/en-us/aspnet/core/security/docker-https?view=aspnetcore-9.0)

[12]: [How to securely reverse-proxy ASP.NET Core web apps](https://anthonysimmon.com/securely-reverse-proxy-aspnet-core-web-apps/)

[13]: [Configure ASP.NET Core to work with proxy servers and load balancers](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-9.0)

[14]: [Write custom ASP.NET Core middleware](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/write?view=aspnetcore-9.0)

[15]: [HTTP Client in C#: Best Practices for Experts](https://medium.com/@iamprovidence/http-client-in-c-best-practices-for-experts-840b36d8f8c4)
