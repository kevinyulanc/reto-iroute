# Reto técnico: carga y validación de comercios

Aplicación con SQL Server, API en .NET 8 y front en Angular. Carga un archivo
`commerce_DDMMYYYY.csv`, valida los registros de una fecha y mueve los inválidos
a cuarentena con su motivo.

## Estructura

- `database/`: scripts SQL (tablas y stored procedures)
- `backend/Commerce.Api/`: API .NET 8 (Controllers, Services, Repositories, Models)
- `frontend/commerce-web/`: front Angular

## Requisitos

- .NET 8 SDK
- Node.js LTS y Angular CLI
- SQL Server (Express) y SSMS

## Cómo ejecutarlo

1. **Base de datos:** en SSMS ejecuta, en orden, `database/01_schema.sql` y `database/02_batch.sql`.

2. **API:**

```powershell
   cd backend/Commerce.Api
   dotnet run --launch-profile http
```

   La cadena de conexión está en `appsettings.json` (`ConnectionStrings:Default`).
   Si tu instancia de SQL Server se llama distinto, cambia `Server=` en esa cadena.
   `TrustServerCertificate=True` se usa solo para desarrollo local.

3. **Front:**

```powershell
   cd frontend/commerce-web
   npm install
   ng serve
```

   Abrir http://localhost:4200

## Endpoints

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/commerce/upload` | Carga el CSV (form-data, campo `file`). BadRequest si está vacío o el nombre no cumple el formato |
| POST | `/api/commerce/process` | Recibe la fecha y mueve a cuarentena los registros inválidos. Devuelve cuántos movió |
| GET | `/api/commerce/quarantine` | Lista los registros en cuarentena con su motivo |

## Reglas de validación (cuarentena)

Se mueve a cuarentena un registro si:

- `pc_nomcomred` está vacío
- `pc_numdoc` está vacío, o contiene letras o caracteres especiales

El motivo se guarda en la columna `motivo` (si falla más de una regla, se concatenan).

## Decisiones técnicas

- **Arquitectura por capas** en la API: Controller, Service (reglas de negocio y lectura del CSV con CsvHelper) y Repository (ADO.NET con stored procedures).
- **Inserción en lote:** todas las filas se envían en una sola llamada con un parámetro tipo tabla (`sp_create_commerce_batch`), en vez de un SP por fila. `sp_create_commerce` se mantiene como pide el enunciado.
- **Procesamiento atómico:** `sp_process_commerce` usa una transacción, así que mover a cuarentena y borrar del origen ocurre todo o nada.
- **Front:** previsualiza el CSV antes de enviarlo y valida el nombre del archivo y que no esté vacío.
- **Supuesto:** el enunciado no incluye las columnas del CSV; se usaron `pc_processdate`, `pc_tipodoc`, `pc_numdoc`, `pc_nomcomred`, `pc_razonsocial`, `pc_direccion`, `pc_telefono` y `pc_email`.