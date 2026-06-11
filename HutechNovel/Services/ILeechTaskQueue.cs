using HutechNovel.Models;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace HutechNovel.Services
{
    public interface ILeechTaskQueue
    {
        ValueTask QueueLeechRequestAsync(MassLeechRequest request);
        ValueTask<MassLeechRequest> DequeueAsync(CancellationToken cancellationToken);
    }

    public class LeechTaskQueue : ILeechTaskQueue
    {
        private readonly Channel<MassLeechRequest> _queue;

        public LeechTaskQueue()
        {
            var options = new BoundedChannelOptions(100)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel.CreateBounded<MassLeechRequest>(options);
        }

        public async ValueTask QueueLeechRequestAsync(MassLeechRequest request)
        {
            await _queue.Writer.WriteAsync(request);
        }

        public async ValueTask<MassLeechRequest> DequeueAsync(CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }
}
