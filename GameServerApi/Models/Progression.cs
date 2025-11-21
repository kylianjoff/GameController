namespace GameServerApi;

public class Progression
{
    public int id { get; set; }
    public int userId { get; set; }
    public int count { get; set; }
    public int multiplier { get; set; }
    public int totalClickValue { get; set; }
    public int bestScore { get; set; }

    public Progression() {}
}