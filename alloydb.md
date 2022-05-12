To connect to AlloyDB, we're going to use a private IP and the serverless VPC connector to establish a connection between Cloud Run and the AlloyDb instance.

1. [Configure Serverless VPC Access](https://cloud.google.com/vpc/docs/configure-serverless-vpc-access#gcloud)

2. Add reserved space for AlloyDB??? https://cloud.google.com/alloydb/docs/private-ip-space-increase#gcloud

3. Create AlloyDb

4. Grant access to the SA with the `roles/alloydb.admin` role so that it can create Dbs, etc...

```bash
PROJECT_ID=`gcloud config list --format 'value(core.project)' 2>/dev/null`
SA_NAME=eventssample-sa
SERVICE_ACCOUNT="$SA_NAME@$PROJECT_ID.iam.gserviceaccount.com"
role="roles/alloydb.admin"

gcloud projects add-iam-policy-binding $PROJECT_ID \
    --member=serviceAccount:$SERVICE_ACCOUNT \
    --role=$role
```
5. Create a secret with the connection string:
```bash
ConnectionStrings__MvcMovieContext="Host=10.100.0.2;Database=movie;Username=postgres;Password=xxxx"
```

Notes:

[Using EFCore with Postgresql](https://www.npgsql.org/efcore/)

DateTime handling is Postgres is different, so you need to set a switch: `AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);` for this to work.  Fortunately there is a way to do this in csproj (out of code) as well: 
https://www.npgsql.org/doc/types/datetime.html
https://www.strathweb.com/2019/12/runtime-host-configuration-options-and-appcontext-data-in-net-core/

```xml
  <ItemGroup>
    <RuntimeHostConfigurationOption Include="Npgsql.EnableLegacyTimestampBehavior" Value="true" />
  </ItemGroup>
```