using System;

public class UserAccount
{
    private string _name = "Unspecified";
    public string Name
    {
        get
        {
            return _name;
        }
        set
        {
            _name = value;
        }
    }
    public string Password { get; set; }
}
