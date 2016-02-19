using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;

namespace DownloaderUtil
{
    public abstract class ScraperBase : IScraper
    {
	protected static IWebDriver _driver;
	public abstract ScrapeDesc Scrape(ScrapeReq scrapeReq);
	public event EventHandler<MessageEventArgs> WebDriverProgress;
        public event EventHandler<MessageEventArgs> WebDriverError;

	public ScraperBase()
	{
	    if(_driver == null)
	    {
		var service = PhantomJSDriverService.CreateDefaultService();
		var options = new PhantomJSOptions();
		_driver = new PhantomJSDriver(service, options, TimeSpan.FromSeconds(30));
	    }
	}

        protected bool CheckIfValidUrl(string link)
        {
            Uri uriResult;
            return Uri.TryCreate(link, UriKind.Absolute, out uriResult) && uriResult.Scheme == Uri.UriSchemeHttp;
        }

        public virtual void Stop()
	{
            _driver?.Quit();
        }
        
	protected void SendProgress(string str)
        {
            WebDriverProgress?.Invoke(this, new MessageEventArgs
            {
                Message = str
            });
        }

        protected void SendError(string param, string str, string pageSource)
        {
            WebDriverError?.Invoke(this, new MessageEventArgs{
                Message = param + ": " + str
            });
        }
    
	protected string CleanFileName(string fileName)
	{
	    return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
	}
    }
}
