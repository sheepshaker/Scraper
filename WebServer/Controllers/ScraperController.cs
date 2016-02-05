using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using DownloaderUtil;
using Scraper.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Scraper.Controllers
{
    [Route("api/[controller]")]
    public class ScraperController : Controller
    {
	ILogger<ScraperController> _logger;

	static List<Scrape> _scrapes = new List<Scrape>{
		new Scrape("test url"){
		    Name = "Name1",
		    Progress = "10",
		    Eta = "01:20:35"
		},
		new Scrape("test url"){
		    Name = "Name2",
		    Progress = "10",
		    Eta = "01:20:35"
		},new Scrape("test url"){
		    Name = "Name3",
		    Progress = "10",
		    Eta = "01:20:35"
		},new Scrape("test url"){
		    Name = "Name4",
		    Progress = "10",
		    Eta = "01:20:35",
		    DateCompleted = "14:38:50 04/02/2016"
		}
	    };

	public ScraperController(ILogger<ScraperController> logger)
	{
	    _logger = logger;
	}

        [HttpGet]
        public IEnumerable<Scrape> GetAllScrapes()
        {
            return _scrapes;
        }
 
        [HttpGet("{scrapeId:int}", Name = "GetScrapeById")]
        public IActionResult GetScrapeById(int scrapeId)
        {
            return null;
        }

	[HttpPost]
	public IActionResult AddScrapePost(string url)
	{
	    if(string.IsNullOrEmpty(url))
	    {	
		return Content("Content is null or empty!");
	    }

	    _logger.LogInformation("New url: " + url);
	    _scrapes.Add(new Scrape(url){
		Name = "new name"
	    });
	    
	    return Content("Post Invoked: " + url);
	}
/*
[HttpPost]
public string PostSimple(string data)
{
    if (data != null)
    {
	//JObject jObject = JObject.Parse(data);
     ///var  url = JSON.Stringify(jObject);
var url = data;
      return "-OK-" + url;
    }
    else
    {
        return "bad";// Request.CreateResponse(HttpStatusCode.BadRequest);
    }
}
*/
    }
}