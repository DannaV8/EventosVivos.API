FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props .editorconfig ./
COPY EventosVivos.Domain/EventosVivos.Domain.csproj EventosVivos.Domain/
COPY EventosVivos.Application/EventosVivos.Application.csproj EventosVivos.Application/
COPY EventosVivos.Infrastructure/EventosVivos.Infrastructure.csproj EventosVivos.Infrastructure/
COPY EventosVivos.API/EventosVivos.API.csproj EventosVivos.API/
RUN dotnet restore EventosVivos.API/EventosVivos.API.csproj

COPY . .
RUN dotnet publish EventosVivos.API/EventosVivos.API.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app .
ENTRYPOINT ["dotnet", "EventosVivos.API.dll"]
