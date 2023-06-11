

# How to Setup a Basic EFCore + Hotchocolate Example

Create a basic project.

## basic setup of project.

We create a new ASP.net Core web project and install HotChocolate and EFCore.


    dotnet new web -n HotChocolateEFCoreDemo
    cd HotChocolateEFCoreDemo
    dotnet add package HotChocolate.AspNetCore
    dotnet add package HotChocolate.Data.EntityFramework
    dotnet add package Microsoft.EntityFrameworkCore.Sqlite


## Set up SQLite database.

We copy over a simple model.cs and initiate the database;
for this we first install a commandline tool for dotnet (dotnet-ef)

  
    cp ../model.cs model.cs
    cp ../Program.cs Program.cs
    dotnet tool install --global dotnet-efexpl
    dotnet add package Microsoft.EntityFrameworkCore.Design
    dotnet ef migrations add InitialCreate
    dotnet ef database update

    
## Important remarks about the code structure;

The mutation adding a post, which takes a `Post` as input has a problem; namely graphQL will demand all fields are filled.
We therefore make the Blog? variable nullable; we don't wan't to repeat the entire Blog, the ID field is used by EF to correctly associate things.
