using System.Net.Mail;
using System.Text.RegularExpressions;
using Lib.Exceptions;
using Lib.Interfaces;
using Lib.Utils;
using Microsoft.EntityFrameworkCore;

namespace Lib.Models;

[Index(nameof(Email), IsUnique = true)]
public partial class Instructor : IContact
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



}