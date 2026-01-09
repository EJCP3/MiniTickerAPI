## 🔙 Backend - Sistema de Solicitudes Internas

[cite_start]Este módulo consiste en una **RESTful Web API** desarrollada en **ASP.NET Core** [cite: 56, 58][cite_start], encargada de toda la lógica de negocio y gestión de datos del Sistema de Mesa de Servicios[cite: 2].

[cite_start]El proyecto está construido siguiendo una **arquitectura en capas** [cite: 61] y cumple con los siguientes requerimientos técnicos:

### 🛠 Tecnologías y Herramientas
* [cite_start]**Framework:** ASP.NET Core Web API[cite: 56].
* [cite_start]**Base de Datos:** SQL Server implementado con **Entity Framework Core** y uso de Migrations[cite: 59].
* [cite_start]**Seguridad:** Autenticación y manejo de sesiones mediante **JWT (JSON Web Tokens)**[cite: 60].

### ⚡ Funcionalidades Principales
* [cite_start]**Gestión de Roles:** Soporte para usuarios Solicitantes, Gestores y Administradores[cite: 13, 14, 15, 16].
* [cite_start]**Administración de Catálogos:** Endpoints para gestionar Áreas, Tipos de Solicitud y Prioridades[cite: 66].
* **Flujo de Solicitudes:**
    * [cite_start]CRUD completo de tickets[cite: 67].
    * [cite_start]Transiciones de estado (Nueva, En Proceso, Resuelta, Cerrada, Rechazada)[cite: 32, 67].
    * [cite_start]Asignación de gestores y trazabilidad mediante comentarios[cite: 67].
* [cite_start]**Manejo de Errores:** Respuestas HTTP estandarizadas y validaciones (400, 401, 403, 404)[cite: 62].