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
	private Downloader()
	{
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

	private ILogger _logger;

	public void AddLogger(ILogger logger)
	{
	    _logger = logger;
	}

        public void Go(string downloadUrl)
        {
	    WriteDebug("Scraping url: " + downloadUrl);
	    
            IWebDriver driver = null;
            string linkLocation = string.Empty;
            DateTime startTime = DateTime.Now;

            try
            {
		var service = PhantomJSDriverService.CreateDefaultService();
		WriteDebug("service OK: " + (service!=null).ToString());
		var options = new PhantomJSOptions();
		WriteDebug("options OK: " + (options!=null).ToString());
		
                driver = new PhantomJSDriver(service, options, TimeSpan.FromSeconds(30));
                //driver = new FirefoxDriver(new FirefoxBinary(), new FirefoxProfile(), TimeSpan.FromMinutes(3));


                WriteDebug("Navigating...");
                driver.Navigate().GoToUrl(downloadUrl);
                //driver.Navigate().GoToUrl("http://localhost/temp2.html");

                //driver.Navigate().GoToUrl("http://www.kinoman.tv/film/gesia-skorka-1");

                //driver.Navigate().GoToUrl("http://www.kinoman.tv/film/pakt-z-diablem");
                //driver.Navigate().GoToUrl("http://localhost/temp3.html");

                try
                {
                    //IWebElement query = driver.FindElement(By.LinkText("OglÄdaj film z limitem."));
                    IWebElement query = driver.FindElement(By.XPath("//div[@class='player-wrapper']/a"));
                    WriteDebug("Click 1");
                    query.Click();
                    WriteDebug("OK");
                    //Thread.Sleep(TimeSpan.FromSeconds(10));
                }
                catch (Exception ex)
                {
                    WriteDebug("Error1", ex.ToString(), driver?.PageSource);
                }

                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                var element = wait.Until((d) => { return d.FindElement(By.XPath("//button[@class='btn btn-primary']")); });

                try
                {
                    WriteDebug("Click 2");
                    //var html = element.GetAttribute("outerHTML");
                    //html = element.GetAttribute("innerHTML");
                    element.Click();
                    WriteDebug("OK");
                }
                catch (Exception ex)
                {
                    WriteDebug("Warning", ex.ToString(), driver?.PageSource);
                }

                try
                {
                    wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                    element = wait.Until((d) => { return d.FindElement(By.ClassName("player-wrapper")); });
                    var temp = element;
                    var iframe = wait.Until((d) => { return temp.FindElement(By.TagName("iframe")); });
                    var link = iframe.GetAttribute("src");

                    WriteDebug("checking if link is valid: " + link);

                    if (CheckIfValidUrl(link))
                    {
                        WriteDebug("Link is OK. Navigating...");
                        driver.Navigate().GoToUrl(link);
                    }
                    else
                    {
                        WriteDebug("Invalid link...");
                        throw new Exception("Invalid link");
                    }
                }
                catch (Exception ex)
                {
                    WriteDebug("Error2", ex.ToString(), driver?.PageSource);
                }

                wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                var tmpUrl = wait.Until(d => { return d.FindElement(By.XPath("//div[@id='playerVidzer']/a")).GetAttribute("href"); });

                int index = tmpUrl.LastIndexOf("http", StringComparison.InvariantCulture);
                WriteDebug("Got link: " + tmpUrl);

                if (index > 0)
                {
                    WriteDebug("Link corrupted, fixing...");

                    tmpUrl = tmpUrl.Remove(0, index);
                }
                else
                {
                    WriteDebug("Link not corrupted - OK");
                }

                WriteDebug("UrlDecode...");

                tmpUrl = System.Net.WebUtility.UrlDecode(tmpUrl);

                WriteDebug("Check if link is valid: " + tmpUrl);
                if (CheckIfValidUrl(tmpUrl))
                {
                    linkLocation = tmpUrl;
                    WriteDebug("link OK");
                }
                else
                {
                    throw new Exception("Invalid link");
                }
            }
            catch (Exception ex)
            {
                WriteDebug("Error3", ex.ToString(), driver?.PageSource);
            }
            finally
            {
                driver?.Quit();
            }

            WriteDebug("\nlink : \n" + linkLocation == string.Empty ? "link not found" : linkLocation);
            WriteDebug("total time = " + new DateTime((DateTime.Now - startTime).Ticks).ToString("HH:mm:ss"));
            WriteDebug("press enter to exit");

            //Console.ReadLine();

        }

        void WriteDebug(string str)
        {
            //Console.WriteLine(str);
    	    _logger?.LogInformation(str);
	}

        void WriteDebug(string param, string str, string pageSource)
        {
	    _logger?.LogInformation("Exception: " + param + str + "\n" + pageSource);
            //Debug.WriteLine(param + " " + str);
            //screenshot.SaveAsFile(@"c:\\users\lewy\" + param + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            //File.WriteAllText(@"c:\\users\lewy\" + param + ".txt", pageSource);
        }

        bool CheckIfValidUrl(string link)
        {

            Uri uriResult;
            return Uri.TryCreate(link, UriKind.Absolute, out uriResult) && uriResult.Scheme == Uri.UriSchemeHttp;
        }
    }
}
