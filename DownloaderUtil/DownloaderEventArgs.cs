using System;
using System.Diagnostics;
using System.IO;

namespace DownloaderUtil
{
    public class DownloaderEventArgs : EventArgs
    {
	public ScrapeDesc ScrapeDesc {get;set;}
    }
}
