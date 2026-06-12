using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Base de datos simulada en memoria
var coleccionNodos = new List<NodoElemento>
{
    new NodoElemento { Id = 10, Valor = "Raíz Inicial (ABB)" },
    new NodoElemento { Id = 5, Valor = "Hijo Izquierdo" }
};

// 1. EJEMPLO DE GET: Retorna todos los nodos actuales
app.MapGet("/api/nodos", () => Results.Ok(coleccionNodos));

// 2. EJEMPLO DE POST: Recibe un nuevo nodo y lo "inserta" en la colección
app.MapPost("/api/nodos", (NodoElemento nuevoNodo) =>
{
    // Validación simple
    if (nuevoNodo.Id <= 0 || string.IsNullOrEmpty(nuevoNodo.Valor))
    {
        return Results.BadRequest("Datos del nodo inválidos.");
    }

    coleccionNodos.Add(nuevoNodo);
    return Results.Created($"/api/nodos/{nuevoNodo.Id}", nuevoNodo);
});

app.Run();

public class NodoElemento
{
    public int Id { get; set; }
    public string Valor { get; set; } = string.Empty;
}
