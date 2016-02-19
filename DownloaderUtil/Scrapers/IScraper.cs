using System;
using System.Diagnostics;
using System.IO;

namespace DownloaderUtil
{
    public interface IScraper
    {
	ScrapeDesc Scrape(ScrapeReq scrapeReq);
	void Stop();
	event EventHandler<MessageEventArgs> WebDriverProgress;
	event EventHandler<MessageEventArgs> WebDriverError;
    }
}
