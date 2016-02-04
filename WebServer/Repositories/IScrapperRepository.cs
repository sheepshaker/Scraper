using Scraper.Models;
using System.Collections.Generic;

namespace Scraper.Repositories
{
    public interface IScraperRepository
    {
	void Add(Scrape scrape);
	void Remove(string id);
	IEnumerable<Scrape> GetAll();
	Scrape Get(string id);
	void Update(Scrape scrape);
	void DeleteAll();
    }
}