# JWT Contract

## Descripción
Los JSON Web Tokens (JWT) son el mecanismo de autenticación utilizado en toda la plataforma.

## Algoritmo
- **Tipo**: HMAC SHA-256 (HS256)
- **Secret**: Configurado via variable de entorno `JWT_SECRET` (mínimo 32 caracteres)

## Header
```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

## Payload (Claims)

### Claims Estándar
| Claim | Tipo | Descripción | Ejemplo |
|-------|------|-------------|---------|
| `sub` | string (UUID) | Subject - User ID | `"550e8400-e29b-41d4-a716-446655440000"` |
| `email` | string | Email del usuario | `"user@example.com"` |
| `role` | string | Rol del usuario | `"user"` o `"admin"` |
| `iat` | number | Issued At (Unix timestamp) | `1704067200` |
| `exp` | number | Expiration (Unix timestamp) | `1704070800` |
| `iss` | string | Issuer | `"file-share-platform"` |
| `aud` | string | Audience | `"file-share-users"` |
| `jti` | string (UUID) | JWT ID único | `"123e4567-e89b-12d3-a456-426614174000"` |

### Payload Example
```json
{
  "sub": "550e8400-e29b-41d4-a716-446655440000",
  "email": "john.doe@example.com",
  "role": "user",
  "iat": 1704067200,
  "exp": 1704070800,
  "iss": "file-share-platform",
  "aud": "file-share-users",
  "jti": "123e4567-e89b-12d3-a456-426614174000"
}
```

## Token Completo (Ejemplo)
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI1NTBlODQwMC1lMjliLTQxZDQtYTcxNi00NDY2NTU0NDAwMDAiLCJlbWFpbCI6ImpvaG4uZG9lQGV4YW1wbGUuY29tIiwicm9sZSI6InVzZXIiLCJpYXQiOjE3MDQwNjcyMDAsImV4cCI6MTcwNDA3MDgwMCwiaXNzIjoiZmlsZS1zaGFyZS1wbGF0Zm9ybSIsImF1ZCI6ImZpbGUtc2hhcmUtdXNlcnMiLCJqdGkiOiIxMjNlNDU2Ny1lODliLTEyZDMtYTQ1Ni00MjY2MTQxNzQwMDAifQ.signature
```

## Uso en Headers HTTP

### Authorization Header
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Ejemplo de Request
```http
GET /api/files HTTP/1.1
Host: localhost:5000
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI1NTBlODQwMC1lMjliLTQxZDQtYTcxNi00NDY2NTU0NDAwMDAiLCJlbWFpbCI6ImpvaG4uZG9lQGV4YW1wbGUuY29tIiwicm9sZSI6InVzZXIiLCJpYXQiOjE3MDQwNjcyMDAsImV4cCI6MTcwNDA3MDgwMCwiaXNzIjoiZmlsZS1zaGFyZS1wbGF0Zm9ybSIsImF1ZCI6ImZpbGUtc2hhcmUtdXNlcnMiLCJqdGkiOiIxMjNlNDU2Ny1lODliLTEyZDMtYTQ1Ni00MjY2MTQxNzQwMDAifQ.signature
Content-Type: application/json
```

## Uso en SOAP Service

### Header SOAP con JWT
```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/">
    <soapenv:Header>
        <AuthToken>Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</AuthToken>
    </soapenv:Header>
    <soapenv:Body>
        <!-- Request body -->
    </soapenv:Body>
</soapenv:Envelope>
```

## Uso en SignalR/WebSocket

### Connection con Token
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/file-upload", {
        accessTokenFactory: () => localStorage.getItem("token")
    })
    .build();
```

## Validación

### Reglas de Validación
1. **Signature**: Verificar firma con `JWT_SECRET`
2. **Expiration**: `exp` debe ser mayor que tiempo actual
3. **Issuer**: `iss` debe ser `"file-share-platform"`
4. **Audience**: `aud` debe ser `"file-share-users"`
5. **Not Before**: Si existe `nbf`, debe ser menor que tiempo actual

### Códigos de Error
| Código HTTP | Descripción |
|-------------|-------------|
| 401 | Token no proporcionado |
| 401 | Token inválido o expirado |
| 403 | Token válido pero sin permisos |

## Refresh Token (Opcional)

### Flow
```
1. Access Token expira
2. Frontend detecta 401
3. POST /api/auth/refresh con refresh token
4. Nuevo access token emitido
```

### Refresh Token Payload
```json
{
  "sub": "user-id",
  "type": "refresh",
  "jti": "unique-refresh-id",
  "exp": 1704672000
}
```

## Configuración

### Variables de Entorno
```env
JWT_SECRET=your-super-secret-jwt-key-min-32-characters-long-here
JWT_ISSUER=file-share-platform
JWT_AUDIENCE=file-share-users
JWT_EXPIRATION_MINUTES=60
```

### Configuración .NET (appsettings.json)
```json
{
  "Jwt": {
    "Secret": "${JWT_SECRET}",
    "Issuer": "${JWT_ISSUER}",
    "Audience": "${JWT_AUDIENCE}",
    "ExpirationMinutes": 60
  }
}
```

## Seguridad

### Recomendaciones
- ✅ Almacenar token en memoria (no localStorage para producción)
- ✅ Usar HTTPS en producción
- ✅ Rotar JWT_SECRET periódicamente
- ✅ Implementar logout invalidando refresh tokens
- ✅ Usar tokens de corta duración (< 1 hora)
- ❌ No incluir información sensible en el payload
- ❌ No compartir tokens entre dominios
