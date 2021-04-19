namespace Maxstupo.DynamicPaper.Wallpaper {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HeyRed.Mime;
    using Maxstupo.DynamicPaper.Wallpaper.Players;

    public class MediaPlayerStore {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private static MediaPlayerStore instance;
        public static MediaPlayerStore Instance => instance ?? (instance = new MediaPlayerStore());


        private readonly Dictionary<string, string> mimeLookup = new Dictionary<string, string>();
        private readonly Dictionary<string, string[]> playerTypes = new Dictionary<string, string[]>();
        private readonly Dictionary<string, Type> players = new Dictionary<string, Type>();

        public IAttachablePlayer CreatePlayer(string mimeType, IAttachablePlayer existingPlayer = null) {
            if (!mimeLookup.TryGetValue(mimeType, out string playerId)) {
                Logger.Warn("Failed to find player for type {0}", mimeType);
                return null;
            }

            Logger.Debug("Creating player: {0}", mimeType);

            if (existingPlayer != null && existingPlayer.SupportedMimeTypes.Contains(mimeType)) {
                Logger.Debug("  - Reusing player: {0}", existingPlayer.GetType().Name);
                return existingPlayer;
            }

            Type playerType = players[playerId];

            IAttachablePlayer newPlayer = (IAttachablePlayer) Activator.CreateInstance(playerType);
            newPlayer.SupportedMimeTypes = playerTypes[playerId];

            Logger.Debug("  - Player: {0}", newPlayer.GetType().Name);

            return newPlayer;
        }


        public MediaPlayerStore RegisterPlayer<T>(params string[] mimeTypes) where T : IAttachablePlayer {
            string key = typeof(T).FullName;

            if (players.ContainsKey(key))
                throw new ArgumentException("Can't register the same player more than once!");
           
            players.Add(key, typeof(T));
            playerTypes.Add(key, mimeTypes);

            foreach (string mime in mimeTypes) {
                if (mimeLookup.ContainsKey(mime))
                    throw new ArgumentException("Mime type already registered for a player!");

                mimeLookup.Add(mime, key);
            }

            return this;
        }

    }

}