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
	System.Threading.Timer _timer;

        public DownloadManager(ILogger logger, IHubContext hub)
        {
	    _timer = new System.Threading.Timer( ProgressProc, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
/*	    var list = new List<Scrape>();
	    list.Add(new Scrape("new url"){IsDownloadFailed=true, DownloadFailedMessage="failed message", ProgressPercentage="30", BytesReceived="2300", FileSize="6900", Name="film name"});
	    list.Add(new Scrape("new url"){IsDownloadCanceled=true, ProgressPercentage="30", BytesReceived="2300", FileSize="6900", Name="film name"});
	    list.Add(new Scrape("new url"){IsDownloadCompleted=true, ProgressPercentage="30", BytesReceived="2300", FileSize="6900", Name="film name"});
	    list.Add(new Scrape("new url"){IsDownloadInProgress=true, ProgressPercentage="30", BytesReceived="2300", FileSize="6900", Name="film name"});
	    list.Add(new Scrape("new url"){IsScrapingInProgress=true});
	    list.Add(new Scrape("new url"){IsScrapingFailed=true, ScrapingFailedMessage="failed message"});
	    
	    _scrapes = list.ToDictionary(l => l.Id);
*/
	    _scrapes = new Dictionary<string, Scrape>();	    
	    _hub = hub;
	    _logger = logger;

	    _downloader = DownloaderUtil.Downloader.Instance;
	    _downloader.WebDriverProgress += (s,e) => { _logger.LogInformation(e.Message); };
	    _downloader.WebDriverError += (s,e) => { _logger.LogError(e.Message); };
	    _downloader.DownloadCompleted += (s,e) => {
		//_logger.LogInformation("Progress");
		
		Scrape scrape;
		if(_scrapes.TryGetValue(e.ScrapeDesc.Id, out scrape))
		{
		    scrape.IsDownloadInProgress = false;
		    scrape.IsDownloadCompleted = true;
		    scrape.DownloadSpeed = e.ScrapeDesc.DownloadSpeed.ToString();
		    scrape.ProgressPercentage = e.ScrapeDesc.ProgressPercentage.ToString();
		    scrape.BytesReceived = e.ScrapeDesc.BytesReceived.ToString();
		    scrape.FileSize = e.ScrapeDesc.FileSize.ToString();
		    scrape.Eta = FormatTimeSpan(e.ScrapeDesc.Eta);

		    var json = JsonConvert.SerializeObject(scrape);
		    var job = JObject.Parse(json);
		    _hub.Clients.All.broadcastScrapeUpdate(job);
		}
		else
{
    _logger.LogError("completed");
}	    };
	    _downloader.DownloadProgress += (s,e) => {
		//_logger.LogInformation("Progress");
		
		Scrape scrape;
		if(_scrapes.TryGetValue(e.ScrapeDesc.Id, out scrape) && scrape.IsDownloadFailed == false && scrape.IsDownloadCanceled == false && scrape.IsDownloadCompleted == false)
		{
		    scrape.IsDownloadInProgress = true;
		    scrape.DownloadSpeed = e.ScrapeDesc.DownloadSpeed.ToString();
		    scrape.ProgressPercentage = e.ScrapeDesc.ProgressPercentage.ToString();
		    scrape.BytesReceived = e.ScrapeDesc.BytesReceived.ToString();
		    scrape.FileSize = e.ScrapeDesc.FileSize.ToString();
		    scrape.Eta = FormatTimeSpan(e.ScrapeDesc.Eta);
		    scrape.SetDownload(e.ScrapeDesc);
/*
		    var json = JsonConvert.SerializeObject(scrape);
		    var job = JObject.Parse(json);
		    _hub.Clients.All.broadcastScrapeUpdate(job);
*/		}
		else
{
    _logger.LogError("progress");
}
	    };
	    
	    _downloader.DownloadError += (s,e) => {
		_logger.LogError("Downloader reported error:");
		_logger.LogError(e.Message);
		
		Scrape scrape;
		if(_scrapes.TryGetValue(e.ScrapeDesc.Id, out scrape) && scrape.IsDownloadCanceled == false)
		{
		    scrape.IsDownloadInProgress = false;		
		    scrape.IsDownloadFailed = true;
		    scrape.DownloadFailedMessage = e.Message;

		    var json = JsonConvert.SerializeObject(scrape);
		    var job = JObject.Parse(json);
		    _hub.Clients.All.broadcastScrapeUpdate(job);
		}		else if(scrape.IsDownloadCanceled)
{
    _logger.LogError("downloaderror - actually canceled");
    var json = JsonConvert.SerializeObject(scrape);
    var job = JObject.Parse(json);
    _hub.Clients.All.broadcastScrapeUpdate(job);
}
	    };	    
	    
	    _downloader.DownloadCanceled += (s,e) => {
		_logger.LogError("Downloader reported Cancel");
		_logger.LogError(e.Message);

		Scrape scrape;
		if(_scrapes.TryGetValue(e.ScrapeDesc.Id, out scrape) && scrape.IsDownloadFailed == false)
		{
		    scrape.IsDownloadInProgress = false;		
		    scrape.IsDownloadCanceled = true;
		
		    var json = JsonConvert.SerializeObject(scrape);
		    var job = JObject.Parse(json);
		    _hub.Clients.All.broadcastScrapeUpdate(job);
		}		else
{
    _logger.LogError("canceled");
}
	    };
	    _downloader.ScraperCompleted += (s,e) => {
		_logger.LogInformation("Scraper completed: Name=" + e.ScrapeDesc.Name + ", Url=" + e.ScrapeDesc.DownloadUrl);
		
		Scrape scrape;
		if(_scrapes.TryGetValue(e.ScrapeDesc.Id, out scrape))
		{
		    scrape.Name = e.ScrapeDesc.Name;
		    scrape.DownloadUrl = e.ScrapeDesc.DownloadUrl;
		    scrape.IsScrapingInProgress = false;
		    scrape.IsDownloadInProgress = true;		

		    var json = JsonConvert.SerializeObject(scrape);
		    var job = JObject.Parse(json);
		    _hub.Clients.All.broadcastScrapeUpdate(job);
		} 		else
{
    _logger.LogError("scrapecompleted");
}
	    };

	    _downloader.ScraperFailed += (s,e) => {
		_logger.LogInformation("Scraper failed: " + e.Message);
		
		Scrape scrape;
		if(_scrapes.TryGetValue(e.ScrapeReq.Id, out scrape))
		{
		    scrape.IsScrapingInProgress = false;
		    scrape.IsScrapingFailed = true;
		    scrape.ScrapingFailedMessage = e.Message;

		    var json = JsonConvert.SerializeObject(scrape);
		    var job = JObject.Parse(json);
		    _hub.Clients.All.broadcastScrapeUpdate(job);
		} 		else
{
    _logger.LogError("scrapefailed");
}
	    };
	}

	private void ProgressProc(object status)
	{
	    //_logger.LogInformation("tick");
	    var coll = _scrapes.Values.Where(s => s.IsDownloadInProgress).ToArray();
	    //_logger.LogInformation("tick " + coll.Count() + " " + coll.FirstOrDefault()?.Id);
	    var json = JsonConvert.SerializeObject(coll);
	    var jarray = JArray.Parse(json);
	    _hub.Clients.All.broadcastScrapesUpdate(jarray);
	}

	public void Stop()
	{
	    _downloader.Stop();
	}

	public static void DeleteAll()
	{
	    foreach(var scrape in _scrapes.Values)
	    {
		if(scrape.IsDownloadInProgress)
		    scrape.Cancel();
	    }
    
	    _scrapes.Clear();
	}
	
	public static void Cancel(string scrapeId)
	{
	    Scrape scrape;
	    if(_scrapes.TryGetValue(scrapeId, out scrape))
	    {
		scrape.Cancel();		
	    }
	    else
	    {
		throw new Exception("Scrape Id not found");
	    }
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
