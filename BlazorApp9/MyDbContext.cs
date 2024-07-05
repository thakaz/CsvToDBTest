using BlazorApp9.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp9.Data;

public partial class MyDbContext : DbContext
{
    public MyDbContext()
    {
    }

    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

}
