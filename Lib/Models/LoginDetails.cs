namespace Lib.Models;

public record LoginDetails(string Username = "", string Password = "") : ILogin
{
    public LoginDetails(ILogin login) : this(login.Username, login.Password) { }
};