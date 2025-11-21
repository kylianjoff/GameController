namespace GameServerApi;

public record ErrorResponse
{
    public string? message { get; set; }
    public string? code { get; set; }

    public ErrorResponse(string? message, string? code)
    {
        this.message = message;
        this.code = code;
    }
}