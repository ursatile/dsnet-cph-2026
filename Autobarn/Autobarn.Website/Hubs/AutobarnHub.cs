using Microsoft.AspNetCore.SignalR;

namespace Autobarn.Website.Hubs {
	public class AutobarnHub : Hub {
		public async Task NotifyWebsiteUsers(string user, string message) {
			await Clients.All.SendAsync("LookThisIsAMagicString", user, message);
		}
	}
}
