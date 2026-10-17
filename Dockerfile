# syntax=docker/dockerfile:1
# Build context = solution root (the folder that contains DotNet.sln)
#
# Base images are pinned through build args. For production builds pass an exact digest, e.g.
#   --build-arg SDK_IMAGE=mcr.microsoft.com/dotnet/sdk:8.0@sha256:[SDK_DIGEST]
#   --build-arg ASPNET_IMAGE=mcr.microsoft.com/dotnet/aspnet:8.0@sha256:[ASPNET_DIGEST]
# Get digests with: docker buildx imagetools inspect mcr.microsoft.com/dotnet/aspnet:8.0
# (the CI workflow reads them from the SDK_IMAGE / ASPNET_IMAGE repository variables)
ARG SDK_IMAGE=mcr.microsoft.com/dotnet/sdk:8.0
ARG ASPNET_IMAGE=mcr.microsoft.com/dotnet/aspnet:8.0

# ---------- Build stage ----------
FROM ${SDK_IMAGE} AS build
WORKDIR /src

# Copy only the .csproj files first so "dotnet restore" is cached until dependencies change
COPY ["WebApi/WebApi.csproj",     "WebApi/"]
COPY ["Bll/Bll.csproj",           "Bll/"]
COPY ["Dll/Dll.csproj",           "Dll/"]
COPY ["Utility/Utility.csproj",   "Utility/"]
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore "WebApi/WebApi.csproj"

# Copy the rest of the source.
COPY . .

# Safety net: remove Visual Studio's obj/bin folders if they were copied in. They contain Windows paths
# (e.g. a "C:\Program Files (x86)\Microsoft Visual Studio\..." NuGet fallback folder) that break the Linux build.
RUN find . -type d \( -name obj -o -name bin \) -prune -exec rm -rf {} +

# Restore again on the clean tree (fast: packages are in the cache mount), then publish
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore "WebApi/WebApi.csproj" \
 && dotnet publish "WebApi/WebApi.csproj" \
        -c Release \
        --no-restore \
        -o /app/publish \
        /p:UseAppHost=false \
        /p:DebugType=none

# ---------- Runtime stage ----------
# Debian-based image (not Alpine): SQL Server client + ICU globalization work without extra setup
FROM ${ASPNET_IMAGE} AS final

ARG VERSION=dev
ARG REVISION=unknown
ARG CREATED=unknown
LABEL org.opencontainers.image.title="admin-panel-webapi" \
      org.opencontainers.image.version="${VERSION}" \
      org.opencontainers.image.revision="${REVISION}" \
      org.opencontainers.image.created="${CREATED}"

WORKDIR /app

# Writable folders for logs and the data-protection key ring (mount volumes here)
RUN mkdir -p /app/logs /app/keys \
 && chown -R $APP_UID:$APP_UID /app/logs /app/keys \
 && chmod 700 /app/keys

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_EnableDiagnostics=0 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Run as the non-root "app" user that ships with the .NET 8 images
USER $APP_UID

EXPOSE 8080

# Liveness probe: the image has no curl, so use bash's built-in /dev/tcp.
# (/health/live only checks the process; /health/ready also checks the database and is for load balancers.)
HEALTHCHECK --interval=30s --timeout=5s --start-period=40s --retries=3 \
  CMD ["bash", "-c", "exec 3<>/dev/tcp/127.0.0.1/8080 && printf 'GET /health/live HTTP/1.0\\r\\nHost: localhost\\r\\n\\r\\n' >&3 && head -n1 <&3 | grep -q ' 200 '"]

ENTRYPOINT ["dotnet", "WebApi.dll"]
