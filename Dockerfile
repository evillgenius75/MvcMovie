# Build the API
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS api-build

WORKDIR /src
COPY . .
RUN dotnet publish \
    -r linux-x64 --self-contained true -p:PublishSingleFile=true \
    -c Release -o /publish

# Combine both into the final container
FROM mcr.microsoft.com/dotnet/runtime-deps:6.0 as runtime
WORKDIR /app
COPY --from=api-build /publish .

# Use the default port for Cloud Run
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

ENTRYPOINT ["./MvcMovie"]