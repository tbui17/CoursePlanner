namespace CoursePlanner.Models;

public interface IDbEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
}