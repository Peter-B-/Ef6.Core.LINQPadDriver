namespace Ef6Demo.CodeFirst;

public class Child
{
    public int Id { get; set; }
    public virtual string Name { get; set; }

    public virtual int ParentId { get; set; }

    public virtual Parent Parent { get; set; }
}
