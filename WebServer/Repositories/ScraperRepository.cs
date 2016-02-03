using System;
using MongoDB.Bson;
using MongoDB.Driver;
using Scraper.Models;

namespace Scraper.Repositories
{
    public class ScraperRepository : IScraperRepository
    {
	protected static IMongoClient _client;
	protected static IMongoDatabase _database;
	
	public ScraperRepository()
	{
	    _client = new MongoClient();
	    _database = _client.GetDatabase("ScraperDB");
	}

	public void Add(Scrape scrape)
	{
	    var collection = _database.GetCollection<Scrape>("scrapes");
	    collection.InsertOne(scrape);
	}

	public void Update(Scrape scrape)
	{
	    var collection = _database.GetCollection<Scrape>("scrapes");
	    var filter = Builders<Scrape>.Filter.Eq(s => s.Id, scrape.Id);
	    collection.ReplaceOne(filter, scrape);
	}

	public void Remove(string id)
	{
	    var collection = _database.GetCollection<Scrape>("scrapes");
	    var filter = Builders<Scrape>.Filter.Eq(s => s.Id, id);
	    collection.DeleteOne(filter);    
	}
    }
}