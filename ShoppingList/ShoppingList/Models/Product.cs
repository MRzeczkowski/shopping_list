using MongoDB.Bson;

namespace ShoppingList.Models;

public class Product
{
    public ObjectId? Id { get; init; }

    public required string Name { get; init; }

    public bool IsPurchased { get; init; }
}
