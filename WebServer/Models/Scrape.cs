using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Scraper.Models
{
    public class Scrape
    {
	const string TempOutputPath = "/media/MEDIA/ScraperDownload/temp";
	const string OutputPath = "/media/MEDIA/ScraperDownload";	
	const string DateTimeFormat = "HH:mm:ss dd/MM/yyyy";
	
	DateTime _dateStarted;
	DateTime? _dateCompleted;
	decimal _downloadSize;
	decimal _downloadSpeed;
	decimal _downloadedBytes;

	public Scrape(string inputUrl)
	{
	    if(Directory.Exists(TempOutputPath) == false)
	    {
		Directory.CreateDirectory(TempOutputPath);
	    }

	    if(Directory.Exists(OutputPath) == false)
	    {
	    	Directory.CreateDirectory(OutputPath);
	    }

	    Id = Guid.NewGuid().ToString();
	    //DownloadUrl = downloadUrl;
	    InputUrl = inputUrl;
	    _dateStarted = DateTime.Now;
	    _downloadSize = 700000m;
	    _downloadSpeed = 1024m;
	    _downloadedBytes = 0;
	}
	
	public string Id { get; private set; }
	public string Name { get; set; }
	public string Progress { get { return System.Math.Round((_downloadedBytes*100/_downloadSize), 2).ToString(); } }
	public string Eta
	{
	    get
	    {
		if(IsDownloadCompleted || IsDownloadFailed || IsDownloadCanceled || IsScrapingInProgress || IsScrapingFailed)
		    return string.Empty;
		
		var span = TimeSpan.FromSeconds((double) (_downloadSize * 1000/ _downloadSpeed));
		return FormatTime(span);
	    } 
	}

	public string Elapsed 
	{
	    get
	    {
		if(_dateCompleted.HasValue == false)
		    return string.Empty;

		return FormatTime(_dateCompleted.Value - _dateStarted);
	    } 
	}

	public string DateStarted { get { return _dateStarted.ToString(DateTimeFormat); } }
	public string DateCompleted { get; set;}	
	public string DownloadUrl {get; set;}
	public string InputUrl {get; set;}
	public string DownloadedBytes {get{ return _downloadedBytes.ToString(); } }
	public string DownloadSize { get { return _downloadSize.ToString(); } }
	public string DownloadSpeed { get { return _downloadSpeed.ToString(); } }
	public bool IsDownloadFailed { get; set; }
	public string DownloadFailedMessage {get;set;}	
	public bool IsDownloadCanceled { get; set; }
    	public bool IsDownloadCompleted { get; set; }
	public bool IsDownloadInProgress { get; set; }
	public bool IsScrapingInProgress { get; set; }
	public bool IsScrapingFailed {get;set;}
	public string ScrapingFailedMessage {get;set;}	

	public void SetProgress(decimal bytesDownloaded)
	{
	    if(_downloadedBytes + bytesDownloaded >= _downloadSize)
	    {
		//completed
		_downloadedBytes = _downloadSize;
		_dateCompleted = DateTime.Now;
		IsDownloadCompleted = true;
		IsDownloadInProgress = false;
	    }
	    else
	    {
		_downloadedBytes += bytesDownloaded;
		_downloadSpeed = bytesDownloaded;
	    }
	}

	private string FormatTime(TimeSpan span)
	{
	    return string.Format("{0:00}:{1:00}:{2:00}", System.Math.Abs(span.Hours), System.Math.Abs(span.Minutes), System.Math.Abs(span.Seconds));
	}

	private void SetToCompleted()
	{
	    _dateCompleted = DateTime.Now;
	    _downloadSpeed = 0;
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
