using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using ShoppingList.Models;

namespace ShoppingList.Controllers;

public class ShoppingListController : Controller
{
    private static IMongoDatabase? _database;
    private readonly ILogger<ShoppingListController> _logger;
    private readonly IConfiguration _configuration;

    public ShoppingListController(ILogger<ShoppingListController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        EnsureMongoConnection();
        
        try
        {
            var productsCollection = GetProductsCollection();
            var products = productsCollection.Find(product => true).ToList();
            return View(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products from database.");
            return View("Error");
        }
    }

    [HttpPost]
    public IActionResult Create(string productName)
    {
        EnsureMongoConnection();
        
        if (ModelState.IsValid)
        {
            try
            {
                var newProduct = new Product
                {
                    Id = ObjectId.GenerateNewId(),
                    Name = productName,
                    IsPurchased = false
                };

                var productsCollection = GetProductsCollection();
                productsCollection.InsertOne(newProduct);
                _logger.LogInformation("Product created successfully: {ProductName}", productName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new product: {ProductName}", productName);
            }
        }
        else
        {
            _logger.LogWarning("Model state invalid for creating product: {ProductName}", productName);
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Edit(string id, bool isPurchased)
    {
        EnsureMongoConnection();
        
        try
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Id, new ObjectId(id));
            var update = Builders<Product>.Update.Set(p => p.IsPurchased, isPurchased);

            var productsCollection = GetProductsCollection();
            productsCollection.UpdateOne(filter, update);
            _logger.LogInformation("Product updated successfully: {ProductId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product: {ProductId}", id);
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Delete(string id)
    {
        try
        {
            var productsCollection = GetProductsCollection();
            productsCollection.DeleteOne(product => product.Id == new ObjectId(id));
            _logger.LogInformation("Product deleted successfully: {ProductId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product: {ProductId}", id);
        }

        return RedirectToAction("Index");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private void EnsureMongoConnection()
    {
        if (_database != null)
        {
            return;
        }

        try
        {
            var mongoDbSettings = _configuration.GetSection("MongoDB");
            var connectionString = mongoDbSettings["ConnectionString"];
            var databaseName = mongoDbSettings["Database"];

            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);

            _logger.LogInformation("MongoDB connection established successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error establishing MongoDB connection.");
            throw;
        }
    }

    private IMongoCollection<Product> GetProductsCollection() =>
        _database!.GetCollection<Product>(_configuration.GetSection("MongoDB")["CollectionName"]);
}