# Piedrazul — Backend

> Sistema de gestión de citas médicas y terapéuticas para clínica de salud natural.
> Backend RESTful construido con **.NET 10**, siguiendo **Clean Architecture** y el patrón **CQRS** con MediatR

---

## Tabla de contenidos

- [Descripción general](#descripción-general)
- [Tecnologías](#tecnologías)
- [Arquitectura](#arquitectura)
- [Patrones de diseño](#patrones-de-diseño)
- [Estructura de carpetas](#estructura-de-carpetas)
- [Modelo de dominio](#modelo-de-dominio)
- [API — Endpoints](#api--endpoints)
- [Autenticación y autorización](#autenticación-y-autorización)
- [Configuración](#configuración)
- [Ejecución local](#ejecución-local)
- [Tests](#tests)
- [Decisiones de diseño](#decisiones-de-diseño)

---

## Descripción general

**Piedrazul** es el backend de un sistema de agendamiento de citas para una clínica que ofrece servicios de **Neuroterapia, Quiropráctica y Fisioterapia**. El sistema permite:

- Registro e inicio de sesión de usuarios con roles diferenciados.
- Gestión de médicos/terapeutas y sus horarios de atención.
- Agendamiento de citas por parte de agendadores o por los propios pacientes.
- Consulta de franjas horarias disponibles según la disponibilidad del profesional.
- Cancelación, reprogramación y cierre de citas.
- Registro de auditoría de las acciones sobre citas.
- Configuración del ventana de agendamiento (semanas hacia adelante habilitadas).

---

## Tecnologías

| Capa | Tecnología / Librería | Versión |
|---|---|---|
| Runtime | .NET | 10.0 |
| Lenguaje | C# (nullable enabled) | 13 |
| ORM | Entity Framework Core | 10.0.3 |
| Base de datos | PostgreSQL (Npgsql) | 10.0.0 |
| CQRS / Mediator | MediatR | 14.1.0 |
| Validación | FluentValidation | 12.1.1 |
| Autenticación | JWT Bearer (AspNetCore) | 10.0.3 |
| Hash de contraseñas | BCrypt.Net-Next | 4.1.0 |
| Documentación API | Swashbuckle / Swagger | 10.1.4 |
| Tests | xUnit + Moq + FluentAssertions | — |

---

## Arquitectura

El proyecto sigue los principios de **Clean Architecture** (Arquitectura Limpia), organizando el código en capas concéntricas donde las dependencias siempre apuntan hacia el interior (hacia el dominio).

```
┌──────────────────────────────────────────────────────────────┐
│                        Piedrazul.API                         │  ← Capa de presentación
│         Controllers · Middleware · Program.cs                │
├──────────────────────────────────────────────────────────────┤
│                    Piedrazul.Application                     │  ← Casos de uso
│       Commands · Queries · Handlers · Validators             │
│       Interfaces de servicios (IJwtService, IAuditService…)  │
├──────────────────────────────────────────────────────────────┤
│                    Piedrazul.Infrastructure                  │  ← Adaptadores / detalles
│    EF Core DbContext · Repositories · JwtService · Migrations│
├──────────────────────────────────────────────────────────────┤
│                      Piedrazul.Domain                        │  ← Núcleo del negocio
│         Entities · Enums · Interfaces · Exceptions           │
└──────────────────────────────────────────────────────────────┘
                    Piedrazul.Tests  (transversal)
```

### Flujo de una solicitud HTTP

```
HTTP Request
    │
    ▼
[Controller]
    │  Crea Command/Query
    ▼
[MediatR Pipeline]
    │  ValidationBehavior (FluentValidation)
    ▼
[Handler]
    │  Lógica de aplicación
    │  Interactúa con Repositorios / Servicios
    ▼
[Domain Entities]
    │  Lógica de negocio pura
    ▼
[Repository / EF Core]
    │
    ▼
[PostgreSQL]
```

---

## Patrones de diseño

### CQRS (Command Query Responsibility Segregation)

Todas las operaciones se expresan como **Commands** (mutaciones) o **Queries** (lecturas), procesadas por sus respectivos **Handlers** a través de MediatR.

```
Commands (escritura)                 Queries (lectura)
──────────────────                   ─────────────────
CreateAppointmentCommand             GetAvailableSlotsQuery
BookAppointmentCommand               GetAppointmentsByDateQuery
CreateDoctorCommand                  GetAppointmentsByDoctorAndDateQuery
UpdateDoctorCommand                  GetSchedulingConfigQuery
CancelAppointmentCommand
CompleteAppointmentCommand
RescheduleAppointmentCommand
RegisterCommand
AddDoctorScheduleCommand
RemoveDoctorScheduleCommand
```

### Repository Pattern

Las interfaces de repositorios se definen en `Domain`, sus implementaciones viven en `Infrastructure`. Esto permite invertir la dependencia y testear handlers sin base de datos real.

```
IDoctorRepository         ←→  DoctorRepository (EF Core)
IAppointmentRepository    ←→  AppointmentRepository
IPatientRepository        ←→  PatientRepository
IUserRepository           ←→  UserRepository
ISchedulingSettingsRepository ←→ SchedulingSettingsRepository
```

### Unit of Work

Implementado para garantizar atomicidad en operaciones que afectan múltiples entidades (ej.: creación simultánea de `User` + `Patient`). Soporta `BeginTransaction`, `Commit` y `Rollback`.

### Pipeline Behavior (Decorator sobre MediatR)

El `ValidationBehavior<TRequest, TResponse>` intercepta **todos** los requests antes de llegar al handler, ejecuta los validadores de FluentValidation asociados y lanza `ValidationException` si hay errores. No es necesario validar manualmente en cada handler.

### Domain-Driven Design (táctico)

- **Entidades ricas:** `Doctor.GetAvailableSlots()`, `Doctor.IsValidSlot()`, `Appointment.Reschedule()`.
- **Factory methods estáticos:** `User.Create(...)`, `Patient.Create(...)`, `Appointment.Create(...)`.
- **Excepciones de dominio semánticas:** `SlotNotAvailableException`, `DuplicateAppointmentException`.

### Middleware de manejo global de excepciones

`ExceptionHandlingMiddleware` captura todas las excepciones no controladas y las traduce a respuestas HTTP con código y mensaje apropiados, sin que los controllers necesiten try/catch.

| Excepción de dominio | HTTP Status |
|---|---|
| `ValidationException` | 400 Bad Request |
| `SlotNotAvailableException` | 409 Conflict |
| `DuplicateAppointmentException` | 409 Conflict |
| `InvalidOperationException` | 400 Bad Request |
| `KeyNotFoundException` | 404 Not Found |
| `UnauthorizedAccessException` | 401 Unauthorized |
| Cualquier otra | 500 Internal Server Error |

---

## Estructura de carpetas

```
Piedrazul/
│
├── Piedrazul.Domain/
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── Patient.cs
│   │   ├── Doctor.cs
│   │   ├── Appointment.cs
│   │   ├── DoctorSchedule.cs
│   │   ├── AuditLog.cs
│   │   └── SchedulingSettings.cs
│   ├── Enums/
│   │   ├── UserRole.cs           (Admin, Scheduler, Doctor, Patient)
│   │   ├── AppointmentStatus.cs  (Scheduled, Completed, Cancelled, Rescheduled)
│   │   ├── DoctorType.cs         (Doctor, Therapist)
│   │   ├── Specialty.cs          (NeuralTherapy, Chiropractic, Physiotherapy)
│   │   └── Gender.cs
│   ├── Interfaces/
│   │   ├── IDoctorRepository.cs
│   │   ├── IAppointmentRepository.cs
│   │   ├── IPatientRepository.cs
│   │   ├── IUserRepository.cs
│   │   └── ISchedulingSettingsRepository.cs
│   └── Exceptions/
│       ├── SlotNotAvailableException.cs
│       └── DuplicateAppointmentException.cs
│
├── Piedrazul.Application/
│   ├── Common/
│   │   ├── Behaviors/
│   │   │   └── ValidationBehavior.cs
│   │   ├── Interfaces/
│   │   │   ├── IJwtService.cs
│   │   │   ├── IPasswordHasher.cs
│   │   │   ├── IAuditService.cs
│   │   │   ├── IUnitOfWork.cs
│   │   │   └── ICurrentUserService.cs
│   │   └── Options/
│   │       └── SchedulingOptions.cs
│   └── Modules/
│       ├── Auth/
│       │   ├── Commands/Login/    (LoginCommand, LoginHandler, LoginValidator)
│       │   └── Commands/Register/ (RegisterCommand, RegisterHandler, RegisterValidator)
│       ├── Doctors/
│       │   ├── Commands/          (Create, Update, Deactivate, Activate, AddSchedule, RemoveSchedule)
│       │   └── Queries/           (GetDoctors)
│       ├── Appointments/
│       │   ├── Commands/          (Create, Book, Cancel, Complete, Reschedule)
│       │   └── Queries/           (GetByDate, GetByDoctorAndDate)
│       ├── Patients/
│       │   └── Queries/           (GetByDocument)
│       └── Scheduling/
│           └── Queries/           (GetAvailableSlots, GetSchedulingConfig)
│
├── Piedrazul.Infrastructure/
│   ├── Persistence/
│   │   ├── PiedrazulDbContext.cs
│   │   ├── UnitOfWork.cs
│   │   ├── Configurations/
│   │   │   ├── UserConfiguration.cs
│   │   │   ├── PatientConfiguration.cs
│   │   │   ├── DoctorConfiguration.cs
│   │   │   ├── AppointmentConfiguration.cs
│   │   │   ├── DoctorScheduleConfiguration.cs
│   │   │   ├── AuditLogConfiguration.cs
│   │   │   └── SchedulingSettingsConfiguration.cs
│   │   └── Migrations/
│   ├── Repositories/
│   │   ├── DoctorRepository.cs
│   │   ├── AppointmentRepository.cs
│   │   ├── PatientRepository.cs
│   │   ├── UserRepository.cs
│   │   └── SchedulingSettingsRepository.cs
│   └── Services/
│       ├── JwtService.cs
│       ├── PasswordHasher.cs
│       ├── CurrentUserService.cs
│       └── AuditService.cs
│
├── Piedrazul.API/
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── DoctorsController.cs
│   │   ├── AppointmentsController.cs
│   │   ├── PatientAppointmentsController.cs
│   │   ├── SchedulingController.cs
│   │   ├── PatientsController.cs
│   │   └── SeedController.cs
│   ├── Middleware/
│   │   └── ExceptionHandlingMiddleware.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── Program.cs
│
└── Piedrazul.Tests/
    ├── Helpers/
    │   └── EntityBuilder.cs
    ├── RF1_ListarCitasMedicoTests.cs
    ├── RF2_CrearCitaWhatsAppTests.cs
    ├── RF3_AgendarCitaTests.cs
    └── RF4_ConfigurarParametrosTests.cs
```

---

## Modelo de dominio

```
┌──────────┐        ┌───────────────┐        ┌──────────────┐
│   User   │──1:1──▶│    Patient    │        │    Doctor    │
│──────────│        │───────────────│        │──────────────│
│ Username │        │ DocumentId    │        │ FullName     │
│ Password │        │ FullName      │        │ Type (enum)  │
│ Role     │        │ Phone         │        │ Specialty    │
│ IsActive │        │ Gender        │        │ Interval min │
└──────────┘        │ BirthDate     │        │ IsActive     │
     │              └───────────────┘        └──────────────┘
     │1:1                   │1:N                   │1:N
     ▼                      ▼                      ▼
 (doctor)            ┌─────────────┐        ┌──────────────┐
                     │ Appointment │        │DoctorSchedule│
                     │─────────────│        │──────────────│
                     │ Date + Time │        │ DayOfWeek    │
                     │ Status      │        │ StartTime    │
                     │ Specialty   │        │ EndTime      │
                     │ Reschedule  │        └──────────────┘
                     │   metadata  │
                     └─────────────┘
                            │
                            ▼
                      ┌──────────┐
                      │ AuditLog │
                      └──────────┘
```

---

## API — Endpoints

### Autenticación

| Método | Ruta | Acceso | Descripción |
|---|---|---|---|
| `POST` | `/api/auth/login` | Público | Login → devuelve JWT |
| `POST` | `/api/auth/register` | Público / Admin | Registro de usuario. Roles elevados requieren token Admin |

### Médicos

| Método | Ruta | Roles | Descripción |
|---|---|---|---|
| `GET` | `/api/doctors` | Anónimo | Listar médicos (filtro por especialidad) |
| `POST` | `/api/doctors` | Admin | Crear médico |
| `PUT` | `/api/doctors/{id}` | Admin | Actualizar médico |
| `DELETE` | `/api/doctors/{id}` | Admin | Desactivar médico |
| `POST` | `/api/doctors/{id}/activate` | Admin | Reactivar médico |
| `POST` | `/api/doctors/{doctorId}/schedules` | Admin | Agregar horario al médico |
| `DELETE` | `/api/doctors/{doctorId}/schedules/{scheduleId}` | Admin | Eliminar horario |

### Citas (Agendadores / Administradores)

| Método | Ruta | Roles | Descripción |
|---|---|---|---|
| `GET` | `/api/appointments` | Admin, Scheduler | Listar citas por fecha |
| `GET` | `/api/appointments/by-doctor` | Admin, Scheduler | Listar citas por médico y fecha |
| `POST` | `/api/appointments` | Admin, Scheduler | Crear cita manualmente |

### Citas (Paciente)

| Método | Ruta | Roles | Descripción |
|---|---|---|---|
| `POST` | `/api/patient/appointments` | Patient | Paciente agenda su propia cita |

### Agendamiento

| Método | Ruta | Acceso | Descripción |
|---|---|---|---|
| `GET` | `/api/scheduling/config` | Anónimo | Obtener configuración de ventana de agendamiento |
| `GET` | `/api/scheduling/slots` | Autenticado | Obtener franjas disponibles para médico y fecha |

### Pacientes

| Método | Ruta | Roles | Descripción |
|---|---|---|---|
| `GET` | `/api/patients/by-document` | Admin, Scheduler | Buscar paciente por número de documento |

### Seed (solo desarrollo)

| Método | Ruta | Descripción |
|---|---|---|
| `POST` | `/api/seed/admin` | Crea usuario admin (`admin` / `admin123`) |
| `POST` | `/api/seed/scheduler` | Crea usuario agendador |
| `POST` | `/api/seed/doctor` | Crea médico de prueba con horario Lun–Vie |
| `POST` | `/api/seed/{doctorId}/schedules` | Agrega horarios Lun–Vie a médico existente |

---

## Autenticación y autorización

El sistema utiliza **JWT Bearer Tokens** con los siguientes claims embebidos:

| Claim | Contenido |
|---|---|
| `NameIdentifier` | UserId (GUID) |
| `Name` | Username |
| `Role` | Rol del usuario |
| `fullName` | Nombre completo |

Los tokens tienen una vigencia de **24 horas**. La firma usa **HMAC-SHA256** con la clave configurada en `appsettings`.

### Roles y sus permisos

| Rol | Capacidades |
|---|---|
| **Admin** | Todo: gestionar médicos, horarios, citas, usuarios |
| **Scheduler** | Crear/ver citas, buscar pacientes |
| **Doctor** | Acceso a sus propias citas (lectura) |
| **Patient** | Agendar y consultar sus propias citas |

---

## Configuración

### `appsettings.json`

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=piedrazul_db;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Secret": "<clave-secreta-larga>",
    "Issuer": "Piedrazul.API",
    "Audience": "Piedrazul.Client"
  },
  "Scheduling": {
    "WeeksAhead": 4
  }
}
```

| Variable | Descripción |
|---|---|
| `ConnectionStrings:Default` | Cadena de conexión a PostgreSQL |
| `Jwt:Secret` | Clave secreta para firma de tokens JWT |
| `Jwt:Issuer` | Identificador del emisor del token |
| `Jwt:Audience` | Audiencia esperada del token |
| `Scheduling:WeeksAhead` | Semanas hacia adelante habilitadas para agendar |

---

## Ejecución local

### Requisitos previos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL 14+

### Pasos

```bash
# 1. Clonar el repositorio
git clone <repo-url>
cd Piedrazul

# 2. Configurar la cadena de conexión en appsettings.json o mediante variables de entorno

# 3. Restaurar dependencias
dotnet restore

# 4. Ejecutar el API (las migraciones se aplican automáticamente al iniciar)
dotnet run --project Piedrazul.API

# 5. Acceder a Swagger UI
# http://localhost:5107/swagger
```

> La aplicación aplica las migraciones de EF Core automáticamente en el arranque mediante `dbContext.Database.MigrateAsync()`.

### Datos de prueba (seed)

Una vez iniciada la aplicación:

```bash
# Crear usuario administrador
curl -X POST http://localhost:5107/api/seed/admin

# Crear agendador
curl -X POST http://localhost:5107/api/seed/scheduler

# Crear médico con horario de lunes a viernes
curl -X POST http://localhost:5107/api/seed/doctor
```

---

## Tests

Los tests están organizados por **requerimiento funcional (RF)**:

| Archivo | Requerimiento cubierto |
|---|---|
| `RF1_ListarCitasMedicoTests` | Listado de citas por médico y fecha |
| `RF2_CrearCitaWhatsAppTests` | Creación de citas por canal WhatsApp / agendador |
| `RF3_AgendarCitaTests` | Autoagendamiento del paciente con validación de slots |
| `RF4_ConfigurarParametrosTests` | Configuración de parámetros de agendamiento |

```bash
# Ejecutar todos los tests
dotnet test

# Con reporte detallado
dotnet test --logger "console;verbosity=detailed"
```

Los tests unitarios utilizan **mocks de repositorios** (Moq) para aislar la lógica de aplicación de la base de datos, y **FluentAssertions** para aserciones expresivas y legibles.

---

## Decisiones de diseño

### Clean Architecture sobre MVC tradicional

Se eligió Clean Architecture para garantizar que la lógica de negocio sea completamente independiente del framework, la base de datos y la capa HTTP. Esto facilita el testing unitario y reduce el acoplamiento entre capas.

### CQRS con MediatR en lugar de servicios de aplicación clásicos

MediatR permite que cada caso de uso tenga su propio handler, su validator y su DTO sin necesidad de servicios monolíticos. Esto favorece el **principio de responsabilidad única** y hace que el código sea fácil de localizar y extender.

### Lógica de slots en el dominio

La generación de franjas horarias disponibles (`GetAvailableSlots`) y la validación de una franja (`IsValidSlot`) viven directamente en la entidad `Doctor`. Esto encapsula el conocimiento del negocio en el lugar correcto y evita que handlers de aplicación repliquen esa lógica.

### Excepciones de dominio semánticas

En lugar de retornar booleanos o códigos de error, el dominio lanza excepciones específicas (`SlotNotAvailableException`, `DuplicateAppointmentException`). El middleware centralizado las intercepta y produce la respuesta HTTP correcta, manteniendo los controllers y handlers limpios.

### Auditoría integrada

Toda acción relevante sobre citas queda registrada en `AuditLog` con el usuario que la ejecutó, la entidad afectada y el timestamp. Esto es un requerimiento de trazabilidad clínica.

### Ventana de agendamiento configurable

El parámetro `WeeksAhead` en `appsettings.json` (y en la entidad `SchedulingSettings`) define cuántas semanas hacia adelante pueden agendarse citas. Puede modificarse en tiempo de ejecución sin redesplegar, lo cual da flexibilidad operativa a la clínica.

### Seed controller para desarrollo ágil

El `SeedController` permite crear datos de prueba con un solo request HTTP, acelerando el ciclo de desarrollo y las demostraciones del sistema sin depender de scripts SQL externos.

### Auto-migración en arranque

`MigrateAsync()` se invoca al iniciar la aplicación, garantizando que el esquema de base de datos esté siempre actualizado sin pasos manuales en el despliegue.

---

## Equipo

Proyecto académico — Universidad del Cauca, 2026-I
Asignatura: Ingeniería de Software III
