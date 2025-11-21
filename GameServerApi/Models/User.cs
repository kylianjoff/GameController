namespace GameServerApi;

public enum Role
{
    Admin,
    User
}
public class User
{
    public int id { get; set; }
    public string? username { get; set; }
    public string? password { get; set; }
    public Role role { get; set; }

    public User() { }

    public User(string? username, string? password, Role role)
    {
        this.username = username;
        this.password = password;
        this.role = role;
    }
}