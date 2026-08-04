# Trabajo Práctico Integrador
## Desarrollo de Software 2026

### Universidad Tecnológica Nacional
**Facultad Regional Tucumán**

---

## Integrantes

| Nombre | Legajo | Correo electrónico |
|---------|:------:|--------------------|
| Carlos Iván Chirino | 60388 | civanchirino@gmail.com |
| Juana Vicente | 60599 | juanivicentee@gmail.com |
| Rocío Marianela López | 60913 | alumrlopez@gmail.com |

---

## Descripción del proyecto

Sistema de Turnos Médicos desarrollado como Trabajo Práctico Integrador de la asignatura Desarrollo de Software.

El backend consiste en una API RESTful desarrollada con ASP.NET Core que permite gestionar médicos, especialidades, disponibilidades horarias, pacientes y citas médicas.

El sistema implementa autenticación y autorización mediante JWT, diferenciando los roles de **ADMINISTRADOR** y **PACIENTE**.

---

## Tecnologías utilizadas

- C#
- ASP.NET Core
- .NET 10
- Entity Framework Core
- SQL Server / LocalDB
- ASP.NET Core Identity
- JWT (JSON Web Token)
- Swagger / OpenAPI
- Serilog
- ASP.NET Core Rate Limiting
- xUnit

---

## Arquitectura

El proyecto utiliza una arquitectura en capas con separación de responsabilidades:

- **Dsw2026Tpi.Api**: exposición de endpoints, configuración, middlewares y controladores.
- **Dsw2026Tpi.Application**: servicios, DTOs, interfaces y lógica de aplicación.
- **Dsw2026Tpi.Domain**: entidades, enumeraciones y reglas propias del dominio.
- **Dsw2026Tpi.Data**: persistencia de datos, repositorios, Entity Framework Core e Identity.
- **Dsw2026Tpi.CrossCutting**: elementos transversales compartidos por las distintas capas.
- **Dsw2026Tpi.Tests**: pruebas unitarias.

---

## Requisitos previos

Para ejecutar el proyecto localmente se requiere:

- .NET 10 SDK
- SQL Server LocalDB o una instancia compatible de SQL Server
- Visual Studio 2022 o superior, o un IDE compatible con .NET
- Entity Framework Core CLI, en caso de ejecutar migraciones desde consola

Para instalar la herramienta de Entity Framework Core:

```bash
dotnet tool install --global dotnet-ef
```

Si ya se encuentra instalada:

```bash
dotnet tool update --global dotnet-ef
```

---

## Configuración de la base de datos

La cadena de conexión utilizada en el entorno de desarrollo se encuentra en:

`Dsw2026Tpi.Api/appsettings.Development.json`

Configuración por defecto:

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=(localdb)\\MSSQLLocalDB;Database=Dsw2026Tpi;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True"
}
```

En caso de utilizar otra instancia de SQL Server, modificar `DefaultConnection` según corresponda.

---

## Configuración del administrador inicial

Al iniciar el sistema por primera vez se crea automáticamente un usuario con rol **ADMINISTRADOR**.

El correo del administrador se configura mediante:

```json
"AdminSeed": {
  "Email": "desarrollo@test.com"
}
```

Por razones de seguridad, la contraseña del administrador **no se almacena en el repositorio** y debe configurarse mediante **User Secrets**.

Desde la raíz del proyecto ejecutar:

```bash
dotnet user-secrets set "AdminSeed:Password" "ContraseñaSegura123" --project Dsw2026Tpi.Api
```

La contraseña debe cumplir las políticas configuradas por el sistema:

- Mínimo 8 caracteres.
- Al menos una letra mayúscula.
- Al menos una letra minúscula.
- Al menos un número.

> **Importante:** si el administrador ya existe en la base de datos, modificar el valor de `AdminSeed:Email` no reemplaza ni modifica automáticamente al administrador existente. El proceso de inicialización crea el usuario configurado únicamente cuando este no existe.

---

## Migraciones de Entity Framework Core

El proyecto utiliza dos contextos de Entity Framework Core:

- `Dsw2026TpiDbContext`: información correspondiente al dominio de la aplicación.
- `AuthenticationDbContext`: información correspondiente a autenticación, usuarios y roles.

Antes de ejecutar el proyecto por primera vez, aplicar las migraciones de ambos contextos.

### Base de datos del dominio

```bash
dotnet ef database update --context Dsw2026TpiDbContext --project Dsw2026Tpi.Data --startup-project Dsw2026Tpi.Api
```

### Base de datos de autenticación

```bash
dotnet ef database update --context AuthenticationDbContext --project Dsw2026Tpi.Data --startup-project Dsw2026Tpi.Api
```

Ambos contextos utilizan la misma cadena de conexión.

---

## Ejecución del proyecto

Desde la raíz de la solución:

```bash
dotnet restore
```

Luego:

```bash
dotnet build
```

Y finalmente:

```bash
dotnet run --project Dsw2026Tpi.Api
```

También puede ejecutarse directamente desde Visual Studio seleccionando `Dsw2026Tpi.Api` como proyecto de inicio.

---

## Swagger

En el entorno de desarrollo, la API expone Swagger para visualizar y probar los endpoints implementados.

Una vez iniciada la aplicación, acceder a la URL de Swagger indicada por la consola o por Visual Studio.

Los endpoints se encuentran agrupados por módulo para facilitar su utilización.

Para los endpoints protegidos se debe obtener previamente un token JWT mediante alguno de los endpoints de autenticación y enviarlo utilizando el esquema:

```text
Bearer <token>
```

---

## Autenticación y autorización

El sistema utiliza JWT para autenticar las solicitudes.

Existen dos roles:

- `ADMINISTRADOR`
- `PACIENTE`

Los únicos endpoints que no requieren autenticación son los correspondientes al inicio de sesión.

### Administrador

```http
POST /api/auth/admin/login
```

Recibe email y contraseña y retorna un token JWT con rol `ADMINISTRADOR`.

### Paciente

```http
POST /api/auth/patient/login
```

Recibe email y DNI.

Si el paciente inicia sesión por primera vez, el sistema realiza su registración automáticamente y le asigna el rol `PACIENTE`.

---

## Endpoints principales

### Autenticación

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/auth/admin/login` | Autenticación de administrador |
| POST | `/api/auth/patient/login` | Autenticación de paciente |

### Especialidades

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/specialties` | Lista y filtra especialidades |
| GET | `/api/specialties/{id}` | Obtiene una especialidad |
| POST | `/api/specialties` | Crea una especialidad |
| PUT | `/api/specialties/{id}` | Actualiza una especialidad |
| DELETE | `/api/specialties/{id}` | Realiza la eliminación lógica de una especialidad |

### Médicos

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/doctors` | Lista y filtra médicos |
| GET | `/api/doctors/{id}` | Obtiene un médico |
| GET | `/api/doctors/{id}/availabilities` | Obtiene las disponibilidades de un médico |
| POST | `/api/doctors` | Crea un médico |
| PUT | `/api/doctors/{id}` | Actualiza un médico |
| DELETE | `/api/doctors/{id}` | Realiza la eliminación lógica de un médico |

### Disponibilidades

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/availabilities` | Registra disponibilidades de un médico |
| PUT | `/api/availabilities` | Actualiza las disponibilidades mensuales |

Las disponibilidades generan intervalos de atención de **30 minutos** y contemplan las validaciones correspondientes a horarios, solapamientos y días no laborables.

### Citas

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/appointments` | Reserva una cita |
| GET | `/api/appointments` | Obtiene citas por fecha |
| GET | `/api/appointments/search` | Realiza búsquedas avanzadas y paginadas |
| GET | `/api/appointments/patient` | Obtiene las citas activas de un paciente |
| DELETE | `/api/appointments/{id}` | Cancela una cita |

---

## Paginación y filtros

Los endpoints de consulta que lo requieren admiten parámetros mediante query string.

Ejemplo:

```http
GET /api/doctors?pageSize=10&pageIndex=1&name=Juan
```

La búsqueda avanzada de citas permite combinar distintos criterios:

```http
GET /api/appointments/search?pageSize=10&pageIndex=1&specialtyId={id}&doctorId={id}&dni={dni}&date=2026-08-04
```

Los parámetros opcionales pueden omitirse cuando no se desea aplicar el filtro correspondiente.

---

## Eliminación lógica

Las operaciones `DELETE` correspondientes a entidades administrables utilizan **soft delete**.

Esto significa que los registros no se eliminan físicamente de la base de datos, sino que se marcan como eliminados y dejan de aparecer en las consultas correspondientes.

---

## Manejo de errores

La API implementa un middleware global para centralizar el manejo de excepciones y mantener un formato consistente de respuesta.

Formato general:

```json
{
  "errorCode": "ERROR_CODE",
  "message": "Descripción del error",
  "details": [
    {
      "field": "campo",
      "issue": "detalle"
    }
  ]
}
```

El campo `details` se incluye cuando corresponde.

Las validaciones de entrada también utilizan el formato general de errores de la aplicación.

---

## Rate Limiting

La API implementa políticas de limitación de solicitudes mediante ASP.NET Core Rate Limiting.

Políticas configuradas:

| Operación | Límite |
|-----------|--------|
| Login administrador | 5 solicitudes por minuto |
| Login paciente | 10 solicitudes por minuto |
| Reserva de citas | 5 solicitudes por minuto |
| Política general | 100 solicitudes por minuto |

Cuando se supera un límite, la API retorna HTTP `429 Too Many Requests`.

Los valores se encuentran configurados externamente en `appsettings.json`.

---

## Logging

El sistema utiliza **Serilog** para registrar la ejecución de las funcionalidades correspondientes.

Los logs se generan:

- En consola.
- En archivos dentro del directorio `Logs/`.

Los archivos poseen rotación diaria y una política de retención configurada.

---

## Health Check

La aplicación dispone del siguiente endpoint para verificar su disponibilidad:

```http
GET /health-check
```

---

## Pruebas

El proyecto incluye un proyecto independiente:

```text
Dsw2026Tpi.Tests
```

Para ejecutar las pruebas:

```bash
dotnet test
```

---

## Consideraciones importantes

- Los identificadores de las entidades utilizan `GUID`.
- Los turnos se generan en intervalos de 30 minutos.
- No se permiten reservas de turnos en fechas u horarios pasados.
- El sistema evita la doble reserva de un mismo turno.
- Las disponibilidades no generan turnos para los días definidos como feriados o no laborables.
- Las operaciones administrativas están protegidas por el rol `ADMINISTRADOR`.
- Las operaciones correspondientes al paciente están protegidas por el rol `PACIENTE`.
- Las contraseñas se administran mediante ASP.NET Core Identity y no se almacenan en texto plano.
- Las credenciales sensibles no deben incorporarse al repositorio.
- La API utiliza DTOs para separar los modelos de entrada y salida de las entidades persistidas.
- La aplicación implementa validaciones de entrada, manejo global de errores, logging y rate limiting.

---

## Estado del proyecto

Backend correspondiente al Trabajo Práctico Integrador de Desarrollo de Software 2026.
