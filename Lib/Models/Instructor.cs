using System.Net.Mail;
using Lib.Exceptions;
using Lib.Interfaces;
using Lib.Utils;
using Microsoft.EntityFrameworkCore;

namespace Lib.Models;

[Index(nameof(Email), IsUnique = true)]
public class Instructor : IInstructorFormFields
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

    Dictionary<string, string> Fields() =>
        new()
        {
            [nameof(Name)] = Name, [nameof(Email)] = Email, [nameof(Phone)] = Phone,
        };

    public DomainException? Validate()
    {
        var fields = Fields();

        var messages = GetValidationMessages();

        if (messages.Count <= 0) return null;

        var msg = messages.StringJoin("\n");


        return new DomainException(msg);

        List<string> GetValidationMessages()
        {
            var list = fields
               .Select(x => (x.Key, string.IsNullOrWhiteSpace(x.Value)))
               .Where(x => x.Item2)
               .Select(x => x.Item1)
               .Select(fieldName => $"{fieldName} cannot be empty.")
               .ToList();

            if (!MailAddress.TryCreate(Email, out _))
            {
                list.Add("Email is not valid.");
            }

            if (Phone.Length != PhoneLength || !Phone.All(Digits.Contains))
            {
                list.Add("Phone must be in ddddddd format.");
            }

            return list;
        }
    }

    private static readonly HashSet<char> Digits = new("0123456789");
    private const int PhoneLength = 7;
}