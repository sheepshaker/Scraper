using System;
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
	public event EventHandler<MessageEventArgs> WebDriverProgress;
	public event EventHandler<MessageEventArgs> WebDriverError;
	public event EventHandler<DownloaderEventArgs> DownloaderCompleted;
	public event EventHandler<DownloaderEventArgs> DownloaderProgress;
	public event EventHandler<MessageEventArgs> DownloaderError;
	public event EventHandler DownloaderCanceled;
	public event EventHandler<ScraperCompletedEventArgs> ScraperCompleted;
	public event EventHandler<ScraperFailedEventArgs> ScraperFailed;
	
	const string TempOutputPath = "/media/MEDIA/ScraperDownload/temp";
        const string OutputPath = "/media/MEDIA/ScraperDownload";

	IWebDriver _driver;
	string _name;
	Thread _downloadProc;
	Queue<ScrapeReq> _queue;
	bool _stop;

	private Downloader()
	{
	    var service = PhantomJSDriverService.CreateDefaultService();
	    var options = new PhantomJSOptions();
	    	
	    _driver = new PhantomJSDriver(service, options, TimeSpan.FromSeconds(30));

	    _queue  = new Queue<ScrapeReq>();
	    
	    _webClient = new WebClient();
	    _webClient.DownloadFileCompleted += Completed;
	    _webClient.DownloadProgressChanged += ProgressChanged;
		
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

        public void Go(string id, string inputUrl)
        {
	    if(CheckIfValidUrl(inputUrl) == false)
		throw new Exception("Invalid url");

	    _queue.Enqueue(new ScrapeReq{Id = id, InputUrl=inputUrl});
        }

	private ScrapeDesc Scrape(ScrapeReq scrapeReq)
	{
    	    if(_driver == null)
	    {
		SendProgress("WEB Driver is null!");
		
		throw new Exception();
	    }
	    
	    ScrapeDesc scrapeDesc = null;    
	    SendProgress("Scraping url: " + scrapeReq.InputUrl);
	        
            string linkLocation = string.Empty;
	    string name = string.Empty;
            DateTime startTime = DateTime.Now;
	    List<string> errors = new List<string>();

            try
            {

                SendProgress("Navigating...");
                _driver.Navigate().GoToUrl(scrapeReq.InputUrl);
                
                try
                {
		    name = _driver.FindElement(By.XPath("//span[@itemprop='name']")).Text;
                    SendProgress("Found name: " + name);
		    IWebElement query = _driver.FindElement(By.XPath("//div[@class='player-wrapper']/a"));
                    SendProgress("Click 1");
                    query.Click();
                    SendProgress("OK");
                }
                catch (Exception ex)
                {
		    errors.Add("Error1");
                    SendError("Error1", ex.ToString(), _driver?.PageSource);
                }

                WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
                var element = wait.Until((d) => { return d.FindElement(By.XPath("//button[@class='btn btn-primary']")); });

                try
                {
                    SendProgress("Click 2");
                    //var html = element.GetAttribute("outerHTML");
                    //html = element.GetAttribute("innerHTML");
                    element.Click();
                    SendProgress("OK");
                }
                catch (Exception ex)
                {
                    SendError("Warning", ex.ToString(), _driver?.PageSource);
                }

                try
                {
                    wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
                    element = wait.Until((d) => { return d.FindElement(By.ClassName("player-wrapper")); });
                    var temp = element;
                    var iframe = wait.Until((d) => { return temp.FindElement(By.TagName("iframe")); });
                    var link = iframe.GetAttribute("src");

                    SendProgress("checking if link is valid: " + link);

                    if (CheckIfValidUrl(link))
                    {
                        SendProgress("Link is OK. Navigating...");
                        _driver.Navigate().GoToUrl(link);
                    }
                    else
                    {
                        SendProgress("Invalid link...");
                        throw new Exception("Invalid link");
                    }
                }
                catch (Exception ex)
                {
		    errors.Add("Error2");
                    SendError("Error2", ex.ToString(), _driver?.PageSource);
                }

                wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
                var tmpUrl = wait.Until(d => { return d.FindElement(By.XPath("//div[@id='playerVidzer']/a")).GetAttribute("href"); });

                int index = tmpUrl.LastIndexOf("http", StringComparison.InvariantCulture);
                SendProgress("Got link: " + tmpUrl);

                if (index > 0)
                {
                    SendProgress("Link corrupted, fixing...");

                    tmpUrl = tmpUrl.Remove(0, index);
                }
                else
                {
                    SendProgress("Link not corrupted - OK");
                }

                SendProgress("UrlDecode...");

                tmpUrl = System.Net.WebUtility.UrlDecode(tmpUrl);

                SendProgress("Check if link is valid: " + tmpUrl);
                if (CheckIfValidUrl(tmpUrl))
                {
                    linkLocation = tmpUrl;
                    SendProgress("link OK");
		    scrapeDesc = new ScrapeDesc{Name=name, DownloadUrl=linkLocation, Id=scrapeReq.Id};
                }
                else
                {
                    throw new Exception("Invalid link");
                }
            }
            catch (Exception ex)
            {
		errors.Add("Error3");
                SendError("Error3", ex.ToString(), _driver?.PageSource);
            }
            
            SendProgress("\nlink : \n" + linkLocation == string.Empty ? "link not found" : linkLocation);
            SendProgress("total time = " + new DateTime((DateTime.Now - startTime).Ticks).ToString("HH:mm:ss"));

	    if(scrapeDesc != null)
		ScraperCompleted?.Invoke(this, new ScraperCompletedEventArgs{ ScrapeDesc = scrapeDesc });
	    else
		ScraperFailed?.Invoke(this, new ScraperFailedEventArgs{ ScrapeReq = scrapeReq, Message = string.Join(",", errors.ToArray()) });

	    return scrapeDesc;
	}

	WebClient _webClient = new WebClient();

	private void Download(ScrapeDesc scrapeDesc)
	{
	    SendProgress("Downloading " + scrapeDesc.Name + " from " + scrapeDesc.DownloadUrl);
	    
	    scrapeDesc.Stopwatch = new Stopwatch();

	    //using(scrapeDesc.WebClient = new WebClient())
	    {
		try
		{
		    scrapeDesc.Stopwatch.Start();
		    _webClient.DownloadFileAsync(scrapeDesc.DownloadUrl, TempOutputPath + "//" + scrapeDesc.Name, scraperDesc);
		}
		catch(Exception ex)
		{
		    DownloaderError?.Invoke(this, new MessageEventArgs{Message=ex.Message});
		}
	    }
	}

	private void Completed(object sender, AsyncCompletedEventArgs e)
	{ 
	    if (e.Error != null)
	    {
    		string error = e.Error.ToString();
    		DownloaderError?.Invoke(this, new MessageEventArgs{Message=error});
    		return;
	    }
    	    if (e.Cancelled == true)
	    {
		DownloaderCanceled?.Invoke(this, EventArgs.Empty);
	    }
	    else
	    {
		ScrapeDesc scrapeDesc = (ScrapeDesc) e.UserState;

	    	scrapeDesc.DownloadSpeed = e.BytesReceived / scrapeDesc.Stopwatch.Elapsed.TotalSeconds;
	        scrapeDesc.ProgressPercentage = e.ProgressPercentage;
    		scrapeDesc.BytesReceived = e.BytesReceived;
		scrapeDesc.FileSize = e.TotalBytesToReceive;

		DownloaderProgress?.Invoke(this, new DownloaderEventArgs{ScrapeDesc = scrapeDesc});
	    }
	    
	    // Stop the stopwatch.
	    sw.Stop();

	}

	private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
	{
	    ScrapeDesc scrapeDesc = (ScrapeDesc) e.UserState;
	    
	    scrapeDesc.DownloadSpeed = e.BytesReceived / scrapeDesc.Stopwatch.Elapsed.TotalSeconds;

	    scrapeDesc.ProgressPercentage = e.ProgressPercentage;
 
	    scrapeDesc.BytesReceived = e.BytesReceived;

	    scrapeDesc.FileSize = e.TotalBytesToReceive;

	    DownloaderProgress?.Invoke(this, new DownloaderEventArgs{ScrapeDesc = scrapeDesc});
	}

	public void Close()
	{
	    _driver?.Quit();
	}

        void SendProgress(string str)
        {
    	    WebDriverProgress?.Invoke(this, new MessageEventArgs
	    {
		Message = str
	    });
	    //_logger.LogError(str);
	}

        void SendError(string param, string str, string pageSource)
        {
	    WebDriverError?.Invoke(this, new MessageEventArgs{
		Message = param + ": " + str
	    });
	}

        bool CheckIfValidUrl(string link)
        {
            Uri uriResult;
            return Uri.TryCreate(link, UriKind.Absolute, out uriResult) && uriResult.Scheme == Uri.UriSchemeHttp;
        }

	public void Stop()
	{
	    _stop = true;
	}

	private void DownloadProc()
	{
	    //SendProgress("Downloader thread is running " + Thread.CurrentThread.ManagedThreadId);
	    
	    while(_stop == false)
	    {
		//SendProgress("+SIZE = " + _queue.Count + " " +  Thread.CurrentThread.ManagedThreadId);
		
		if(_queue.Count > 0)
		{
		    
		    var scrapeReq = _queue.Dequeue();
		    //SendProgress("dequeued SIZE = " + _queue.Count + " " +  Thread.CurrentThread.ManagedThreadId);
		    var scrapeDesc = Scrape(scrapeReq);
		    if(scrapeDesc != null)
			Download(scrapeDesc);
		    
		    //_downloader.Go(url);
		    //_scrapes.Add(new Scrape(url));
		}
		else
		{
		    Thread.Sleep(1000);
		}

		//SendProgress("-SIZE = " + _queue.Count + " " +  Thread.CurrentThread.ManagedThreadId);
	    }

	    SendProgress("Downloader thread terminated");
	}
    }

    public class ScrapeReq
    {
	public string Id{get;set;}
	public string InputUrl {get;set;}
    }
	
    public class ScrapeDesc
    {
	public string Id {get;set;}
	public string Name {get;set;}
	public string DownloadUrl {get;set;}
	
	public Stopwatch Stopwatch {get;set;}
	
	public decimal DownloadSpeed;
	public decimal ProgressPercentage; 
	public decimal BytesReceived;
	public decimal FileSize;
    }
}
