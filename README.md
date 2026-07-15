# 🛒 Mi Despensa - Inventario Personal

Este proyecto es un sistema de gestión de inventario personal para la despensa del hogar. Permite registrar productos, controlar el stock, vigilar fechas de caducidad y cuenta con un módulo de autenticación (Login) multiusuario.

## 🛠️ Tecnologías Utilizadas
*   **Backend:** ASP.NET Core API (C# .NET 10) con Entity Framework Core.
*   **Frontend:** Vue.js (Vite) + Nginx para producción.
*   **Base de Datos:** SQL Server 2022.
*   **Contenedores:** Docker y Docker Compose.
*   **Plataforma de Desarrollo:** macOS (Optimizado nativamente para chips Apple Silicon M-Series).

---

## 🚀 Requisitos Previos

Antes de comenzar, asegúrate de tener instalado en tu Mac:
1.  **Docker Desktop** (Asegúrate de tener activa la emulación Rosetta 2 en los ajustes para SQL Server).
2.  **SDK de .NET 10** (Para desarrollo local del backend).
3.  **Node.js (LTS)** (Para desarrollo local del frontend).

---

## ⚙️ Configuración Inicial (Primeros Pasos)

### 1. Clonar el proyecto y estructura
Asegúrate de tener la siguiente estructura de archivos en tu directorio de trabajo:
```text
MyEconomy/
├── docker-compose.yml
├── README.md
├── .gitignore
├── DataBase/
│   └── (Proyecto SQLserver) 
├── Backend/
│   └── (Proyecto C#)
└── frontend/
    └── (Proyecto Vue)

