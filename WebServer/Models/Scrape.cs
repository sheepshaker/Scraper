using System;
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
		return FormatTimeSpan(_dateCompleted - _dateStarted);
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
	
	public void SetToCompleted()
	{
	    //IsDownloadInProgress = false;
	    _dateCompleted = DateTime.Now;
	    //DownloadSpeed = "0";
	}

	public void Cancel()
	{
	    IsDownloadCanceled = true;
	    SetToCompleted();
	}

	public void Fail()
	{
	    IsDownloadFailed = true;
	    SetToCompleted();
	}	

	private void Download(string url, string outputFile)
	{
	}
    }
}
