using Microsoft.AspNetCore.SignalR;

namespace WebapiPlayground.Hubs
{
    public class RTHub : Hub
    {
        INotificationService _notificationService;
        public RTHub(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }
        public Task SendMessage(string user)
        {
            //await Clients.All.SendAsync("ReceiveMessage", user);
            return _notificationService.Subscribe1(Context.ConnectionId);
        }

        public void SendMessage2(string user)
        {
            //await Clients.All.SendAsync("ReceiveMessage", user);
            _notificationService.Subscribe2(Context.ConnectionId);
        }
    }

    public interface INotificationService
    {
        public Task Subscribe1(string connectionId);
        public void Subscribe2(string connectionId);
    }

    public class NotificationService : INotificationService
    {
        private IHubContext<RTHub> _hub;
        public List<string> ConnectionIds = new List<string>();
        private DataService ds = new DataService();

        public NotificationService(IHubContext<RTHub> hub)
        {
            _hub = hub;
        }

        public async Task Subscribe1(string connectionId)
        {
            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(500);
                _hub.Clients.Client(connectionId).SendAsync("ReceiveMessage", i);
            }
        }

        public void Subscribe2(string connectionId)
        {
            ds.ExecuteAsync((x) =>
            {
                _hub.Clients.Client(connectionId).SendAsync("ReceiveMessage", x);
            });
        }

        public class DataService
        {
            public void ExecuteAsync(Action<string> callback)
            {
                new Thread(() =>
                {
                    for (int i = 0; i < 10; i++)
                    {
                        var uniqueData = DateTime.Now.Millisecond.ToString();
                        callback(uniqueData);
                        Thread.Sleep(100000);
                    }
                    callback("done");

                }).Start();
            }
        }
    }
}
