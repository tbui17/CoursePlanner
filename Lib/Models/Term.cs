namespace Lib.Models;

public class Term
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    public DateTime Start { get; set; } = DateTime.Now;
    public DateTime End { get; set; } = DateTime.Now;
    
    public ICollection<Course> Courses { get; set; } = [];
    
    


}