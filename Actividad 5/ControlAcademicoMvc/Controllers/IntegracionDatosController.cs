using ControlAcademicoMvc.Data;
using ControlAcademicoMvc.Models;
using ControlAcademicoMvc.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControlAcademicoMvc.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IntegracionDatosController : ControllerBase
{
    private readonly ControlAcademicoContext _context;
    private readonly AlumnoServicio _alumnoServicio;

    public IntegracionDatosController(ControlAcademicoContext context, AlumnoServicio alumnoServicio)
    {
        _context = context;
        _alumnoServicio = alumnoServicio;
    }

    [HttpGet("alumno-remoto")]
    public async Task<IActionResult> ObtenerAlumnoRemoto()
    {
        try
        {
            Alumno? alumno = await _alumnoServicio.ObtenerAlumnoAsync();
            return Ok(alumno);
        }
        catch (Exception ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status502BadGateway);
        }
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

            if (!int.TryParse(columnas[0], out int id))
            {
                continue;
            }

            alumnos.Add(new Alumno
            {
                Id = id,
                Nombre = columnas[1].Trim(),
                Carrera = columnas[2].Trim()
            });
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