FROM conneqthub/dotnet-maui-android AS base
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

# Azure CLI
RUN curl -sL https://aka.ms/InstallAzureCLIDeb | bash

FROM base as final

# Auth
ARG KEY_URI
ARG USERNAME
ARG PASSWORD
ARG TENANT
ENV KeyUri=$KEY_URI
RUN az login --service-principal --username "$USERNAME" --password "$PASSWORD" --tenant "$TENANT"

# Build
ARG ProjectName=Scripts
ARG BUILD_CONFIGURATION=Debug
WORKDIR /app
COPY . .
WORKDIR "/app/${ProjectName}"
RUN dotnet build "${ProjectName}.csproj" -c $BUILD_CONFIGURATION
ENTRYPOINT ["dotnet", "run", "--verbosity", "normal", "--project", "Scripts.csproj", "--", "publish_upload"]