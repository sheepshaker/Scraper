using System;
using System.Threading;
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
    [Route("api/[controller]/[action]")]
    public class ScraperController : Controller
    {
	ILogger<ScraperController> _logger;
	
	public ScraperController(ILogger<ScraperController> logger)
	{
	    _logger = logger;
	    if(_logger == null)
		throw new Exception("logger is null");
	}

        [HttpGet]
        public IEnumerable<Scrape> GetAllScrapes()
        {
            return DownloadManager.GetScrapes();//_scrapes;
        }
 
        [HttpGet("{scrapeId:int}", Name = "GetScrapeById")]
        public IActionResult GetScrapeById(int scrapeId)
        {
            return null;
        }

	[HttpPost]
	public IActionResult AddScrapePost(string inputUrl)
	{
	    if(string.IsNullOrEmpty(inputUrl))
	    {	
		return Content("Content is null or empty!");
	    }

	    _logger.LogInformation("New url: " + inputUrl);
	    
	    Scrape scrape = new Scrape(inputUrl);
	    DownloadManager.Go(scrape);
	    
	    return Content("Download enqueued for " + scrape.InputUrl + ", id=" + scrape.Id );
	}

	[HttpPost]
	public IActionResult CancelScrapePost(string scrapeId)
	{
	    if(string.IsNullOrEmpty(scrapeId))
	    {	
		return Content("Content is null or empty!");
	    }

	    _logger.LogInformation("Cancel id: " + scrapeId);
	    
	    try
	    {
		DownloadManager.Cancel(scrapeId);
	    }
	    catch(Exception ex)
	    {
		_logger.LogError("Error: " + ex.Message);
		return Content("Error: " + ex.Message);
	    }

	    _logger.LogInformation("Cancel done for: " + scrapeId);    
	    
	    return Content("Cancel done for " + scrapeId);
	}
	
	[HttpDelete]
        public string DeleteAll()
        {
	    //_scrapes.Clear();
	    //DownloaderUtil.Downloader.Instance.Stop();
	    DownloadManager.DeleteAll();
	    var hub = GlobalHost.ConnectionManager.GetHubContext<Scraper.Hubs.ChatHub>();
	    hub.Clients.All.broadcastDeleteAll();
	    	    
	    return "Delete All - OK";
        }

	protected override void Dispose(bool disposing) 
	{
	    /*_logger.LogInformation("Disposing the controller!");
	    _downloader.WebDriverProgress -= OnMessage;
            _downloader.WebDriverError -= OnError;
            _downloader.DownloaderProgress -= OnDownloaderProgress;
            _downloader.DownloaderError -= OnError;
	    */
	    base.Dispose(disposing);
	}
    }
}