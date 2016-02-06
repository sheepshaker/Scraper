using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using Scraper.Models;

namespace Scraper.Hubs
{
    public class ChatHub : Hub
    {
        private static System.Threading.Timer _timer;

        public override Task OnConnected()
        {
            if (_timer == null)
            {
                _timer = new Timer(BroadcastMessage);
                _timer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(1));
            }

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            return base.OnDisconnected(stopCalled);
        }

        void BroadcastMessage(object state)
        {
            Clients.All.broadcastTime(DateTime.Now.ToString("HH:mm:ss tt zz"));
        }

        public void Send(string name, string message)
        {
            // Call the broadcastMessage method to update clients.
            //Clients.All.broadcastMessage(name, message);
            Clients.All.addNewMessageToPage(name, message);
	}

	public void SendScrapeAdded(Scrape scrape)
	{
	    var json = JsonConvert.SerializeObject(scrape);
	    Clients.All.broadcastScrapeAdded(json);
	}
    }
}