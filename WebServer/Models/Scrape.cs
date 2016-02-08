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

	public Scrape(string url)
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
	    Url = url;
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
		if(IsCompleted || IsFailed || IsCanceled)
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
	public string Url {get; set;}
	public string DownloadedBytes {get{ return _downloadedBytes.ToString(); } }
	public string DownloadSize { get { return _downloadSize.ToString(); } }
	public string DownloadSpeed { get { return _downloadSpeed.ToString(); } }
	public bool IsFailed { get; set; }
	public bool IsCanceled { get; set; }
    	public bool IsCompleted { get; set; }

	public void SetProgress(decimal bytesDownloaded)
	{
	    if(_downloadedBytes + bytesDownloaded >= _downloadSize)
	    {
		//completed
		_dateCompleted = DateTime.Now;
		IsCompleted = true;
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
	    IsCanceled = true;
	    SetToCompleted();
	}

	public void Fail()
	{
	    IsFailed = true;
	    SetToCompleted();
	}	

	private void Download(string url, string outputFile)
	{
	}
    }
}
