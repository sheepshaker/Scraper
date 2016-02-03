using System;
using System.Diagnostics;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;
using Microsoft.Extensions.Logging;

namespace DownloaderUtil
{
    public sealed class Downloader
    {
	public event EventHandler<MessageEventArgs> WebDriverProgress;
	public event EventHandler<MessageEventArgs> WebDriverError;
	public event EventHandler<DownloaderEventArgs> DownloaderProgress;
	public event EventHandler<MessageEventArgs> DownloaderError;

	IWebDriver _driver;
	
	private Downloader()
	{
	    var service = PhantomJSDriverService.CreateDefaultService();
	    var options = new PhantomJSOptions();
	    	
	    _driver = new PhantomJSDriver(service, options, TimeSpan.FromSeconds(30));
	}

	public static Downloader Instance { get { return Nested.instance; } }
    
	private class Nested
	{
	    // Explicit static constructor to tell C# compiler
	    // not to mark type as beforefieldinit
	    static Nested()
	    {
	    }
    	    internal static readonly Downloader instance = new Downloader();
	}

        public void Go(string downloadUrl)
        {
	    if(_driver == null)
	    {
		SendProgress("Driver is null!");
		
		throw new Exception();
	    }
	    
	    SendProgress("Scraping url: " + downloadUrl);
	        
            string linkLocation = string.Empty;
            DateTime startTime = DateTime.Now;

            try
            {

                SendProgress("Navigating...");
                _driver.Navigate().GoToUrl(downloadUrl);
                
                try
                {
                    IWebElement query = _driver.FindElement(By.XPath("//div[@class='player-wrapper']/a"));
                    SendProgress("Click 1");
                    query.Click();
                    SendProgress("OK");
                }
                catch (Exception ex)
                {
                    SendError("Error1", ex.ToString(), _driver?.PageSource);
                }

                WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
                var element = wait.Until((d) => { return d.FindElement(By.XPath("//button[@class='btn btn-primary']")); });

                try
                {
                    SendProgress("Click 2");
                    //var html = element.GetAttribute("outerHTML");
                    //html = element.GetAttribute("innerHTML");
                    element.Click();
                    SendProgress("OK");
                }
                catch (Exception ex)
                {
                    SendError("Warning", ex.ToString(), _driver?.PageSource);
                }

                try
                {
                    wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
                    element = wait.Until((d) => { return d.FindElement(By.ClassName("player-wrapper")); });
                    var temp = element;
                    var iframe = wait.Until((d) => { return temp.FindElement(By.TagName("iframe")); });
                    var link = iframe.GetAttribute("src");

                    SendProgress("checking if link is valid: " + link);

                    if (CheckIfValidUrl(link))
                    {
                        SendProgress("Link is OK. Navigating...");
                        _driver.Navigate().GoToUrl(link);
                    }
                    else
                    {
                        SendProgress("Invalid link...");
                        throw new Exception("Invalid link");
                    }
                }
                catch (Exception ex)
                {
                    SendError("Error2", ex.ToString(), _driver?.PageSource);
                }

                wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
                var tmpUrl = wait.Until(d => { return d.FindElement(By.XPath("//div[@id='playerVidzer']/a")).GetAttribute("href"); });

                int index = tmpUrl.LastIndexOf("http", StringComparison.InvariantCulture);
                SendProgress("Got link: " + tmpUrl);

                if (index > 0)
                {
                    SendProgress("Link corrupted, fixing...");

                    tmpUrl = tmpUrl.Remove(0, index);
                }
                else
                {
                    SendProgress("Link not corrupted - OK");
                }

                SendProgress("UrlDecode...");

                tmpUrl = System.Net.WebUtility.UrlDecode(tmpUrl);

                SendProgress("Check if link is valid: " + tmpUrl);
                if (CheckIfValidUrl(tmpUrl))
                {
                    linkLocation = tmpUrl;
                    SendProgress("link OK");
                }
                else
                {
                    throw new Exception("Invalid link");
                }
            }
            catch (Exception ex)
            {
                SendError("Error3", ex.ToString(), _driver?.PageSource);
            }
            
            SendProgress("\nlink : \n" + linkLocation == string.Empty ? "link not found" : linkLocation);
            SendProgress("total time = " + new DateTime((DateTime.Now - startTime).Ticks).ToString("HH:mm:ss"));
            SendProgress("press enter to exit");

            //Console.ReadLine();

        }

	public void Close()
	{
	    _driver?.Quit();
	}

        void SendProgress(string str)
        {
    	    WebDriverProgress?.Invoke(this, new MessageEventArgs
	    {
		Message = str
	    });
	}

        void SendError(string param, string str, string pageSource)
        {
	    WebDriverError?.Invoke(this, new MessageEventArgs{
		Message = param + ": " + str
	    });
	}

        bool CheckIfValidUrl(string link)
        {

            Uri uriResult;
            return Uri.TryCreate(link, UriKind.Absolute, out uriResult) && uriResult.Scheme == Uri.UriSchemeHttp;
        }
    }
}
