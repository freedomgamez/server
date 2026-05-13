using DigitalWorldOnline.Application.Separar.Commands.Update;
using DigitalWorldOnline.Commons.Entities;
using MediatR;
using System.Linq;

namespace DigitalWorldOnline.Game.Services
{
    public sealed class OwnerStorageFlushService
    {
        private readonly ISender _sender;

        public OwnerStorageFlushService(ISender sender)
        {
            _sender = sender;
        }

        public async Task FlushForTransitionAsync(GameClient client)
        {
            if (client?.Tamer == null)
                return;

            var storages = client.Tamer.ItemList
                .Where(x => x != null && x.Id > 0)
                .GroupBy(x => x.Id)
                .Select(x => x.First())
                .ToList();

            foreach (var storage in storages)
                await _sender.Send(new UpdateItemsCommand(storage));
        }
    }
}
