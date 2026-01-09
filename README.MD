## 🔙 Backend - Sistema de Solicitudes Internas

Este módulo consiste en una **RESTful Web API** desarrollada en **ASP.NET Core** , encargada de toda la lógica de negocio y gestión de datos del Sistema de Mesa de Servicios[cite: 2].

El proyecto está construido siguiendo una **arquitectura en capas**  y cumple con los siguientes requerimientos técnicos:

### 🛠 Tecnologías y Herramientas
 **Framework:** ASP.NET Core Web API
**Base de Datos:** SQL Server implementado con **Entity Framework Core** y uso de Migrations
**Seguridad:** Autenticación y manejo de sesiones mediante **JWT (JSON Web Tokens)**

### ⚡ Funcionalidades Principales
**Gestión de Roles:** Soporte para usuarios Solicitantes, Gestores y Administradores[cite: 13, 14, 15, 16].
**Administración de Catálogos:** Endpoints para gestionar Áreas, Tipos de Solicitud y Prioridades.
* **Flujo de Solicitudes:**
    * CRUD completo de tickets.
    * Transiciones de estado (Nueva, En Proceso, Resuelta, Cerrada, Rechazada).
    * Asignación de gestores y trazabilidad mediante comentarios.
