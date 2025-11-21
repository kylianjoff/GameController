namespace GameServerApi;

public class Item
{
    public int id { get; set; }
    public string? name { get; set; }
    public int price { get; set; }
    public int maxQuantity { get; set; }
    public int clickValue { get; set; }

    public Item() {}
    public Item(string? name, int price, int maxQuantity, int clickValue)
    {
        this.name = name;
        this.price = price;
        this.maxQuantity = maxQuantity;
        this.clickValue = clickValue;
    }
}