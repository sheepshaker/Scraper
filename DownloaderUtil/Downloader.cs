using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Diagnostics;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;
using Microsoft.Extensions.Logging;
using System.Net;
using System.ComponentModel;

namespace DownloaderUtil
{
    public sealed class Downloader
    {
	public event EventHandler<ScraperCompletedEventArgs> ScraperCompleted;
	public event EventHandler<MessageEventArgs> ScraperProgress;
	public event EventHandler<ScraperFailedEventArgs> ScraperFailed;
	public event EventHandler<DownloadEventArgs> DownloadCompleted;
	public event EventHandler<DownloadEventArgs> DownloadProgress;
	public event EventHandler<MessageEventArgs> DownloadError;
	public event EventHandler<MessageEventArgs> DownloadCanceled;
	
	const string TempOutputPath = "/media/MEDIA/ScraperDownload/temp";
        const string OutputPath = "/media/MEDIA/ScraperDownload";

	Dictionary<string, IScraper> _scrapers;
	IWebDriver _driver;
	Thread _downloadProc;
	Queue<ScrapeReq> _queue;
	bool _stop;

	private Downloader()
	{	    
            if(Directory.Exists(TempOutputPath) == false)
            {
                Directory.CreateDirectory(TempOutputPath);
            }

            if(Directory.Exists(OutputPath) == false)
            {
                Directory.CreateDirectory(OutputPath);
            }

	    var service = PhantomJSDriverService.CreateDefaultService();
	    var options = new PhantomJSOptions();
	    	
	    _driver = new PhantomJSDriver(service, options, TimeSpan.FromSeconds(30));

	    _queue  = new Queue<ScrapeReq>();
	    
	    _scrapers = new Dictionary<string, IScraper>();
	    _scrapers.Add(KinomanScraper.Key, new KinomanScraper());
	    _scrapers.Add(WatchSeriesScraper.Key, new WatchSeriesScraper());
	    _scrapers.Add(WatchFreeScraper.Key, new WatchFreeScraper());

	    foreach(var scraper in _scrapers.Values)
	    {
		scraper.WebDriverError += OnWebDriverProgress;
		scraper.WebDriverProgress += OnWebDriverProgress;
	    }

	    _downloadProc = new Thread(DownloadProc);
	    _downloadProc.Start();	    	
    	}

	public static Downloader Instance { get { return Nested.instance; } }
    
	private class Nested
	{
	    // Explicit static constructor to tell C# compiler
	    // not to mark type as beforefieldinit
	    static Nested()
	    {
	    }
    	    internal static readonly Downloader instance = new Downloader();
	}

        public void Go(string id, string inputUrl, bool goDirect = false)
        {
	    ScrapeReq scrapeReq = new ScrapeReq{Id = id, InputUrl=inputUrl, GoDirect=goDirect};

	    if(CheckIfValidUrl(inputUrl) == false)
	    {
		ScraperFailed?.Invoke(this, new ScraperFailedEventArgs{ScrapeReq=scrapeReq, Message="Invalid url"});
    	    }
	    else
	    {	
		_queue.Enqueue(scrapeReq);
	    }
        }
	
        void OnWebDriverProgress(object source, MessageEventArgs e)
        {
    	    ScraperProgress?.Invoke(source, e);
	}

        bool CheckIfValidUrl(string link)
        {
            Uri uriResult;
            return Uri.TryCreate(link, UriKind.Absolute, out uriResult) && uriResult.Scheme == Uri.UriSchemeHttp;
        }

	public void Stop()
	{
	    _stop = true;
	    _driver?.Quit();
	    
	    foreach(var scraper in _scrapers.Values)
	    {
		scraper.WebDriverError -= OnWebDriverProgress;
		scraper.WebDriverProgress -= OnWebDriverProgress;
	    }
	}

	private void DownloadProc()
	{
	    while(_stop == false)
	    {
		if(_queue.Count > 0)
		{
		    var scrapeReq = _queue.Dequeue();
		    ScrapeDesc scrapeDesc = null;
		    if(scrapeReq.GoDirect == false)
		    {
			foreach(var scraperKey in _scrapers.Keys)
			{
			    if(scrapeReq.InputUrl.StartsWith(scraperKey))
			    {
				scrapeDesc = _scrapers[scraperKey].Scrape(scrapeReq);
				break;
			    }
			}

			if(scrapeDesc != null)
			    ScraperCompleted?.Invoke(this, new ScraperCompletedEventArgs{ ScrapeDesc = scrapeDesc });
			else
			    ScraperFailed?.Invoke(this, new ScraperFailedEventArgs{ ScrapeReq = scrapeReq, Message = "Scraper Failed" });			    
		    }
		    else
		    {	
			scrapeDesc = new ScrapeDesc{Name=Path.GetRandomFileName(), DownloadUrl=scrapeReq.InputUrl};
		    }
			
		    if(scrapeDesc != null)
		    {
			scrapeDesc.WebClient.DownloadFileCompleted += Completed;
			scrapeDesc.WebClient.DownloadProgressChanged += ProgressChanged;
			Download(scrapeDesc);
		    }
		}
		else
		{
		    Thread.Sleep(1000);
		}
	    }
	}

	private void Download(ScrapeDesc scrapeDesc)
	{
	    try
	    {
		scrapeDesc.FilePath = TempOutputPath + "//" + scrapeDesc.Name;
		scrapeDesc.Stopwatch.Start();
		scrapeDesc.WebClient.DownloadFileAsync(new Uri(scrapeDesc.DownloadUrl), scrapeDesc.FilePath, scrapeDesc);
	    }
	    catch(Exception ex)
	    {
		DownloadError?.Invoke(this, new MessageEventArgs{Message=ex.Message});
		scrapeDesc.WebClient.Dispose();
	    }
	}
	
	private void Completed(object sender, AsyncCompletedEventArgs e)
	{
	    ScrapeDesc scrapeDesc = (ScrapeDesc)e.UserState;
	    
	    scrapeDesc.WebClient.DownloadFileCompleted -= Completed;
	    scrapeDesc.WebClient.DownloadProgressChanged -= ProgressChanged;
	    
	    if (e.Error != null)
	    {
    		string error = e.Error.ToString();
    		DownloadError?.Invoke(this, new MessageEventArgs{ScrapeDesc = scrapeDesc, Message=error});
    		return;
	    }
    	    else if (e.Cancelled == true)
	    {
		DownloadCanceled?.Invoke(this, new MessageEventArgs{ScrapeDesc = scrapeDesc, Message="Canceled"});
	    }
	    else
	    {
		try	
		{
	    	    File.Move(scrapeDesc.FilePath, OutputPath + "//" + scrapeDesc.Name + ".mp4");
		    DownloadCompleted?.Invoke(this, new DownloadEventArgs{ScrapeDesc = scrapeDesc});
		}
		catch(Exception ex)
		{
		    DownloadError?.Invoke(this, new MessageEventArgs{ScrapeDesc = scrapeDesc, Message=ex.ToString()});
		}		
	    }
	    
	    scrapeDesc.Stopwatch.Stop();
	    scrapeDesc.WebClient.Dispose();
	}

	private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
	{	
	    ScrapeDesc scrapeDesc = (ScrapeDesc)e.UserState;
	    
	    scrapeDesc.DownloadSpeed = (decimal)(e.BytesReceived / scrapeDesc.Stopwatch.Elapsed.TotalSeconds);

	    scrapeDesc.ProgressPercentage = e.ProgressPercentage;
 
	    scrapeDesc.BytesReceived = e.BytesReceived;

	    scrapeDesc.FileSize = e.TotalBytesToReceive;

	    scrapeDesc.Eta = TimeSpan.FromSeconds((double) (scrapeDesc.FileSize / scrapeDesc.DownloadSpeed));

	    //if(e.BytesReceived % scrapeDesc.DownloadSpeed == 0
		DownloadProgress?.Invoke(this, new DownloadEventArgs{ScrapeDesc = scrapeDesc});
	}
    }

    public class ScrapeReq
    {
	public string Id{get;set;}
	public string InputUrl {get;set;}
	public bool GoDirect{get;set;}
    }
	
    public class ScrapeDesc:ScrapeReq
    {
	public ScrapeDesc()
	{
	    Stopwatch = new Stopwatch();
	    WebClient = new WebClient();
	}

	public string DownloadUrl {get;set;}
	public string Name {get;set;}
	public Stopwatch Stopwatch {get;set;}
	public WebClient WebClient {get;set;}
	public decimal DownloadSpeed{get;set;}
	public decimal ProgressPercentage{get;set;} 
	public decimal BytesReceived{get;set;}
	public decimal FileSize{get;set;}
	public TimeSpan Eta{get;set;}
	public string FilePath{get;set;}		
    }
}
