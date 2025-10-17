using System.Threading;
using System.Threading.Tasks;

namespace SWIMS.Services.Notifications;

public interface IWebPushSender
{
    Task SendToUserAsync(int userId, object payload, CancellationToken cancellationToken = default);
}
