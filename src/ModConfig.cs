using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vschatbot.src
{
    public class ModConfig
    {
        public string Token { get; set; } = "insert token here";
        public ulong ServerId { get; set; } = 11111111111;
        public ulong ChannelId { get; set; } = 22222222222;

        public bool SendDeathMessages { get; set; } = true;
        public bool AddDeathCountToDeathMessages { get; set; } = true;
        public bool SendServerMessages { get; set; } = true;
        public bool SendStormNotification { get; set; } = true;
        public bool SendStormEarlyNotification { get; set; } = true;
        public bool RelayDiscordToGame { get; set; } = true;
        public bool RelayGameToDiscord { get; set; } = true;

        public string TEXT_StormEarlyWarning { get; set; } = "It appears a {strength} storm is coming...";
        public string TEXT_StormBegin { get; set; } = "Harketh the storm doth come, Wary be thine self, as for thy own end be near.";
        public string TEXT_StormEnd { get; set; } = "The temporal storm seems to be waning...";
        public string TEXT_ServerStart { get; set; } = "Server is now up and running. Come on in!";
        public string TEXT_ServerStop { get; set; } = "Server is shutting down. Goodbye!";
        public string TEXT_DeathMessage { get; set; } = "Their total death count is now {deathCount}!";
    }
}
