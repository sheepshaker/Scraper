using System;
using System.Diagnostics;
using System.IO;

namespace DownloaderUtil
{
    public class ScraperFailedEventArgs : EventArgs
    {
	public ScrapeReq ScrapeReq {get; set;}
	public string Message {get;set;}
    }
}
