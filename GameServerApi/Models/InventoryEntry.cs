namespace GameServerApi;

public class InventoryEntry
{
    public int id { get; set; }
    public int userId { get; set; }
    public int itemId { get; set; }
    public int quantity { get; set; }

    public InventoryEntry() {}
}