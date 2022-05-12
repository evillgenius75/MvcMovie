To connect to AlloyDB, we're going to use a private IP and the serverless VPC connector to establish a connection between Cloud Run and the AlloyDb instance.

1. [Configure Serverless VPC Access](https://cloud.google.com/vpc/docs/configure-serverless-vpc-access#gcloud)

clusterid:  jasondel-alloydb
password:   /nKOd?47Sb)p6SC7

2. Add reserved space for AlloyDB??? https://cloud.google.com/alloydb/docs/private-ip-space-increase#gcloud

3. Create AlloyDb

4. 

DateTime handling is Postgres is different, so you need to set a switch: `AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);` for this to work.  Fortunately there is a way to do this in configruation as well: 
https://www.npgsql.org/doc/types/datetime.html
https://www.strathweb.com/2019/12/runtime-host-configuration-options-and-appcontext-data-in-net-core/