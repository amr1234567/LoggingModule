# Serilog Logging for ASP.NET Core with SQL Server

This guide walks you through integrating Serilog into an ASP.NET Core application using SQL Server. You'll install the required packages, configure structured logging to the database, add request/response logging middleware, integrate with Entity Framework Core (EF Core), and optionally set up an MVC interface to browse logs with a modern, responsive UI.

## Prerequisites

- **Database**: SQL Server for persisting logs.
- **ASP.NET Core**: Version 6.0 or later is recommended.
- **Entity Framework Core**: For migrations and data access.

## Step 1: Install Required NuGet Packages

Install these packages in your ASP.NET Core project to enable Serilog and SQL Server logging:

- `Serilog.AspNetCore`: Serilog integration with ASP.NET Core.
- `Serilog.Expressions`: Advanced filtering and formatting of log events.
- `Serilog.Sinks.MSSqlServer`: Writes logs to SQL Server.
- `Newtonsoft.Json`: JSON serialization/deserialization.

Use the .NET CLI:

```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Expressions
dotnet add package Serilog.Sinks.MSSqlServer
dotnet add package Newtonsoft.Json
```

## Step 2: Configure Serilog for Database Logging

Set up Serilog to log to SQL Server:

1. **Copy configuration files**
   - Copy the `Configurations` folder (Serilog configuration) to your infrastructure or shared project.
   - Ensure the connection string name used inside `AddLoggingServices` matches your app settings (rename if needed).

2. **Register logging services**
   - In `Program.cs` add the configuration call:
    ```csharp
    var builder = WebApplication.CreateBuilder(args);
    
    // Add services to the container
    builder.Services.AddControllersWithViews();
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    
    // Add Serilog logging services
    builder.AddLoggingServices();
    
    var app = builder.Build();
    ```

## Step 3: Add Request/Response Logging Middleware

Use the provided middleware to capture HTTP requests and responses:

1. **Copy middleware**
   - Copy the `Middlewares` folder into your presentation layer (where controllers live).

2. **Register middleware in the pipeline**
   - In `Program.cs` add the middleware:
     ```csharp
     // Configure the HTTP request pipeline
     if (app.Environment.IsDevelopment())
     {
         app.UseDeveloperExceptionPage();
     }
     else
     {
         app.UseExceptionHandler("/Home/Error");
         app.UseHsts();
     }
     
     app.UseHttpsRedirection();
     app.UseStaticFiles();
     app.UseRouting();
     
     // Add request/response logging middleware BEFORE authentication/authorization
     app.UseMiddleware<RequestResponseLoggingMiddleware>();
     
     app.UseAuthentication();
     app.UseAuthorization();
     
     app.MapControllerRoute(
         name: "default",
         pattern: "{controller=Home}/{action=Index}/{id?}");
     ```
   
   - **Placement tips**
     - To capture **all requests** (including unauthorized), place it **before** `app.UseAuthentication();` and `app.UseAuthorization();`.
     - To capture **only authorized** traffic, place it **after** both.

3. **Mark controllers to log**
   - Implement `IApiLoggable` for each controller you want to include in logs:
     ```csharp
     public class AppLogicController : BaseAPIController, IApiLoggable
     {
     }
     ```

## Step 4: Integrate with Entity Framework Core (EF Core)

Configure your `DbContext` so you can query log records when needed:

1. **Initialize migrations**
   - Ensure your database exists and EF Core migrations are set up. The Serilog sink will create a `Logs` table automatically upon first write.

2. **Add `HttpRequestLog` entity**
   - In your `DbContext` add:
     ```csharp
     public DbSet<HttpRequestLog> HttpRequestLogs { get; set; }
     ```

3. **Map to the `Logs` table**
   - In `OnModelCreating`:
     ```csharp
     protected override void OnModelCreating(ModelBuilder builder)
     {
         base.OnModelCreating(builder);
         builder.Entity<HttpRequestLog>().ToTable("Logs");
     }
     ```

4. **Troubleshooting design-time DbContext (optional)**
   - If `add-migration` or `update-database` fails due to design-time context creation, copy `Helpers/ApplicationDbContextFactory.cs` next to your `DbContext` and update its connection string name to match your app.

5. **Migration Manipulation**
when you want EF Core to recognize and track the structure of the Serilog `Logs` table without actually creating or dropping it, follow these steps:
    1. **Generate a migration**  
    Create a migration that includes the `Logs` table mapping so EF Core’s **model snapshot** contains its schema.

    2. **Edit the migration**  
      - Remove the `CreateTable` statement for `Logs` from the `Up` method.  
      - Remove the `DropTable` statement for `Logs` from the `Down` method.  

    3. **Reason**  
      Even though you’ve deleted the table creation and drop commands from the migration, EF Core will still store the table’s schema in the model snapshot.  
      This means:
      - EF Core believes the table already exists.
      - EF Core will not try to create it again.
      - EF Core will still generate `ALTER TABLE` statements in future migrations if you change the `Logs` entity.

    4. **(Optional) Completely exclude the Logs table from migrations**  
      If you want EF Core to **never** include the `Logs` table in any migrations, configure it in `OnModelCreating`:
      ```csharp
      modelBuilder.Entity<HttpRequestLog>(entity =>
      {
          entity.ToTable("Logs"); // or whatever Serilog named it
          entity.Metadata.SetIsTableExcludedFromMigrations(true);
      });

## Step 5: (Optional) Add MVC Interface for Viewing Logs

Provide a responsive UI to browse logs:

1. **Copy MVC artifacts**
   - Copy `Controllers/LogsController.cs` to your project.
   - Copy the `Views/Logs` folder to your project's `Views`.
   - Copy services, contracts, and DTOs to their appropriate layers (service layer, DTO folder, etc.).

2. **Copy static assets (CSS & JavaScript)**
   - CSS: `wwwroot/css/logging-stylesheet.css` → your `wwwroot/css/`.
   - JS: copy to your `wwwroot/js/`:
     - `wwwroot/js/logging-scripts.js`
     - `wwwroot/js/helpers/time-helper.js`
   - Folder structure:
     ```
     wwwroot/
     ├── css/
     │   └── logging-stylesheet.css
     └── js/
         ├── logging-scripts.js
         └── helpers/
             └── time-helper.js
     ```

3. **Wire up dependencies**
   - Ensure the controller and services reference the correct `DbContext` and namespaces.
   - If you use a result pattern (e.g., `Result<T>`), align return types; otherwise, return plain models.

4. **Dependency injection**
   - Register the logging service:
     ```csharp
     builder.Services.AddScoped<ILogServices, LogServices>();
     ```


## Step 6: Validate the Setup

1. **Run the application**
   - Hit endpoints in controllers that implement `IApiLoggable`.

2. **Inspect the database**
   - Confirm records exist in the `Logs` table.

3. **Verify the MVC UI (if added)**
   - Navigate to `/Logs` and ensure data renders correctly.

4. **Troubleshoot**
   - Check the connection string used by `AddLoggingServices`.
   - Verify middleware order in the pipeline.
   - Ensure the `Logs` table exists and EF migrations run successfully.
   - Review Serilog initialization output for errors.

## Notes

- **Connection string**: Ensure the name used in `AddLoggingServices` and helper files matches your app configuration.
- **Result pattern**: If you don't use a result wrapper (e.g., `Result<T>`), return plain models and align service/controller signatures accordingly.
- **Performance**: Logging every request has overhead. Consider filtering, sampling, or async logging.
- **Security**: Protect the logs UI (authorization/role checks) to avoid exposing sensitive data.
- **PDF converter (DinkToPdf) tip**: If you see this error during migrations:
  ```bash
  Unhandled exception. System.DllNotFoundException: Unable to load DLL 'libwkhtmltox' or one of its dependencies: The specified module could not be found. (0x8007007E)
  ```
  Remove the design-time registration line `services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));`. Instead, construct `new SynchronizedConverter(new PdfTools())` only where you actually use it at runtime.

- **Git ignore caveat**: `.gitignore` often excludes log files/folders. If you intend to commit log samples for demos, explicitly include them.

By following these steps, you'll integrate Serilog into your ASP.NET Core app, persist logs in SQL Server, and optionally provide a clean UI for viewing them.
