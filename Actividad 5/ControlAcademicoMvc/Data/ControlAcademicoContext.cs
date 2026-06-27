using ControlAcademicoMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace ControlAcademicoMvc.Data;

public class ControlAcademicoContext : DbContext
{
    public ControlAcademicoContext(DbContextOptions<ControlAcademicoContext> options)
        : base(options)
    {
    }

    public DbSet<Alumno> Alumnos => Set<Alumno>();
}