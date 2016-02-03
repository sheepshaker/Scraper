using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using DownloaderUtil;
using Scraper.Models;

namespace Scraper.Controllers
{
    public class ScraperController : Controller
    {
        [HttpGet]
        public IEnumerable<Scrape> GetAllScrapes()
        {
            return null;
        }
 
        [HttpGet("{scrapeId:int}", Name = "GetScrapeById")]
        public IActionResult GetScrapeById(int scrapeId)
        {
            return null;
        }
    }
}