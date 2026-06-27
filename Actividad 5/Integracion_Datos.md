# Integracion de Datos

## Parte 1: Evaluacion Conceptual y Buenas Practicas

### 1. Formatos de Intercambio

| Formato | Ventajas | Desventajas |
| --- | --- | --- |
| CSV | Es simple, liviano, facil de generar y procesar, y funciona bien para carga masiva de datos tabulares. | No maneja bien estructuras jerarquicas, tiene poca semantica y depende de convenciones para separar campos y encabezados. |
| XML | Es auto-descriptivo, soporta estructuras complejas y jerarquias, y permite incluir metadatos. | Es mas verboso, ocupa mas espacio y suele ser mas lento de leer y escribir que CSV. |

### 2. Diferenciacion de Procesos

En `System.Text.Json`, la **serializacion** es el proceso de convertir un objeto de C# a una representacion JSON. Se usa, por ejemplo, cuando una aplicacion envia datos a una API o guarda informacion en formato JSON.

La **deserializacion** es el proceso inverso: toma un JSON recibido y lo convierte en un objeto tipado de C#. En este caso, `JsonSerializer.Deserialize<T>()` reconstruye la instancia a partir del texto JSON.

La diferencia tecnica central es la direccion de la transformacion: serializar va de objeto a JSON, y deserializar va de JSON a objeto.

### 3. El Antipatron del Rendimiento

El error comun de rendimiento llamado **N+1** ocurre cuando se procesa un archivo masivo y, por cada registro leido, se ejecuta una operacion adicional contra la base de datos o un servicio externo. Eso genera una consulta o insercion inicial, mas N operaciones repetidas, lo que multiplica el costo total.

La solucion es aplicar **Batching**: acumular registros en memoria controlada y enviarlos en bloques. En lugar de insertar fila por fila, se agregan varios registros con `AddRange()` y se ejecuta una sola llamada a `SaveChangesAsync()` por lote, reduciendo el numero de viajes a la base de datos y mejorando el rendimiento.

## Parte 2: Implementacion Practica en C#

### Desafio 1: Consumo de Endpoints y Deserializacion

```csharp
using System.Net.Http;
using System.Text.Json;

public class AlumnoServicio
{
    private readonly HttpClient _httpClient;

    public AlumnoServicio(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Alumno?> ObtenerAlumnoAsync()
    {
        try
        {
            using HttpResponseMessage respuesta = await _httpClient.GetAsync("https://api.usac.edu/v1/alumnos");
            respuesta.EnsureSuccessStatusCode();

            string contenidoJson = await respuesta.Content.ReadAsStringAsync();

            JsonSerializerOptions opciones = new()
            {
                PropertyNameCaseInsensitive = true
            };

            Alumno? alumno = JsonSerializer.Deserialize<Alumno>(contenidoJson, opciones);
            return alumno;
        }
        catch (HttpRequestException)
        {
            throw;
        }
        catch (JsonException)
        {
            throw;
        }
    }
}

public class Alumno
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Carrera { get; set; } = string.Empty;
}
```

### Desafio 2: Endpoint para Carga Masiva CSV

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class AlumnosController : ControllerBase
{
    private readonly ControlAcademicoContext _context;

    public AlumnosController(ControlAcademicoContext context)
    {
        _context = context;
    }

    [HttpPost("carga-masiva-csv")]
    public async Task<IActionResult> CargarCsvMasivo([FromForm] IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
        {
            return BadRequest(new { mensaje = "Debe enviar un archivo CSV valido." });
        }

        List<Alumno> alumnos = new();

        await using Stream stream = archivo.OpenReadStream();
        using StreamReader lector = new(stream);

        string? linea;

        while ((linea = await lector.ReadLineAsync()) != null)
        {
            if (string.IsNullOrWhiteSpace(linea))
            {
                continue;
            }

            string[] columnas = linea.Split(',');

            if (columnas.Length < 3)
            {
                continue;
            }

            Alumno alumno = new()
            {
                Id = int.Parse(columnas[0]),
                Nombre = columnas[1].Trim(),
                Carrera = columnas[2].Trim()
            };

            alumnos.Add(alumno);
        }

        _context.Alumnos.AddRange(alumnos);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Carga masiva completada correctamente.",
            registrosProcesados = alumnos.Count
        });
    }
}

public class ControlAcademicoContext : DbContext
{
    public ControlAcademicoContext(DbContextOptions<ControlAcademicoContext> options)
        : base(options)
    {
    }

    public DbSet<Alumno> Alumnos => Set<Alumno>();
}
```

## Parte 3: Referencias Bibliograficas

Facultad de Ingenieria, USAC. (2026). *Sesion 20: Integracion de Datos. Consumo de APIs Externas y Carga Masiva (CSV/XML)*. Laboratorio del curso Introduccion a la Programacion y Computacion 2. Guatemala.