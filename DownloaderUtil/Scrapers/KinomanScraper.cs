using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Diagnostics;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;
using Microsoft.Extensions.Logging;
using System.Net;
using System.ComponentModel;

namespace DownloaderUtil
{
    public sealed class KinomanScraper : ScraperBase
    {
	public override ScrapeDesc Scrape(ScrapeReq scrapeReq)
	{
    	    if(_driver == null)
	    {
		SendProgress("WEB Driver is null!");
		
		throw new Exception();
	    }
	    
	    ScrapeDesc scrapeDesc = null;    
	    SendProgress("Scraping url: " + scrapeReq.InputUrl);
	        
            string linkLocation = string.Empty;
	    string name = string.Empty;
            DateTime startTime = DateTime.Now;
	    List<string> errors = new List<string>();

            try
            {

                SendProgress("Navigating...");
                _driver.Navigate().GoToUrl(scrapeReq.InputUrl);
                
                try
                {
		    name = _driver.FindElement(By.XPath("//span[@itemprop='name']")).Text;
                    SendProgress("Found name: " + name);
		    IWebElement query = _driver.FindElement(By.XPath("//div[@class='player-wrapper']/a"));
                    SendProgress("Click 1");
                    query.Click();
                    SendProgress("OK");
                }
                catch (Exception ex)
                {
		    errors.Add("Error1");
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
		    errors.Add("Error2");
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
                    SendProgress("link OK, removing invalid characters: " + name);
		    name = CleanFileName(name);
		    SendProgress("done: " + name);
		    
		    scrapeDesc = new ScrapeDesc{Name=name, DownloadUrl=linkLocation, Id=scrapeReq.Id};
                }
                else
                {
                    throw new Exception("Invalid link");
                }
            }
            catch (Exception ex)
            {
		errors.Add("Error3");
                SendError("Error3", ex.ToString(), _driver?.PageSource);
            }
            
            SendProgress("\nlink : \n" + linkLocation == string.Empty ? "link not found" : linkLocation);
            SendProgress("total time = " + new DateTime((DateTime.Now - startTime).Ticks).ToString("HH:mm:ss"));

	    return scrapeDesc;
	}
    }
}
