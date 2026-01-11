using System.Data.Entity;
using Ef6Demo.CodeFirst;

namespace Ef6Demo.Startup;

public static class CodeFirstExample
{
    public static async Task Run()
    {
        using var db = new DemoContext();

        var parent = new Parent
        {
            Name = "Hans",
            Children = new List<Child>
            {
                new() { Name = "Franz" },
                new() { Name = "Sepp" }
            }
        };
        db.Parents.Add(parent);

        await db.SaveChangesAsync();


        var childNames =
            await db.Children
                .OrderBy(c => c.Name)
                .Select(c => c.Name)
                .ToListAsync();

        foreach (var name in childNames)
            Console.WriteLine(name);
    }
}
