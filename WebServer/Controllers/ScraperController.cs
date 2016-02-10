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
    [Route("api/[controller]")]
    public class ScraperController : Controller
    {
	ILogger<ScraperController> _logger;
	
	//static List<Scrape> _scrapes = new List<Scrape>();
	/*{
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
	    };*/

	//static System.Threading.Timer _timer;
	//static Random _random;
 
	public ScraperController(ILogger<ScraperController> logger)
	{
	    _logger = logger;
	    if(_logger == null)
		throw new Exception("logger is null");
	    
	    //if(_timer == null)
	    //{
		//_timer = new System.Threading.Timer(Simulate);
		//_timer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(1));
	    //}

	    //if(_random == null)
	    //{
		//_random = new Random();
	    //}

	    //DownloaderUtil.Downloader._logger = _logger;
	    //_downloader = DownloaderUtil.Downloader.Instance;
            /*_downloader.WebDriverProgress += OnMessage;
            _downloader.WebDriverError += OnError;
            _downloader.DownloaderProgress += OnDownloaderProgress;
            _downloader.DownloaderError += OnError;
	*/}

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
	
	[HttpDelete]
        public string DeleteAll()
        {
	    //_scrapes.Clear();
	    DownloaderUtil.Downloader.Instance.Stop();
	    var hub = GlobalHost.ConnectionManager.GetHubContext<Scraper.Hubs.ChatHub>();
	    hub.Clients.All.broadcastDeleteAll();
	    	    
	    return "Delete All - OK";
        }
/*
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
*//*
	private void OnMessage(object sender, MessageEventArgs e)
	{
	    _logger.LogInformation(e.Message);
	}

	private void OnDownloaderProgress(object sender, DownloaderEventArgs e)
	{
	    _logger.LogError("Progress");
	}

	private void OnError(object sender, MessageEventArgs e)
	{
	    _logger.LogError(e.Message);
	}
*/
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