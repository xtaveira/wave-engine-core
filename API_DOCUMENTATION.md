# 🔌 **API DOCUMENTATION - Wave Engine Core Level 4**

## 🔐 **AUTHENTICATION ENDPOINTS**

### **Base URL**: `/api/auth`

| Method | Endpoint     | Description                          | Auth Required |
| ------ | ------------ | ------------------------------------ | ------------- |
| `POST` | `/configure` | Configure authentication credentials | ❌            |
| `POST` | `/login`     | Authenticate and get JWT token       | ❌            |
| `GET`  | `/status`    | Get authentication status            | ❌            |
| `POST` | `/validate`  | Validate JWT token                   | ❌            |
| `POST` | `/logout`    | Logout (client-side token removal)   | ❌            |

---

## 🎛️ **MICROWAVE HEATING ENDPOINTS**

### **Base URL**: `/api/microwave/heating`

### **🔑 All endpoints require Bearer Token authentication**

| Method | Endpoint    | Description                         | Request Body          |
| ------ | ----------- | ----------------------------------- | --------------------- |
| `POST` | `/start`    | Start manual heating                | `StartHeatingRequest` |
| `POST` | `/quick`    | Start quick heating (30s, power 10) | -                     |
| `POST` | `/pause`    | Pause current heating               | -                     |
| `POST` | `/cancel`   | Cancel current heating              | -                     |
| `POST` | `/add-time` | Add time to current heating         | `AddTimeRequest`      |
| `GET`  | `/status`   | Get current heating status          | -                     |

---

## 📋 **MICROWAVE PROGRAMS ENDPOINTS**

### **Base URL**: `/api/microwave/programs`

### **🔑 All endpoints require Bearer Token authentication**

| Method   | Endpoint                   | Description                            | Request Body                 |
| -------- | -------------------------- | -------------------------------------- | ---------------------------- |
| `GET`    | `/`                        | Get all programs (predefined + custom) | -                            |
| `GET`    | `/predefined`              | Get predefined programs                | -                            |
| `POST`   | `/predefined/{name}/start` | Start predefined program               | -                            |
| `GET`    | `/custom`                  | Get custom programs                    | -                            |
| `GET`    | `/custom/{id}`             | Get custom program by ID               | -                            |
| `POST`   | `/custom`                  | Create new custom program              | `CreateCustomProgramRequest` |
| `PUT`    | `/custom/{id}`             | Update custom program                  | `UpdateCustomProgramRequest` |
| `DELETE` | `/custom/{id}`             | Delete custom program                  | -                            |
| `POST`   | `/custom/{id}/start`       | Start custom program                   | -                            |

---

## 📊 **REQUEST/RESPONSE MODELS**

### **Authentication Models**

#### `AuthCredentials`

```json
{
  "username": "string",
  "password": "string"
}
```

#### `AuthConfigRequest`

```json
{
  "username": "string",
  "password": "string",
  "connectionString": "string (optional)"
}
```

#### `TokenValidationRequest`

```json
{
  "token": "string"
}
```

### **Heating Models**

#### `StartHeatingRequest`

```json
{
  "timeInSeconds": 60,
  "powerLevel": 5
}
```

#### `AddTimeRequest`

```json
{
  "additionalSeconds": 30
}
```

#### `HeatingStatusResponse`

```json
{
  "isRunning": true,
  "remainingTime": 120,
  "powerLevel": 7,
  "progress": 50,
  "currentState": "HEATING",
  "heatingChar": "∩",
  "currentProgram": "Pipoca",
  "progressDisplay": "Aquecendo...",
  "startTime": "2025-09-03T22:00:00Z"
}
```

### **Program Models**

#### `CreateCustomProgramRequest`

```json
{
  "name": "Pizza",
  "food": "Pizza congelada",
  "powerLevel": 8,
  "timeInSeconds": 120,
  "character": "P",
  "instructions": "Remover embalagem antes de aquecer"
}
```

#### `PredefinedProgram`

```json
{
  "name": "Pipoca",
  "food": "Pipoca (de micro-ondas)",
  "timeInSeconds": 180,
  "powerLevel": 7,
  "heatingChar": "∩",
  "instructions": "Observar barulho de estouros..."
}
```

---

## 🔑 **AUTHENTICATION FLOW**

### **1. Configure Authentication**

```bash
POST /api/auth/configure
Content-Type: application/json

{
  "username": "admin",
  "password": "mypassword123",
  "connectionString": "Server=localhost;Database=MyDB;"
}
```

### **2. Login and Get Token**

```bash
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "mypassword123"
}

# Response:
{
  "success": true,
  "message": "Login realizado com sucesso",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAt": "2025-09-04T06:00:00Z",
    "username": "admin"
  }
}
```

### **3. Use Token in API Calls**

```bash
GET /api/microwave/heating/status
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## 📝 **USAGE EXAMPLES**

### **Start Manual Heating**

```bash
POST /api/microwave/heating/start
Authorization: Bearer <token>
Content-Type: application/json

{
  "timeInSeconds": 90,
  "powerLevel": 8
}
```

### **Start Predefined Program**

```bash
POST /api/microwave/programs/predefined/Pipoca/start
Authorization: Bearer <token>
```

### **Create Custom Program**

```bash
POST /api/microwave/programs/custom
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "Pizza",
  "food": "Pizza congelada",
  "powerLevel": 8,
  "timeInSeconds": 120,
  "character": "P",
  "instructions": "Remover embalagem"
}
```

### **Get Heating Status**

```bash
GET /api/microwave/heating/status
Authorization: Bearer <token>

# Response:
{
  "isRunning": true,
  "remainingTime": 65,
  "powerLevel": 8,
  "progress": 28,
  "currentState": "HEATING",
  "heatingChar": "P",
  "currentProgram": "Pizza"
}
```

---

## 🛡️ **ERROR RESPONSES**

### **401 Unauthorized**

```json
{
  "message": "Token inválido ou expirado",
  "errorCode": "UNAUTHORIZED",
  "traceId": "abc123"
}
```

### **400 Bad Request**

```json
{
  "message": "Parâmetros inválidos",
  "errorCode": "INVALID_PARAMETERS",
  "errors": ["Campo obrigatório"],
  "traceId": "abc123"
}
```

### **500 Internal Server Error**

```json
{
  "message": "Erro interno do servidor",
  "errorCode": "INTERNAL_ERROR",
  "traceId": "abc123"
}
```

---

## 🎯 **API SUMMARY**

- **Total Endpoints**: 20
- **Authentication**: JWT Bearer Token (8h expiration)
- **Security**: SHA1 password hashing + AES connection string encryption
- **Exception Handling**: Centralized middleware with standardized responses
- **Logging**: Thread-safe file-based exception logging
- **Testing**: 216 comprehensive tests (100% passing)
