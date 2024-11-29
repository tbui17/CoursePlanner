using Lib.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lib.Models;

[Index(nameof(Email), IsUnique = true)]
public class Instructor : IContact
{
    public ICollection<Course> Courses { get; set; } = [];
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public override string ToString()
    {
        return Name;
    }
}