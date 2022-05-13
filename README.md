## Overview

Based off the [ASP.NET Core MVC Tutorial](https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-mvc-app&tabs=visual-studio-code) this demo will deploy a simple ASP.NET 6 application to Google Cloud Run that uses [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) to create, seed, read and write to Google Cloud AlloyDB a fully managed PostgreSQL database service.

## Technologies Demonstrated
* Google Cloud Run
* Google Cloud Build
* Google Secret Manager 
* [Google AlloyDB for PostgreSQL](https://cloud.google.com/alloydb)
* [EFCore with PostgreSQL](https://www.npgsql.org/efcore/)
* ASP.NET 6 with Minimal API

## Run the app locally
To test this out locally the application will use Sqlite as a file based database that you do not need to install.  You can see how this is defined in `Program.cs`:

```csharp
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<MvcMovieContext>(options =>
        options.UseSqlite(connectionString));
}
```

To run locally, execute this command which sets the environment to Development and launchs the app to listen on port 5000:
```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run --urls http://+:5000
```

 Now launch a browser to http://localhost:5000/Movies and you should be able to browse, edit and add Movies to the local database.

### Model First Deployment
This sample leverages the model first deployment pattern for EF.  Using `MvcMovieContext.cs`  EF will automatically create the database schema for the target database.  The `SeedData.cs` class was manually created to populate the sample database on first run. 

The `./Migrations` folder already contains auto generated code that was built using the following steps.  *You do not need to run this* unless you want to change the underlying data model. 

```bash
# Install the EF tool.
dotnet tool install --global dotnet-ef

# Set DOTNET_ROOT to the installation location of .NET
export DOTNET_ROOT=$HOME/.dotnet
# And add the tools directory to your path as well
export PATH=$DOTNET_ROOT:$DOTNET_ROOT/tools:$PATH

# Create the ./Migrations auto generated code
dotnet ef migrations add InitialCreate

# [optional] This will generate the database, but if you just run 
# the code, it will do this automatically on first run as well.
#dotnet ef database update
```
## Deploy the cloud services

    **Coming Soon**: a script to automate these deployment steps.

If you have a fresh new project (recommended) you will need to enable the following APIs.  This script will automate:

```bash
# Enable required cloud service apis
echo 'Enabling required cloud apis...'
declare -a apis=("compute.googleapis.com"
    "cloudresourcemanager.googleapis.com"
    "cloudbuild.googleapis.com"
    "artifactregistry.googleapis.com"
    "secretmanager.googleapis.com"
    "run.googleapis.com"
    "alloydb.googleapis.com"
    )
for api in "${apis[@]}"
do
gcloud services enable $api
done
```
## Deploy AlloyDB
At the time of this writing [AlloyDB](https://cloud.google.com/alloydb/docs/overview) is a preview release.  Network connectivity is based on Private IP only, not a bad thing since most enterprises demand this.  Start [here](https://cloud.google.com/alloydb/docs/project-enable-access) to enable AlloyDB.

### Increase Private IP Space
Initially I skipped over the [Configure connectivity](https://cloud.google.com/alloydb/docs/configure-connectivity) section, but you may encounter an error when deploying if you did not [add reserved space for AlloyDB](https://cloud.google.com/alloydb/docs/private-ip-space-increase#gcloud)

### Create a secret for the password
Take note of the private IP and your password for the `postgres` user and [create a secret](https://cloud.google.com/secret-manager/docs/creating-and-accessing-secrets#create) using Google Secret Manager.  Name the secret `ConnectionStrings__MvcMovieContext`.

```bash
echo "Host=X.X.X.X;Database=movie;Username=postgres;Password=XXXXX" | gcloud secrets create ConnectionStrings__MvcMovieContext --data-file=-
```

## Setup Cloud Run

### Create a Service Account
If you use the UI to deploy Cloud Run the Service Account (SA) will automatically get created for you, however by scripting you can easily add the additional roles we need to access secrets and AlloyDB.

```bash
#
# Create a service account that Cloud Run will execute as and assign required permissions
#
PROJECT_ID=`gcloud config list --format 'value(core.project)' 2>/dev/null`
SA_NAME=movie-sa
SERVICE_ACCOUNT="$SA_NAME@$PROJECT_ID.iam.gserviceaccount.com"

echo 'Creating a service account to run the app...'
gcloud iam service-accounts create $SA_NAME

echo 'Assigning roles to the service account...'
declare -a sa_roles=("roles/iam.serviceAccountUser"
    "roles/run.admin"
    "roles/secretmanager.secretAccessor"
    "roles/alloydb.admin"
    )
for role in "${sa_roles[@]}"
do
gcloud projects add-iam-policy-binding $PROJECT_ID \
    --member=serviceAccount:$SERVICE_ACCOUNT \
    --role=$role
done
```

### Serverless VPC Access
In order for Cloud Run apps to access AlloyDB via a private IP we need to [Configure Serverless VPC Access](https://cloud.google.com/vpc/docs/configure-serverless-vpc-access#gcloud).  Create a VPC Connector named `vpc-connector`.

### Deploy the app to Cloud Run
The last step is to deploy the application using the following command which will use Cloud Build to build the application in a small, optimized container and deploy to Cloud Run with access to the secret containting the connection string and private vpc network access.

```bash
PROJECT_ID=`gcloud config list --format 'value(core.project)' 2>/dev/null`
SA_NAME=movie-sa
SERVICE_ACCOUNT="$SA_NAME@$PROJECT_ID.iam.gserviceaccount.com"
VPC_CONNECTOR=vpc-connector
CONNECTION_SECRET="ConnectionStrings__MvcMovieContext"

gcloud beta run deploy \
--region us-central1 \
--platform managed \
--allow-unauthenticated \
--set-secrets="$CONNECTION_SECRET=$CONNECTION_SECRET:latest" \
--service-account="$SERVICE_ACCOUNT" \
--vpc-connector $VPC_CONNECTOR \
--source . \
movie
```

## DateTime in PostgreSQL
The application uses [EFCore with Postgresql](https://www.npgsql.org/efcore/) as AlloyDB is a PostgreSQL 14 compatible database.  [DateTime handling is Postgres is different](https://www.npgsql.org/doc/types/datetime.html), so you need to set a switch: `AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);` for this to work properly.  This sample uses an alternative way of setting this in the csproj instead: 

```xml
  <ItemGroup>
    <RuntimeHostConfigurationOption Include="Npgsql.EnableLegacyTimestampBehavior" Value="true" />
  </ItemGroup>
```
