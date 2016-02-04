using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Scraper.Models;

namespace Scraper.Repositories
{
    public class ScraperRepository : IScraperRepository
    {
	protected static IMongoClient _client;
	protected static IMongoDatabase _database;
	
	public ScraperRepository(string dbName)
	{
	    _client = new MongoClient();
	    _database = _client.GetDatabase(dbName);
	}

	public void Add(Scrape scrape)
	{
	    var collection = _database.GetCollection<Scrape>("scrapes");
	    collection.InsertOne(scrape);
	}

	public void Update(Scrape newScrape)
	{
	    var collection = _database.GetCollection<Scrape>("scrapes");
	    collection.ReplaceOne(s => string.Equals(s.Id, newScrape.Id), newScrape);
	}

	public void Remove(string id)
	{
	    var collection = _database.GetCollection<Scrape>("scrapes");
	    collection.DeleteOne(s => string.Equals(s.Id, id));    
	}

	public IEnumerable<Scrape> GetAll()
	{
	    var collection = _database.GetCollection<Scrape>("scrapes");
	    return collection.Find(_ => true).ToListAsync().GetAwaiter().GetResult();
	}

	public void DeleteAll()
	{
	    _database.GetCollection<Scrape>("scrapes").DeleteManyAsync(s => true).GetAwaiter().GetResult();
	}
	
	public Scrape Get(string id)
	{
	    return _database.GetCollection<Scrape>("scrapes").Find(s => string.Equals(s.Id, id)).FirstOrDefault();
	}
    }
}