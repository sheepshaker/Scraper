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
	    _scrapes.Add("1", new Scrape("new url"){IsDownloadFailed=true, DownloadFailedMessage="failed message", ProgressPercentage="30", BytesReceived="2300", FileSize="6900", Name="film name"});
	    _scrapes.Add("2", new Scrape("new url"){IsDownloadCanceled=true, ProgressPercentage="30", BytesReceived="2300", FileSize="6900", Name="film name"});
	    _scrapes.Add("3", new Scrape("new url"){IsDownloadCompleted=true, ProgressPercentage="30", BytesReceived="2300", FileSize="6900", Name="film name"});
	    _scrapes.Add("4", new Scrape("new url"){IsDownloadInProgress=true, ProgressPercentage="30", BytesReceived="2300", FileSize="6900", Name="film name"});
	    _scrapes.Add("5", new Scrape("new url"){IsScrapingInProgress=true});
	    _scrapes.Add("6", new Scrape("new url"){IsScrapingFailed=true, ScrapingFailedMessage="failed message"});

	    _hub = hub;
	    _logger = logger;

	    _downloader = DownloaderUtil.Downloader.Instance;
	    _downloader.WebDriverProgress += (s,e) => { _logger.LogInformation(e.Message); };
	    _downloader.WebDriverError += (s,e) => { _logger.LogError(e.Message); };
	    _downloader.DownloadCompleted += (s,e) => {
		//_logger.LogInformation("Progress");
		
		Scrape scrape = _scrapes[e.ScrapeDesc.Id];
		scrape.IsDownloadInProgress = false;
		scrape.IsDownloadCompleted = true;
		scrape.DownloadSpeed = e.ScrapeDesc.DownloadSpeed.ToString();
		scrape.ProgressPercentage = e.ScrapeDesc.ProgressPercentage.ToString();
		scrape.BytesReceived = e.ScrapeDesc.BytesReceived.ToString();
		scrape.FileSize = e.ScrapeDesc.FileSize.ToString();
		scrape.Eta = FormatTimeSpan(e.ScrapeDesc.Eta);
		scrape.SetToCompleted();

		var json = JsonConvert.SerializeObject(scrape);
		var job = JObject.Parse(json);
		_hub.Clients.All.broadcastScrapeUpdate(job);

		
	    };
	    _downloader.DownloadProgress += (s,e) => {
		//_logger.LogInformation("Progress");
	    
		Scrape scrape = _scrapes[e.ScrapeDesc.Id];
		scrape.IsDownloadInProgress = true;
		scrape.DownloadSpeed = e.ScrapeDesc.DownloadSpeed.ToString();
		scrape.ProgressPercentage = e.ScrapeDesc.ProgressPercentage.ToString();
		scrape.BytesReceived = e.ScrapeDesc.BytesReceived.ToString();
		scrape.FileSize = e.ScrapeDesc.FileSize.ToString();
		scrape.Eta = FormatTimeSpan(e.ScrapeDesc.Eta);

		var json = JsonConvert.SerializeObject(scrape);
		var job = JObject.Parse(json);
		_hub.Clients.All.broadcastScrapeUpdate(job);
	    };
	    _downloader.DownloadError += (s,e) => {
		_logger.LogError(e.Message);
		Scrape scrape = _scrapes[e.ScrapeDesc.Id];
		scrape.IsDownloadInProgress = false;		
		scrape.IsDownloadFailed = true;
		scrape.SetToCompleted();
		scrape.DownloadFailedMessage = e.Message;

		var json = JsonConvert.SerializeObject(scrape);
		var job = JObject.Parse(json);
		_hub.Clients.All.broadcastScrapeUpdate(job);
	     };	    
	    
	    _downloader.DownloadCanceled += (s,e) => {
		_logger.LogError(e.Message);
		Scrape scrape = _scrapes[e.ScrapeDesc.Id];
		scrape.IsDownloadInProgress = false;		
		scrape.IsDownloadCanceled = true;
		scrape.SetToCompleted();
		
		var json = JsonConvert.SerializeObject(scrape);
		var job = JObject.Parse(json);
		_hub.Clients.All.broadcastScrapeUpdate(job);
	     };
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

	private string FormatTimeSpan(TimeSpan span)
	{
	    return string.Format("{0:00}:{1:00}:{2:00}", System.Math.Abs(span.Hours), System.Math.Abs(span.Minutes), System.Math.Abs(span.Seconds));
	}
    }
}
