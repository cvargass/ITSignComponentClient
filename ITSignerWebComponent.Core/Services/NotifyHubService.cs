using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace ITSignerWebComponent.Core.Services
{
    public class NotifyHubService : Hub
    {
        // Método invocable desde app de escritorio
        public async Task ReloadPage()
        {
            // Avisa a todos los navegadores conectados
            await Clients.All.SendAsync("ReloadPage");
        }
    }
}
