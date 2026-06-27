using System.Text.Json;
using ControlAcademicoMvc.Models;

namespace ControlAcademicoMvc.Services;

public class AlumnoServicio
{
    private readonly HttpClient _httpClient;

    public AlumnoServicio(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Alumno?> ObtenerAlumnoAsync()
    {
        using HttpResponseMessage respuesta = await _httpClient.GetAsync("https://api.usac.edu/v1/alumnos");
        respuesta.EnsureSuccessStatusCode();

        string contenidoJson = await respuesta.Content.ReadAsStringAsync();

        JsonSerializerOptions opciones = new()
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<Alumno>(contenidoJson, opciones);
    }
}