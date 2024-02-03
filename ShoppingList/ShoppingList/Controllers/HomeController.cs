using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using ShoppingList.Models;

namespace ShoppingList.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    private readonly IMongoCollection<Product> _products;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;

        var client = new MongoClient("mongodb://productsApi:Pa$$w0rd!@localhost:27017/productsDB");
        var database = client.GetDatabase("productsDB");
        _products = database.GetCollection<Product>("products");
    }

    public IActionResult Index()
    {
        var products = _products.Find(product => true).ToList();

        return View(products);
    }

    [HttpPost]
    public IActionResult Create(Product newProduct)
    {
        if (ModelState.IsValid)
        {
            newProduct.Id = ObjectId.GenerateNewId();
            _products.InsertOne(newProduct);
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Edit(string id, bool isPurchased)
    {
        var filter = Builders<Product>.Filter.Eq(p => p.Id, new ObjectId(id));
        var update = Builders<Product>.Update.Set(p => p.IsPurchased, isPurchased);

        _products.UpdateOne(filter, update);

        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Delete(string id)
    {
        _products.DeleteOne(product => product.Id == new ObjectId(id));
        return RedirectToAction("Index");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}