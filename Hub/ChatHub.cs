using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SignalR.Hubs
{
    // 這就是所謂的 SignalR 中樞
    public class ChatHub : Hub
    {
        // 這是提供 Client (js)端呼叫的方法，後面是這個方法接受的參數
        public async Task SendMessage(string user, string message)
        {
            // 針對每個以連線的客戶端呼叫 ReceiceMassage 方法，並傳送參數 user、message
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}