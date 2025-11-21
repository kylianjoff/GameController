namespace GameServerApi;

public class UserUpdate
{
    public string? username { get; set; }
    public string? password { get; set; }
    public Role role { get; set; }
}