namespace GameServerApi;

public record UserPublic
{
    public int id { get; set; }
    public string? username { get; set; }
    public Role role { get; set; }
}