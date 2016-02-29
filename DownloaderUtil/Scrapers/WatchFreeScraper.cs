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
using Newtonsoft.Json.Linq;

namespace DownloaderUtil
{
    public sealed class WatchFreeScraper : ScraperBase
    {
	public static string Key
	{
	    get
	    {
		return "http://www.watchfree.to";
	    }
	}
 
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
	    WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

            try
            {
                SendProgress("Navigating...");
                _driver.Navigate().GoToUrl(scrapeReq.InputUrl);
                
                string tmpUrl = string.Empty;

		try
                {
		    name = _driver.FindElement(By.XPath("//div[@class='body']/div/h1/span/a")).Text;

		    SendProgress("Found Name: " + name);
		    SendProgress("Clicking...");

                    var videoClick = _driver.FindElement(By.XPath("//div[@class='video_play_button']/a"));
                    videoClick.Click();

		    var frames = _driver.FindElements(By.TagName("iframe"));
                    
		    for (int i = 0; i < frames.Count(); i++)
                    {
                        _driver.SwitchTo().Frame(i);
                        //WriteDebug("OK", "OK", driver.TakeScreenshot(), driver.PageSource);

			try
			{
			    var script = _driver.FindElements(By.TagName("script")).FirstOrDefault(s => s.GetAttribute("innerHTML").Contains("jwplayer(\"vplayer\").setup({")).GetAttribute("innerHTML");
			    if(script != null)
			    {
				script = script.Replace("jwplayer(\"vplayer\").setup(", string.Empty);
                                var endIndex = script.IndexOf("});");
                                script = script.Remove(endIndex, script.Length - endIndex);
                                script += "}";
                                var json = JObject.Parse(script);
                                tmpUrl = json["modes"][1]["config"]["file"].ToString();

				SendProgress("flash video found in frame: " + i);
			    	break;
			    }	
                        }
			catch(Exception ex)
			{
			    
			}
                        finally
                        {
                            _driver.SwitchTo().DefaultContent();
                        }
                    }                    
                }
                catch (Exception ex)
                {
                    SendError("Error1", ex.ToString(), _driver.PageSource??"null");
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
		SendError("Error3", ex.ToString(), _driver?.PageSource);
            }
            
            SendProgress("\nlink : \n" + linkLocation == string.Empty ? "link not found" : linkLocation);
            SendProgress("total time = " + new DateTime((DateTime.Now - startTime).Ticks).ToString("HH:mm:ss"));

	    return scrapeDesc;
	}
    }
}
