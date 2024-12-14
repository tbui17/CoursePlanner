FROM conneqthub/dotnet-maui-android AS base
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

# Azure CLI
RUN curl -sL https://aka.ms/InstallAzureCLIDeb | bash

FROM base as final

ARG BUILD_SCRIPT_PATH="./build.ps1"
ARG COMMAND_NAME="Auth"


COPY $BUILD_SCRIPT_PATH .

# Auth
ARG KEY_URI
ENV KeyUri=$KEY_URI



RUN --mount=type=secret,id=TENANT_ID \
    --mount=type=secret,id=SERVICE_PRINCIPAL_ID \
    --mount=type=secret,id=SERVICE_PRINCIPAL_SECRET \
    pwsh $BUILD_SCRIPT_PATH $COMMAND_NAME


# Build
ARG ProjectName=Scripts
ARG BUILD_CONFIGURATION=Debug
WORKDIR /app
COPY . .
WORKDIR "/app/${ProjectName}"
RUN dotnet build "${ProjectName}.csproj" -c $BUILD_CONFIGURATION
WORKDIR ../
ENTRYPOINT ["pwsh","./build.ps1", "StartApp","publish_upload"]