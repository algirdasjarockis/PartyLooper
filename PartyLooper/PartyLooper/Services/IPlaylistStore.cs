using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartyLooper.Services
{
    public interface IPlaylistStore<T>
    {
        Task<IEnumerable<T>> LoadPlaylistAsync();

        Task PersistPlaylistAsync(IEnumerable<T> items);
    }
}
