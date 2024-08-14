namespace Lib.Interfaces;

public interface IInstructorFormFields
{
    string Name { get; set; }
    string Phone { get; set; }
    string Email { get; set; }

    public void SetValidValues()
    {
        Name = "Name Abcd";
        Phone = "1234567";
        Email = "Name12345@mail.com";
    }
}