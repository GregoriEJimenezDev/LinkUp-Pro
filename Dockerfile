# Etapa base para ejecutar la aplicación
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar el archivo de la solución y los proyectos para restaurar dependencias
COPY ["LinkUpPro.slnx", "./"]
COPY ["LinkUpPro/LinkUpPro.csproj", "LinkUpPro/"]
COPY ["LinkUpPro.Core.Application/LinkUpPro.Core.Application.csproj", "LinkUpPro.Core.Application/"]
COPY ["LinkUpPro.Core.Domain/LinkUpPro.Core.Domain.csproj", "LinkUpPro.Core.Domain/"]
COPY ["LinkUpPro.Infrastructure.Identity/LinkUpPro.Infrastructure.Identity.csproj", "LinkUpPro.Infrastructure.Identity/"]
COPY ["LinkUpPro.Infrastructure.Persistence/LinkUpPro.Infrastructure.Persistence.csproj", "LinkUpPro.Infrastructure.Persistence/"]
COPY ["LinkUpPro.Infrastructure.Shared/LinkUpPro.Infrastructure.Shared.csproj", "LinkUpPro.Infrastructure.Shared/"]

# Restaurar todas las dependencias
RUN dotnet restore "LinkUpPro.slnx"

# Copiar todo el código fuente
COPY . .

# Compilar la aplicación
WORKDIR "/src/LinkUpPro"
RUN dotnet build "LinkUpPro.csproj" -c Release -o /app/build

# Publicar la aplicación optimizada
FROM build AS publish
RUN dotnet publish "LinkUpPro.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa final: construir la imagen de producción
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Configurar que ASP.NET Core escuche en el puerto 8080 (requerido por Railway/Render)
ENV ASPNETCORE_URLS=http://+:8080

# Iniciar la aplicación
ENTRYPOINT ["dotnet", "LinkUpPro.dll"]
