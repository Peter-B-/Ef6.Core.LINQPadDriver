namespace Ef6Demo.CodeFirst;

public class Parent
{
    public int Id { get; set; }
    public virtual string Name { get; set; }
    public virtual ICollection<Child> Children { get; set; }
}
