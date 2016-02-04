using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using DownloaderUtil;
using Scraper.Models;

namespace Scraper.Controllers
{
    [Route("api/[controller]")]
    public class ScraperController : Controller
    {
        [HttpGet]
        public IEnumerable<Scrape> GetAllScrapes()
        {
            return new Scrape[]{
		new Scrape("test url"){
		    Name = "Name1"
		},
		new Scrape("test url"){
		    Name = "Name2"
		},new Scrape("test url"){
		    Name = "Name3"
		},new Scrape("test url"){
		    Name = "Name4"
		}
	    };
        }
 
        [HttpGet("{scrapeId:int}", Name = "GetScrapeById")]
        public IActionResult GetScrapeById(int scrapeId)
        {
            return null;
        }
    }
}