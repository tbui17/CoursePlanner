ARG DOTNET_VERSION=8.0
FROM mcr.microsoft.com/dotnet/sdk:$DOTNET_VERSION AS build
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1


# SSH client
RUN apt-get update && \
  apt-get install -y openssh-client && \
  rm -rf /var/lib/apt/lists/*

# Azure CLI
RUN curl -sL https://aka.ms/InstallAzureCLIDeb | bash

# MAUI Android Workload
RUN dotnet workload install maui-android

# Mono
RUN set -e && \
    apt-get update && \
    apt-get install -y apt-transport-https gnupg ca-certificates curl && \
    curl -o ./add-repository https://gist.githubusercontent.com/Ghostbird/83eb5bcd2ffd4a6b6966137a2e1c4caf && \
    sh ./add-repository "https://keyserver.ubuntu.com/pks/lookup?search=0x3fa7e0328081bff6a14da29aa6a19b38d3d831ef&op=get" "deb https://download.mono-project.com/repo/debian stable-buster main" mono-official-stable.list && \
    rm ./add-repository && \
    apt-get update && \
    apt-get install -y mono-complete nuget

# Java
ARG JAVA_VERSION=17
RUN apt-get update && apt-get install -y software-properties-common
ENV JAVA_HOME=/usr/lib/jvm/java-${JAVA_VERSION}-openjdk-amd64/

# Android SDK Manager
RUN apt-get update && apt-get install -y sdkmanager
ENV ANDROID_SDK_ROOT=/usr/lib/android-sdk

# Android toolchain
ARG ANDROID_API=34
ARG BUILD_TOOLS_VERSION=34.0.0
RUN sdkmanager "platform-tools" "build-tools;${BUILD_TOOLS_VERSION}" "platforms;android-${ANDROID_API}"

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
