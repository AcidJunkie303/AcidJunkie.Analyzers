namespace Playground;

public sealed class Entity
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public List<ProjectionEntity> ProjectionEntities { get; set; } = null!;
    public ProjectionEntity ProjectionEntity { get; set; } = null!;
}
