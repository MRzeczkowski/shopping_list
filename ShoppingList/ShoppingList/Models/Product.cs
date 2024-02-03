using MongoDB.Bson;

namespace ShoppingList.Models;

public class Product
{
    public ObjectId? Id { get; set; }

    public required string Name { get; set; }

    public bool IsPurchased { get; set; }
}
