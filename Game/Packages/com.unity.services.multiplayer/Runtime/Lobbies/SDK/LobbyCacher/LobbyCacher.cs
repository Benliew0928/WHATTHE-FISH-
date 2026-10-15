using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

namespace Lobbies.SDK.LobbyCacher
{
    class LobbyCacher
    {
        readonly string m_PlayerID;
        readonly Dictionary<string, LobbyEntry> m_LobbyCacher;

        class LobbyEntry : ILobbyEvents
        {
            public bool IsSubscribed { get; private set; }
            public Lobby Lobby { get; set; }
            public LobbyEventCallbacks Callbacks { get; set; }

            public Task SubscribeAsync()
            {
                if (Callbacks == null)
                    throw new InvalidOperationException(
                        "No callbacks set for lobby events. Set callbacks using WithEventSubscription before subscribing.");

                IsSubscribed = true;
                return Task.CompletedTask;
            }

            public Task UnsubscribeAsync()
            {
                IsSubscribed = false;
                return Task.CompletedTask;
            }
        }

        public LobbyCacher(string playerID = "")
        {
            m_PlayerID = playerID;
            m_LobbyCacher = new Dictionary<string, LobbyEntry>();
        }

        public ILobbyEvents WithEventSubscription(string lobbyId, LobbyEventCallbacks lobbyCallbacks)
        {
            if (!m_LobbyCacher.TryGetValue(lobbyId, out var lobbyEntry))
            {
                return null;
            }

            lobbyEntry.Callbacks = lobbyCallbacks;
            lobbyEntry.SubscribeAsync();

            return lobbyEntry;
        }

        /// <summary>
        /// Attempts to get the cached <see
        /// cref="Lobby"/> for the specified lobby.
        /// </summary>
        /// <param name="lobbyId">
        /// The identifier of the lobby to look up.
        /// </param>
        /// <param name="cachedLobby">
        /// When this method returns, contains the cached <see
        /// cref="Lobby"/> for <paramref name="lobbyId"/> if
        /// one is present; otherwise, <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if a non-<see langword="null"/>
        /// <see cref="Lobby"/> is cached for <paramref
        /// name="lobbyId"/>; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="lobbyId"/> is <see langword="null"/>.
        /// </exception>
        public bool TryGetLobbyCache(string lobbyId, out Lobby cachedLobby)
        {
            cachedLobby = m_LobbyCacher.TryGetValue(lobbyId, out var lobbyEntry) ? lobbyEntry.Lobby : null;
            return cachedLobby != null;
        }

        public string GetIfMatchTag(string lobbyId, bool applyIfMatch = true)
        {
            return applyIfMatch ? GetLobbyCacheVersion(lobbyId) : default;
        }

        string GetLobbyCacheVersion(string lobbyId)
        {
            return TryGetLobbyCache(lobbyId, out var cachedLobby) ? cachedLobby.Version.ToString() : null;
        }

        public bool RemoveLobbyCache(string lobbyId)
        {
            return m_LobbyCacher.Remove(lobbyId);
        }

        public void UpdateLobbyCache(string lobbyId, Lobby newLobby)
        {
            if (!TryGetLobbyCache(lobbyId, out var cachedLobby))
            {
                return;
            }
            if (newLobby.Version < cachedLobby.Version) return;
            var entry = m_LobbyCacher[lobbyId];
            var changes = LobbyPatcher.GetLobbyDiff(cachedLobby, newLobby);
            // A full response is authoritative, including player order. Turning
            // it into remove/insert operations can produce a different order
            // after several players move, corrupting the next indexed event.
            bool removed = cachedLobby.Players?.Exists(player => player.Id == m_PlayerID) == true &&
                newLobby.Players?.Exists(player => player.Id == m_PlayerID) != true;
            if (removed)
            {
                RemoveLobbyCache(lobbyId);
                if (entry.IsSubscribed) entry.Callbacks?.InvokeKickedFromLobby();
                return;
            }
            entry.Lobby = Unity.Services.Lobbies.Internal.WrappedLobbyService.CloneLobbyHelper(newLobby);
            if (entry.IsSubscribed && newLobby.Version > cachedLobby.Version)
                entry.Callbacks?.InvokeLobbyChanged(changes);
        }

        public void UpdateLobbyCache(string lobbyId, ILobbyChanges changes)
        {
            if (!TryGetLobbyCache(lobbyId, out var cachedLobby))
            {
                return;
            }

            LobbyEntry cachedEntry;
            // if the lobby was deleted, remove it from the cache and invoke the callback
            if (changes.LobbyDeleted)
            {
                if (m_LobbyCacher.TryGetValue(lobbyId, out cachedEntry))
                {
                    var removed = RemoveLobbyCache(lobbyId);
                    if (removed && cachedEntry is { IsSubscribed : true })
                    {
                        cachedEntry.Callbacks?.InvokeLobbyChanged(changes);
                        cachedEntry.UnsubscribeAsync();
                    }
                }

                return;
            }

            // Membership events carry indices rather than identities. A valid
            // index can still refer to the wrong survivor after independently
            // refreshed/reordered lists. Leave membership untouched and let
            // LobbyChannel obtain the authoritative full response.
            if (changes.PlayerLeft.Changed || changes.PlayerLeft.Added ||
                changes.PlayerJoined.Changed || changes.PlayerJoined.Added) return;

            // if the cached lobby version is older than the changed lobby version, update the cached lobby
            // and invoke the callback (or InvokeKickedFromLobby if the player was removed)
            if (cachedLobby.Version < changes.Version.Value)
            {
                // A missing predecessor must be recovered with a full response;
                // its positional player edits cannot be applied to this cache.
                if (changes.Version.Value != cachedLobby.Version + 1) return;
                var updated = Unity.Services.Lobbies.Internal.WrappedLobbyService.CloneLobbyHelper(cachedLobby);
                try { changes.ApplyToLobby(updated); }
                catch (Exception e) when (e is ArgumentOutOfRangeException || e is IndexOutOfRangeException || e is KeyNotFoundException)
                {
                    // Keep both the roster and version unchanged. LobbyChannel
                    // detects the unapplied event and fetches a full snapshot.
                    return;
                }
                bool removed = cachedLobby.Players?.Exists(player => player.Id == m_PlayerID) == true &&
                    updated.Players?.Exists(player => player.Id == m_PlayerID) != true;
                if (removed)
                {
                    if (m_LobbyCacher.TryGetValue(cachedLobby.Id, out cachedEntry))
                    {
                        RemoveLobbyCache(cachedLobby.Id);
                        if (cachedEntry.IsSubscribed) cachedEntry.Callbacks?.InvokeKickedFromLobby();
                    }
                    return;
                }

                if (m_LobbyCacher.TryGetValue(cachedLobby.Id, out cachedEntry))
                {
                    cachedEntry.Lobby = updated;
                    if (cachedEntry.IsSubscribed) cachedEntry.Callbacks?.InvokeLobbyChanged(changes);
                }
            }
        }

        public bool AddLobbyCache(string lobbyId, Lobby lobby)
        {
            if (m_LobbyCacher.TryGetValue(lobbyId, out var cachedLobby))
            {
                if (cachedLobby.Lobby != null)
                {
                    return false;
                }

                cachedLobby.Lobby = lobby;
                return true;
            }

            m_LobbyCacher[lobbyId] = new LobbyEntry { Lobby = lobby };
            return true;
        }

        /// <summary>
        /// Invalidates the cache for a Lobby.
        /// </summary>
        /// <param name="lobbyId">The Lobby id.</param>
        public void InvalidateCacheForLobby(string lobbyId)
        {
            if (m_LobbyCacher.TryGetValue(lobbyId, out var lobbyEntry))
            {
                lobbyEntry.Lobby = null;
            }
        }
    }
}
