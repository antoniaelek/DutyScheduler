# Getting started guide

### 1. Crete new postgres database. 
You can choose any database name, owner and server port you like, just don't forget to check that yur configuration settings from step 2 are correct.

### 2. Set connection string
In `appsettings.json` file, change the `ConnectionStrings` block so that it maches your connection string.

```
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5433;Username=postgres;Password=postgres;Database=DutyScheduler"
  }
```
#### :warning: Do not change the `"Postgres"` key in the specified block, because it is referenced elsewhere in code.

### 3. Run migrations
Open up powershell or cmd and `cd` into `\DutyScheduler\DutyScheduler.Service\src\DutyScheduler` directory. From there, run the following commands

```
dotnet ef migrations add <migration_name>
dotnet ef database update
```

#### :exclamation: You'll have to repeat step 3 each time your models change, in order to have those model changes propagated in the database.


### 4. Documentation
When you run the application, docs will be avaliable at [http://localhost:5000/swagger/ui/index.html](http://localhost:5000/swagger/ui/index.html).

Document your API methods with xml comments and the xml docs will be generated and displayed at the specified URL. 
