using System;
using System.Diagnostics;
using System.IO;

namespace DownloaderUtil
{
    public class BaseEventArgs : EventArgs
    {
	public ScrapeDesc ScrapeDesc {get;set;}
    }
}
