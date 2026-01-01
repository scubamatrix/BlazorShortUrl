# FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
# WORKDIR /app

# ENV ASPNETCORE_URLS http://+:5000
# RUN echo $ASPNETCORE_ENVIRONMENT

# EXPOSE 5000
# USER $APP_UID

FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG configuration=Release
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["BlazorShortUrl.csproj", "./"]
RUN dotnet restore "BlazorShortUrl.csproj"

# Copy csproj and restore as distinct layers
# COPY BlazorShortUrl.sln .
# COPY BlazorShortUrl.csproj ./
# RUN dotnet restore -r linux-x64

# Copy all the other files and build app
COPY . .
WORKDIR /src/
RUN dotnet build "BlazorShortUrl.csproj" -c $configuration -o /app/build

# Publish a release build
FROM build AS publish
ARG configuration=Release
RUN dotnet publish "BlazorShortUrl.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

# Build final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BlazorShortUrl.dll"]
