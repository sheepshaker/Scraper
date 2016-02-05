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
	    DateStarted = DateTime.Now.ToString(DateTimeFormat);
	    Url = url;
	}
	
	public string Id { get; private set; }
	public string Name { get; set; }
	public string Progress { get; set; }
	public string Eta { get; set; }
	public string DateStarted { get; set;}
	public string DateCompleted { get; set;}	
	public string Url {get; set;}	
	public string DownloadSize { get; set; }
	public string DownloadSpeed { get; set; }
	public bool IsFailed { get; set; }

	public void Cancel()
	{
	}

	private void Download(string url, string outputFile)
	{
	}
    }
}
