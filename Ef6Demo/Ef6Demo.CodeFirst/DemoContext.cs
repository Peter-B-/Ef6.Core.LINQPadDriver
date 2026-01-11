using System.Data.Entity;

namespace Ef6Demo.CodeFirst;

public class DemoContext : DbContext
{
    public DemoContext()
        : base("name=DemoContext")
    {
    }


    public DemoContext(string nameOrConnectionString) : base(nameOrConnectionString)
    {
    }

    public virtual DbSet<Parent> Parents { get; set; }
    public virtual DbSet<Child> Children { get; set; }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
    }
}
