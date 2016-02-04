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
	    DateStarted = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
	}
	
	public string Id { get; private set; }
	public string Name { get; set; }
	public string Progress { get; set; }
	public string Eta { get; set; }
	public string DateStarted {get;set;}
	public string DateCompleted {get;set;}	

	public void Cancel()
	{
	}

	private void Download(string url, string outputFile)
	{
	}
    }
}
