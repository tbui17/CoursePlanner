namespace Lib.Interfaces;

public interface IEmail
{
    string Email { get; set; }
}

public interface IContactForm : IEmail
{
    string Name { get; set; }
    string Phone { get; set; }
}