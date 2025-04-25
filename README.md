# VisualAmeco
Visualizer for macroeconomic indicators from the AMECO database.

## Migrations

This section outlines how to create and apply new database migrations using Entity Framework Core. Migrations are essential for evolving the database schema as your application's data model changes over time.

### Prerequisites

* **.NET SDK:** Ensure you have the .NET SDK installed on your system.
* **Entity Framework Core Tools:** The EF Core Tools should be installed globally. If you haven't already, you can install them by running:
    ```bash
    dotnet tool install --global dotnet-ef
    ```
    or update if you have them installed:
    ```bash
    dotnet tool update --global dotnet-ef
    ```
* **NuGet Package:** Your startup project (e.g., `VisualAmeco.API`) needs to reference the `Microsoft.EntityFrameworkCore.Design` NuGet package. You can add it by navigating to your startup project directory and running:
    ```bash
    dotnet add package Microsoft.EntityFrameworkCore.Design
    ```

### Creating a New Migration

1.  **Navigate to your Data project directory:** In your terminal, change the current directory to the project that contains your `DbContext` (e.g., `VisualAmeco.Data`).

    ```bash
    cd /path/to/your/VisualAmeco.Data
    ```

2.  **Run the `dotnet ef migrations add` command:** This command will scaffold a new migration based on the changes you've made to your entity classes and `DbContext`. Replace `<MigrationName>` with a descriptive name for your migration (e.g., `AddBookTitle`, `CreateAuthorTable`).

    ```bash
    dotnet ef migrations add <MigrationName> --startup-project ../VisualAmeco.API
    ```

    * `dotnet ef migrations add`: The command to add a new migration.
    * `<MigrationName>`: A name that clearly describes the changes included in this migration.
    * `--startup-project ../VisualAmeco.API`: Specifies the startup project to be used for finding the application's configuration and dependencies. Adjust the path (`../VisualAmeco.API`) if your startup project is located in a different relative path from your Data project.

3.  **Review the generated migration files:** After running the command successfully, a new directory named `Migrations` will be created (if it doesn't exist) in your Data project. This directory will contain two files:
    * `<Timestamp>_<MigrationName>.cs`: This file contains the code to apply the changes to the database schema (the `Up()` method) and to revert those changes (the `Down()` method).
    * `<Timestamp>_<MigrationName>.Designer.cs`: This file contains metadata about the migration.

    Review these files to ensure that the generated code accurately reflects the changes you intended to make to your database schema. You can manually edit these files if needed, but be cautious when doing so.

### Applying Migrations to the Database

Once you have created a migration, you need to apply it to your database to update the schema.

1.  **Ensure your database connection string is correctly configured:** Check your application's configuration file (e.g., `appsettings.json` in your startup project) to make sure the connection string for your PostgreSQL database is accurate.

2.  **Navigate to your Data project directory (if you are not already there):**

    ```bash
    cd /path/to/your/VisualAmeco.Data
    ```

3.  **Run the `dotnet ef database update` command:** This command will apply any pending migrations to your database.

    ```bash
    dotnet ef database update --startup-project ../VisualAmeco.API
    ```

    * `dotnet ef database update`: The command to apply pending migrations.
    * `--startup-project ../VisualAmeco.API`: Specifies the startup project, similar to the `add` command.

4.  **Verify the database changes:** After the command completes successfully, connect to your PostgreSQL database and verify that the schema has been updated according to the migration you applied.

### Additional Migration Commands

Here are some other useful `dotnet ef migrations` commands:

* **`dotnet ef migrations list --startup-project ../VisualAmeco.API`**: Lists all applied and pending migrations.
* **`dotnet ef migrations remove --startup-project ../VisualAmeco.API`**: Removes the last applied migration (use with caution, as this can lead to data loss if the database schema has diverged significantly).
* **`dotnet ef database drop --startup-project ../VisualAmeco.API --force`**: Drops the database (use with extreme caution, as this will delete all data).

