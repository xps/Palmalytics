# Palmalytics

Palmalytics is a self-hosted, first-party, server-side web analytics dashboard for ASP.NET Core applications. It can replace standard client-side web analytics tools like Google Analytics and Matomo. It tracks pageviews, sessions, referrals, locations, user agents, and more.


## Why Server-Side Analytics?

Unlike client-side analytics, server-side analytics offers the following advantages:

- **High Performance**: Minimal impact on the server side as data is saved asynchronously, and zero impact on the client side.
- **User-Friendly**: No cookies, no annoying consent popups.
- **Accuracy**: Not blocked by adblockers and privacy-focused browsers and extensions.
- **Data Ownership**: You own your data, and it doesn’t leave your server.

The data can be stored in the same database your application already uses (e.g. SQL Server), which means you can also easily query it if you'd like.


## Getting Started

1. Add the [storage-specific NuGet package][0] from the Package Manager Console:

    ```shell
    PM> Install-Package Palmalytics.SqlServer -Pre
    ```

2. Register services in your `Startup.cs` and configure the SQL Server storage:

    ```csharp
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddPalmalytics(options => {
            options.UseSqlServer(new SqlServerOptions
            {
                ConnectionString = Configuration.GetConnectionString("PalmalyticsConnection"),
                Schema = "Palmalytics"
            });
        });

        // ...other services
    }
    ```

3. Register the middleware:

    ```csharp
    public void Configure(IApplicationBuilder app)
    {
        app.UsePalmalytics();

        // ...other middleware
    }
    ```

4. Navigate to `https://<your-app>/palmalytics` to see your dashboard.

<img src="Assets/Screenshot.webp" alt="Screenshot of the Palmalytics dashboard" />

## Dashboard Authorization

By default, the dashboard is only accessible by local requests. In production, you will probably want to change this. Here are some options:

### Public access

If you want to allow public access to your dashboard, just remove the default authorization rule:

```csharp
services.AddPalmalytics(options =>
{
    options.DashboardOptions.Authorize = null;
    // ... other options
}
```

### Restricted access

You can implement your own authorization rule, for example based on ASP.NET user roles:

```csharp
services.AddPalmalytics(options =>
{
    options.DashboardOptions.Authorize = (context) =>
    {
        return context.User.IsInRole("Admin");
    };

    // ... other options
}
```


## Requirements

Currently, Palmalytics requires:

- .NET 8
- SQL Server for storage

We may add support for other data stores (Postgres, SQLite…).


## Change log

### Version 1.0.0-rc (8 Jul 2026)

 - Now targets .NET 8
 - Updated dependencies
 - Bug fixes
 - Performance improvements

### Version 0.3.0-alpha (5 Jul 2026)

 - Automatically decode URLs
 - Updated Dapper
 - Updated public suffix list
 - Added detection of Brave browser

### Version 0.2.0-alpha (12 Oct 2024)

 - Added UTM parameters stats to dashboard
 - Added 'not set' values in referrer stats

### Version 0.1.0-alpha (25 Aug 2024)

 - Initial version


## Licensing

Copyright 2026 Xavier Poinas

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.


## Contributing

Contributions are welcome, whether they are bug reports, feature requests, or code contributions.

By submitting a pull request, you relinquish any rights or claims to the changes you submit to the project and transfer the copyright of those changes to the project owner.

[0]: https://www.nuget.org/packages?q=palmalytics