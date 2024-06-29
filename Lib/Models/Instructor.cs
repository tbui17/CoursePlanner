using Lib.Exceptions;
using Lib.Utils;
using Microsoft.EntityFrameworkCore;

namespace Lib.Models;

[Index(nameof(Email), IsUnique = true)]
public class Instructor
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public ICollection<Course> Courses { get; set; } = [];


    public override string ToString()
    {
        return Name;
    }

    public DomainException? Validate()
    {
        var fields = new Dictionary<string, string>
        {
            [nameof(Name)] = Name, [nameof(Email)] = Email, [nameof(Phone)] = Phone,
        };

        var messages = fields
           .Select(x => (x.Key, string.IsNullOrWhiteSpace(x.Value)))
           .Where(x => x.Item2)
           .Select(x => x.Item1)
           .Select(fieldName => $"{fieldName} cannot be empty.")
           .ToList();

        if (messages.Count <= 0) return null;

        var msg = messages.StringJoin("\n");
        return new DomainException(msg);
    }
}