using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNet.SignalR;
using Scraper.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Scraper
{
    public class DownloadManager
    {
	static ILogger _logger;
	static DownloaderUtil.Downloader _downloader;
	static IHubContext _hub;
	static Dictionary<string, Scrape> _scrapes;

        public DownloadManager(ILogger logger, IHubContext hub)
        {
	    _scrapes = new Dictionary<string, Scrape>();
	    _hub = hub;
	    _logger = logger;

	    _downloader = DownloaderUtil.Downloader.Instance;
	    _downloader.WebDriverProgress += (s,e) => { _logger.LogInformation(e.Message); };
	    _downloader.WebDriverError += (s,e) => { _logger.LogError(e.Message); };
	    _downloader.DownloaderCompleted += (s,e) => { _logger.LogInformation("Progress"); };
	    _downloader.DownloaderProgress += (s,e) => { _logger.LogInformation("Progress"); };
	    _downloader.DownloaderError += (s,e) => { _logger.LogError(e.Message); };	    
	    
	    _downloader.ScraperCompleted += (s,e) => {
		_logger.LogInformation("Scraper completed: Name=" + e.ScrapeDesc.Name + ", Url=" + e.ScrapeDesc.DownloadUrl);
		
		Scrape scrape = _scrapes[e.ScrapeDesc.Id];
		scrape.Name = e.ScrapeDesc.Name;
		scrape.DownloadUrl = e.ScrapeDesc.DownloadUrl;
		scrape.IsScrapingInProgress = false;
		scrape.IsDownloadInProgress = true;		

		var json = JsonConvert.SerializeObject(scrape);
		var job = JObject.Parse(json);
		_hub.Clients.All.broadcastScrapeUpdate(job);
	     };

	    _downloader.ScraperFailed += (s,e) => {
		_logger.LogInformation("Scraper failed: " + e.Message);
		
		Scrape scrape = _scrapes[e.ScrapeReq.Id];
		scrape.IsScrapingInProgress = false;
		scrape.IsScrapingFailed = true;
		scrape.ScrapingFailedMessage = e.Message;

		var json = JsonConvert.SerializeObject(scrape);
		var job = JObject.Parse(json);
		_hub.Clients.All.broadcastScrapeUpdate(job);
	     };
	}

	public static void Go(Scrape scrape)
	{
	    scrape.IsScrapingInProgress = true;
	    _scrapes.Add(scrape.Id, scrape);
	    
	    var json = JsonConvert.SerializeObject(scrape);
	    _hub.Clients.All.broadcastScrapeAdded(JObject.Parse(json));
	    
	    _downloader.Go(scrape.Id, scrape.InputUrl);
	}

	public static IEnumerable<Scrape> GetScrapes()
	{
	    return _scrapes.Values;
	}
    }
}
