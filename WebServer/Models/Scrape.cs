using System;
using DownloaderUtil;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Scraper.Models
{
    public class Scrape
    {
	const string DateTimeFormat = "HH:mm:ss dd/MM/yyyy";
	
	DateTime _dateStarted;
	DateTime _dateCompleted;
	
	public Scrape(string inputUrl)
	{
	    Id = Guid.NewGuid().ToString();
	    InputUrl = inputUrl;
	    _dateStarted = DateTime.Now;
	}
	
	public string Id { get; private set; }
	public string Name { get; set; }
	public string Eta
	{
	    get;set;
	}

	public string Elapsed 
	{
	    get
	    {
		var elapsed = string.Empty;
		if(IsDownloadInProgress)
		    elapsed = FormatTimeSpan(DateTime.Now - _dateStarted); 
		else
		    elapsed =  FormatTimeSpan(_dateCompleted - _dateStarted);
		
		return elapsed;
	    } 
	}

	public string DateStarted { get { return _dateStarted.ToString(DateTimeFormat); } }
	public string DateCompleted { get{return _dateCompleted.ToString(DateTimeFormat);} }	
	public string DownloadUrl {get; set;}
	public string InputUrl {get; set;}
	public string BytesReceived {get;set; }
	public string FileSize { get;set; }
	public string DownloadSpeed { get;set; }
	public string ProgressPercentage { get;set; }
	public bool IsDownloadFailed { get; set; }
	public string DownloadFailedMessage {get;set;}	
	public bool IsDownloadCanceled { get; set; }
    	public bool IsDownloadCompleted { get; set; }
	public bool IsDownloadInProgress { get; set; }
	public bool IsScrapingInProgress { get; set; }
	public bool IsScrapingFailed {get;set;}
	public string ScrapingFailedMessage {get;set;}

	public static string FormatTimeSpan(TimeSpan span)
	{
	    return string.Format("{0:00}:{1:00}:{2:00}", System.Math.Abs(span.Hours), System.Math.Abs(span.Minutes), System.Math.Abs(span.Seconds));
	}

	public void SetCompleteDate(DateTime dateTime)
	{
	    _dateCompleted = dateTime;
	}

	private ScrapeDesc _scrapeDesc;
	public void SetDownload(ScrapeDesc scrapeDesc)
	{
	    _scrapeDesc = scrapeDesc;
	}

	public void Cancel()
	{
	    if(IsDownloadInProgress)
	    {
		IsDownloadInProgress = false;
		IsDownloadCanceled = true;
		try
		{
		    _scrapeDesc?.WebClient.CancelAsync();
		}
		catch(Exception ex)
		{
		    throw new Exception("cancel exception! " + ex);
		}
	    }
	    else
	    {
		throw new Exception("Scrape is not in progress");
	    }
	}	

	private void Download(string url, string outputFile)
	{
	}
    }
}
