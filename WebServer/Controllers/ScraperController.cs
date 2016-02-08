using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using DownloaderUtil;
using Scraper.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.AspNet.SignalR;

namespace Scraper.Controllers
{
    [Route("api/[controller]")]
    public class ScraperController : Controller
    {
	ILogger<ScraperController> _logger;

	static List<Scrape> _scrapes = new List<Scrape>{
		new Scrape("test url"){
		    Name = "Name1",
		},
		new Scrape("test url"){
		    Name = "Name2",
		},new Scrape("test url"){
		    Name = "Name3",
		},new Scrape("test url"){
		    Name = "Name4",
		    DateCompleted = "14:38:50 04/02/2016",
		    IsCompleted = true
		}
	    };

	static System.Threading.Timer _timer;
	static Random _random;
 
	public ScraperController(ILogger<ScraperController> logger)
	{
	    _logger = logger;

	    if(_timer == null)
	    {
		_timer = new System.Threading.Timer(Simulate);
		_timer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(1));
	    }

	    if(_random == null)
	    {
		_random = new Random();
	    }
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
	    
	    var scrape = new Scrape(url){
		Name = "new name"
	    };

	    _scrapes.Add(scrape);

	    var hub = GlobalHost.ConnectionManager.GetHubContext<Scraper.Hubs.ChatHub>();
	    
	    var json = JsonConvert.SerializeObject(scrape);
	    hub.Clients.All.broadcastScrapeAdded(JObject.Parse(json));
	    
	    _logger.LogInformation("json = " + json.ToString());
	    return Content("Post Invoked: " + url);
	    
	}
	
	[HttpDelete]
        public string DeleteAll()
        {
	    _scrapes.Clear();
            
	    var hub = GlobalHost.ConnectionManager.GetHubContext<Scraper.Hubs.ChatHub>();
	    hub.Clients.All.broadcastDeleteAll();
	    	    
	    return "Delete All - OK";
        }

	private void Simulate(object state)
	{
	    //_logger.LogInformation(DateTime.Now.ToString("HH:mm:ss"));
	    //_timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

	    var hub = GlobalHost.ConnectionManager.GetHubContext<Scraper.Hubs.ChatHub>();
	    List<Scrape> changedList = new List<Scrape>();

	    foreach(var scrape in _scrapes)
	    {
		if(scrape.IsCompleted == false)
		{
		    var progress = GetProgress();
		    scrape.SetProgress(progress);
		    changedList.Add(scrape);
		    //_logger.LogInformation(_scrapes.IndexOf(scrape) + " P=" + scrape.Progress + ", S=" + scrape.DownloadSpeed);
		}
	    }

	    //_logger.LogInformation("updating scrapes: " + changedList.Count());
	    
	    if(changedList.Any())
	    {
		try{
		    var json = JsonConvert.SerializeObject(changedList.ToArray());
		    var jarray = JArray.Parse(json);
		    //_logger.LogInformation("jarray.Count = " + jarray.Count);
		    //string str = JsonConvert.SerializeObject(changedList);
		    hub.Clients.All.broadcastScrapeUpdate(jarray);
		}
		catch(Exception ex)
		{
		    _logger.LogInformation("Exception: " + ex.Message);
		}
		finally{
		    //_timer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(1));
		}
	    }
	}
	
	private int GetProgress()
	{
	    return _random.Next(512, 2048);
	}
    }
}