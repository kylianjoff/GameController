namespace GameServerApi;

public enum Role
{
    User,
    Admin
}
public class User
{
    public int id { get; set; }
    public string? pseudo { get; set; }
    public string? password { get; set; }
    public Role role { get; set; }

    public User() { }

    public User(string? pseudo, string? password, Role role)
    {
        this.pseudo = pseudo;
        this.password = password;
        this.role = role;
    }
}