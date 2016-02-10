using System;
using System.Diagnostics;
using System.IO;

namespace DownloaderUtil
{
    public class DownloadFailedEventArgs : EventArgs
    {
	public ScrapeReq ScrapeReq {get; set;}
	public string ErrorMessage {get;set;}
    }
}
