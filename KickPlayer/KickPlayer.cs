using UnityEngine;
using System;
using System.Collections.Generic;
using Steamworks;

namespace KickPlayer
{
    public class KickPlayer : Mod
    {
        public void Start()
        {
            Debug.Log("Mod KickPlayer has been loaded!");
        }

        public void OnModUnload()
        {
            Debug.Log("Mod KickPlayer has been unloaded!");
        }

        [ConsoleCommand(name: "users", docs: "Show all user names and IDs")]
        public static string Users(string[] args)
        {
            List<string> userNames = new List<string>();
            Semih_Network network = ComponentManager<Semih_Network>.Value;
            foreach (CSteamID remoteUserId in network.remoteUsers.Keys)
            {
                if (remoteUserId == network.LocalSteamID)
                {
                    Debug.Log("Ignored localPlayer " + remoteUserId);
                    continue;
                }
                userNames.Add(remoteUserId + " " + SteamFriends.GetFriendPersonaName(remoteUserId));
            }
            return String.Join("\n", userNames);
        }

        [ConsoleCommand(name: "kick", docs: "Kick player by ID")]
        public static string Kick(string[] args)
        {
            if (!Semih_Network.IsHost) {
                return "Only the host can kick users!";
            }
            if (args.Length < 1)
            {
                KickPlayer.Users(new string[] { });
                return "No steamID given!";
            }
            string userId = args[0];
            List<string> usersKicked = new List<string>();
            Semih_Network network = ComponentManager<Semih_Network>.Value;
            foreach (CSteamID remoteUserId in network.remoteUsers.Keys)
            {
                string id = remoteUserId.ToString();
                Debug.Log(id);
                if (id.Substring(0, userId.Length) == userId)
                {
                    // Kick
                    if (remoteUserId == network.LocalSteamID)
                    {
                        // Don't kick yourself
                        Debug.Log("Ignored localPlayer " + id);
                        continue;
                    }
                    network.remoteUsers.Remove(remoteUserId);
                    Helper.LogBuild(SteamFriends.GetFriendPersonaName(remoteUserId) + " has disconnected");
                    SteamNetworking.CloseP2PSessionWithUser(remoteUserId);
                    usersKicked.Add(remoteUserId.ToString());
                }
            }
            ComponentManager<SaveAndLoad>.Value.SaveGame(true);
            return "You've kicked " + (usersKicked.Count > 0 ? String.Join(" ", usersKicked) : "nobody");
        }
    }
}