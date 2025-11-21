namespace GameServerApi;

public record UserPublic
{
    public int id { get; set; }
    public string? username { get; set; }
    public Role role { get; set; }

    public UserPublic() {}

    public UserPublic(int id, string? username, Role role)
    {
        this.id = id;
        this.username = username;
        this.role = role;
    }
}