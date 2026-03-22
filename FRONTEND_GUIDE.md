# Guía de API — Piedrazul Backend

> Dirigida al equipo frontend. Base URL de desarrollo: `https://localhost:{puerto}/api`

---

## Autenticación

Todos los endpoints protegidos requieren un **JWT Bearer token** en el header:

```
Authorization: Bearer <token>
```

El token se obtiene en el endpoint de login (auth). Si el token expira, el servidor responde `401 Unauthorized`.

---

## Roles disponibles

| Rol         | Descripción                          |
|-------------|--------------------------------------|
| `Admin`     | Acceso total                         |
| `Scheduler` | Agendador de citas                   |
| `Patient`   | Paciente autenticado (autoagendado)  |

---

## Módulo: Médicos (`/api/doctors`)

### Listar médicos activos

```
GET /api/doctors
GET /api/doctors?specialty=NeuralTherapy
```

**Sin autenticación requerida.**

Parámetro opcional `specialty`: `NeuralTherapy` | `Chiropractic` | `Physiotherapy`

**Respuesta 200:**
```json
[
  {
    "id": "uuid",
    "fullName": "Dr. García",
    "type": "Doctor",
    "specialty": "NeuralTherapy",
    "intervalMinutes": 30
  }
]
```

> Usar este endpoint para poblar el selector de médico en el buscador de citas. Solo retorna médicos con `IsActive = true`.

---

## Módulo: Citas (`/api/appointments`)

### 1. Listar citas por médico y fecha (HU1 principal)

```
GET /api/appointments/by-doctor?doctorId={uuid}&date={YYYY-MM-DD}&search={texto}
```

**Roles:** `Admin`, `Scheduler`

| Parámetro  | Tipo     | Obligatorio | Descripción                                      |
|------------|----------|-------------|--------------------------------------------------|
| `doctorId` | `uuid`   | ✅          | ID del médico (obtenido de `/api/doctors`)       |
| `date`     | `string` | ✅          | Fecha en formato `YYYY-MM-DD` (ej: `2026-03-25`) |
| `search`   | `string` | ❌          | Busca por nombre o número de documento           |

**Respuesta 200:**
```json
{
  "message": "5 cita(s) encontrada(s).",
  "total": 5,
  "appointments": [
    {
      "id": "uuid",
      "patientName": "Juan Pérez",
      "documentId": "12345678",
      "time": "08:00",
      "specialty": "NeuralTherapy",
      "status": "Scheduled"
    }
  ]
}
```

**Cuando no hay citas:**
```json
{
  "message": "No hay citas registradas para esta búsqueda",
  "total": 0,
  "appointments": []
}
```

**Valores posibles de `status`:** `Scheduled` | `Completed` | `Cancelled` | `Rescheduled`

**Valores posibles de `specialty`:** `NeuralTherapy` | `Chiropractic` | `Physiotherapy`

**Las citas ya vienen ordenadas por `time` (hora de atención).**

---

### 2. Agenda diaria completa (todos los médicos)

```
GET /api/appointments?date={YYYY-MM-DD}
```

**Roles:** `Admin`, `Scheduler`

**Respuesta 200:**
```json
[
  {
    "id": "uuid",
    "date": "2026-03-25",
    "time": "08:00",
    "patientName": "Juan Pérez",
    "documentId": "12345678",
    "phone": "3001234567",
    "specialty": "NeuralTherapy",
    "doctorName": "Dr. García",
    "doctorId": "uuid",
    "status": "Scheduled"
  }
]
```

---

### 3. Crear cita manualmente (por agendador)

```
POST /api/appointments
Content-Type: application/json
```

**Roles:** `Admin`, `Scheduler`

**Body:**
```json
{
  "documentId": "12345678",
  "fullName": "Juan Pérez",
  "phone": "3001234567",
  "gender": "Male",
  "birthDate": "1990-05-15",
  "email": "juan@email.com",
  "doctorId": "uuid",
  "date": "2026-03-25T00:00:00",
  "time": "08:00:00"
}
```

**Respuesta 201:**
```json
{
  "appointmentId": "uuid",
  "message": "Cita creada exitosamente",
  "date": "2026-03-25",
  "time": "08:00",
  "doctorName": "Dr. García"
}
```

**Valores posibles de `gender`:** `Male` | `Female`

---

## Flujo completo — Vista de citas del agendador (HU1)

```
1. Al cargar la página:
   GET /api/doctors
   → Poblar el <Select> de médicos con id + fullName

2. Al presionar "Buscar":
   GET /api/appointments/by-doctor?doctorId={id}&date={YYYY-MM-DD}
   → Renderizar tabla con appointments[]
   → Mostrar message y total en el header de resultados

3. Al escribir en el buscador de la tabla:
   GET /api/appointments/by-doctor?doctorId={id}&date={YYYY-MM-DD}&search={texto}
   → El backend filtra por nombre o documento
   → Re-renderizar tabla con los nuevos resultados
```

---

## Errores comunes

| Código | Causa                                      | Acción sugerida                        |
|--------|--------------------------------------------|----------------------------------------|
| `400`  | Parámetros inválidos (fecha mal formateada, doctorId vacío) | Revisar el body/query params |
| `401`  | Token ausente o expirado                   | Redirigir al login                     |
| `403`  | Rol sin permiso para el endpoint           | Verificar rol del usuario autenticado  |
| `404`  | Recurso no encontrado                      | Verificar el ID enviado               |
| `500`  | Error interno del servidor                 | Reportar al equipo backend             |

---

## Notas de formato

- **Fechas** se envían como `YYYY-MM-DD` en query params y como ISO 8601 en body (`2026-03-25T00:00:00`).
- **Horas** se envían como `HH:mm:ss` en body (`"time": "08:00:00"`) y se reciben como `HH:mm` en respuestas (`"time": "08:00"`).
- **IDs** son siempre `UUID v4` (ejemplo: `"3fa85f64-5717-4562-b3fc-2c963f66afa6"`).
- **Enums** se envían y reciben como `string` (no como número).
