
public class LoginBody
{
    public string email { get; set; } = "";
    public string password { get; set; } = "";

    public LoginBody(string _email, string _password)
        => (email, password) = (_email, _password);
}
