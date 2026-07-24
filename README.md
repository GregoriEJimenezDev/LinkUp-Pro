<div align="center">
  <img src="https://img.shields.io/badge/LinkUp-Pro-2B3467?style=for-the-badge&logo=dotnet" alt="LinkUp-Pro Logo">
  <h1>LinkUp-Pro</h1>
  <p>Una red social moderna y modular construida con <b>ASP.NET Core 9</b> siguiendo los principios de <b>Clean Architecture</b>.</p>

  <p>
    <a href="https://github.com/GregoriEJimenezDev/LinkUp-Pro/network/members"><img src="https://img.shields.io/github/forks/GregoriEJimenezDev/LinkUp-Pro?style=flat-square" alt="Forks"></a>
    <a href="https://github.com/GregoriEJimenezDev/LinkUp-Pro/stargazers"><img src="https://img.shields.io/github/stars/GregoriEJimenezDev/LinkUp-Pro?style=flat-square" alt="Stars"></a>
    <a href="https://github.com/GregoriEJimenezDev/LinkUp-Pro/issues"><img src="https://img.shields.io/github/issues/GregoriEJimenezDev/LinkUp-Pro?style=flat-square" alt="Issues"></a>
    <img src="https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet" alt=".NET 9">
    <img src="https://img.shields.io/badge/PostgreSQL-316192?style=flat-square&logo=postgresql&logoColor=white" alt="Postgres">
    <img src="https://img.shields.io/badge/Docker-2496ED?style=flat-square&logo=docker&logoColor=white" alt="Docker">
  </p>
</div>

<br />

## 📖 Acerca del Proyecto

**LinkUp-Pro** es una plataforma social completa diseñada con un enfoque estricto en buenas prácticas de ingeniería de software. Utiliza un patrón de **Arquitectura Limpia (Clean Architecture)** / Arquitectura Cebolla, garantizando un acoplamiento bajo, una alta cohesión y una estructura altamente testeable y mantenible. 

Los usuarios pueden crear perfiles, conectarse mediante solicitudes de amistad, publicar en un muro (feed), reaccionar a publicaciones, comentar, e incluso jugar un minijuego de "Batalla Naval" (*Battleship*) integrado.

---

## ✨ Características Principales

- 🔐 **Sistema de Identidad Completo:** Autenticación y autorización usando ASP.NET Core Identity.
- ✉️ **Verificación por Correo (SMTP):** Confirmación de cuentas y recuperación de contraseñas de forma segura vía correo electrónico integrado con MailKit.
- 👥 **Sistema de Amistades:** Envío, aceptación y rechazo de solicitudes de amistad.
- 📰 **Feed Interactivo:** Muro de publicaciones con soporte para imágenes (validadas criptográficamente), caché en memoria para alto rendimiento, comentarios y sistema de reacciones.
- 🛡️ **Seguridad:** Borrado lógico (*soft delete*) de registros, protección contra vulnerabilidades comunes, validación de certificados TLS y subida segura de archivos (`.jpg`, `.png`, `.webp`).
- 🎮 **Minijuegos Integrados:** Subsistema de juegos incorporado (Batalla Naval interactivo) usando el patrón `UnitOfWork`.
- 🐳 **Despliegue Sencillo:** `Dockerfile` optimizado multi-etapa listo para ser desplegado en plataformas como Render o Railway.

---

## 🛠️ Tecnologías y Arquitectura

El proyecto está dividido en múltiples capas siguiendo los principios SOLID:

*   **Core / Domain (`LinkUpPro.Core.Domain`):** Entidades, Enums e interfaces base puras. Cero dependencias externas.
*   **Core / Application (`LinkUpPro.Core.Application`):** Casos de uso, DTOs, ViewModels, Servicios genéricos y reglas de negocio. (AutoMapper integrado).
*   **Infrastructure / Persistence (`LinkUpPro.Infrastructure.Persistence`):** Implementación de repositorios genéricos y UnitOfWork usando Entity Framework Core 9 con **PostgreSQL**.
*   **Infrastructure / Identity (`LinkUpPro.Infrastructure.Identity`):** Gestión de usuarios, roles y autenticación JWT.
*   **Infrastructure / Shared (`LinkUpPro.Infrastructure.Shared`):** Servicios transversales como el envío de correos (MailKit) y utilidades externas.
*   **Presentación (`LinkUpPro`):** Aplicación Web MVC con vistas Razor.

---

## 🚀 Guía de Instalación Local

### Requisitos Previos
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL](https://www.postgresql.org/) (o un contenedor de Docker ejecutando Postgres)
- Git

### 1. Clonar el repositorio
```bash
git clone https://github.com/GregoriEJimenezDev/LinkUp-Pro.git
cd LinkUp-Pro
```

### 2. Configurar Variables de Entorno (`appsettings.json`)
Dentro del proyecto de Presentación (`LinkUpPro`), debes asegurarte de tener tus credenciales en el archivo `appsettings.json` o usando User Secrets:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=LinkUpProDb;Username=tu_usuario;Password=tu_password"
  },
  "MailSettings": {
    "EmailFrom": "tu_correo@gmail.com",
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUser": "tu_correo@gmail.com",
    "SmtpPass": "tu_contraseña_de_aplicacion",
    "DisplayName": "LinkUp-Pro Soporte"
  }
}
```
*(Nota: Nunca subas contraseñas reales a GitHub. Usa variables de entorno o Secretos de Usuario para desarrollo local).*

### 3. Aplicar las Migraciones (Base de datos)
Asegúrate de estar en la raíz de la solución, y aplica las migraciones usando el CLI de Entity Framework:
```bash
dotnet ef database update -p LinkUpPro.Infrastructure.Persistence -s LinkUpPro
dotnet ef database update -p LinkUpPro.Infrastructure.Identity -s LinkUpPro -c IdentityContext
```

### 4. Compilar y Ejecutar
```bash
dotnet build
dotnet run --project LinkUpPro/LinkUpPro.csproj
```
La aplicación estará disponible en `http://localhost:5200` (o el puerto configurado).

---

## 🐳 Despliegue con Docker

El proyecto incluye un `Dockerfile` optimizado. Para construir y correr la imagen localmente:

```bash
docker build -t linkup-pro .
docker run -p 8080:8080 -e ASPNETCORE_URLS="http://+:8080" linkup-pro
```

### Despliegue en la Nube (Railway / Render)
1. Conecta tu repositorio de GitHub a tu cuenta de Railway o Render.
2. Crea una instancia de base de datos PostgreSQL.
3. Despliega la aplicación web permitiendo que la plataforma detecte automáticamente el `Dockerfile`.
4. Inyecta tus variables de entorno (como `ConnectionStrings__DefaultConnection` y `MailSettings__SmtpPass`) directamente desde el dashboard de la plataforma.

---

## 🤝 Contribución

¡Las contribuciones son bienvenidas! Sigue estos pasos para colaborar:
1. Haz un Fork del proyecto
2. Crea tu rama de característica (`git checkout -b feature/NuevaCaracteristica`)
3. Haz Commit de tus cambios (`git commit -m 'Añade una Nueva Caracteristica'`)
4. Haz Push a la rama (`git push origin feature/NuevaCaracteristica`)
5. Abre un Pull Request

---

## 📄 Licencia

Este proyecto se distribuye bajo la licencia **MIT**. Consulta el archivo `LICENSE` para más detalles.
