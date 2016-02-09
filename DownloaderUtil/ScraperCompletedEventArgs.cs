using System;
using System.Diagnostics;
using System.IO;

namespace DownloaderUtil
{
    public class ScraperCompletedEventArgs : EventArgs
    {
	public ScrapeDesc ScrapeDesc {get; set;}
    }
}
