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
        public ulong ConsoleChannelId { get; set; } = 33333333333;
        public bool SendDeathMessages { get; set; } = true;
        public bool SendServerMessages { get; set; } = true;
        public bool SendStormNotification { get; set; } = true;
        public bool SendStormEarlyNotification { get; set; } = true;
        public bool RelayDiscordToGame { get; set; } = true;
        public bool RelayGameToDiscord { get; set; } = true;
        public string TEXT_DeathMessageUnknown { get; set; } = "was killed by the unknown"; 
        public string TEXT_DeathMessageGravity { get; set; } = "smashed into the ground"; 
        public string TEXT_DeathMessageFire { get; set; } = "burned to death";
        public string TEXT_DeathMessageCrushing { get; set; } = "was crushed";
        public string TEXT_DeathMessageSlashingAttack { get; set; } = "was sliced open";
        public string TEXT_DeathMessagePiercingAttack { get; set; } = "was pierced through";
        public string TEXT_DeathMessageSuffocation { get; set; } = "suffocated to death";
        public string TEXT_DeathMessageHeal { get; set; } = "was somehow *healed* to death";
        public string TEXT_DeathMessagePoison { get; set; } = "was poisoned";
        public string TEXT_DeathMessageHunger { get; set; } = "starved to death";
        public string TEXT_DeathMessageDefault { get; set; } = "was killed";
        public string TEXT_DeathMessageBlock { get; set; } = "by a block.";
        public string TEXT_DeathMessagePVP { get; set; } = "when they failed at PVP.";
        public string TEXT_DeathMessageFall { get; set; } = "when they fell to their doom.";
        public string TEXT_DeathMessageDrown { get; set; } = "when they tried to breath in water.";
        public string TEXT_DeathMessageRevive { get; set; } = "just as they respawned.";
        public string TEXT_DeathMessageVoid { get; set; } = "when they fell screaming into the abyss.";
        public string TEXT_DeathMessageSuicide { get; set; } = "when they killed themselves.";
        public string TEXT_DeathMessageInternal { get; set; } = "when they took damage from the inside...";
        public string TEXT_DeathMessageWolf { get; set; } = "and eaten by a wolf.";
        public string TEXT_DeathMessagePigM { get; set; } = "by a boar.";
        public string TEXT_DeathMessagePigF { get; set; } = "by a sow.";
        public string TEXT_DeathMessageBighorn { get; set; } = "by a sheep.";
        public string TEXT_DeathMessageСhicken { get; set; } = "by a... chicken.";
        public string TEXT_DeathMessageLocust { get; set; } = "by a locust.";
        public string TEXT_DeathMessageDrifter { get; set; } = "by a drifter.";
        public string TEXT_DeathMessageBee { get; set; } = "by a swarm of bees.";
        public string TEXT_DeathMessageMob { get; set; } = "by a swarm of bees.";
        public string TEXT_DeathMessageExplosion { get; set; } = "when they stood by a bomb.";
        public string TEXT_DeathMessageMachine { get; set; } = "when they got their hands stuck in a machine.";
        public string TEXT_DeathMessageUnknownS { get; set; } = "when they encountered the unknown.";
        public string TEXT_DeathMessageWeather { get; set; } = "when the weather itself suddenly struck.";
        public string TEXT_DeathMessageUnknownU { get; set; } = "by the unknown.";
        public string TEXT_PlayerDeathCountMessage { get; set; } = "Their total death count is now:";
        public string TEXT_PlayerDisconnectMessage { get; set; } = "has disconnect to the server!";
        public string TEXT_PlayerJoinMessage { get; set; } = "has connected to the server!";
        public string TEXT_StormEarlyWarning { get; set; } = "It appears a {strength} storm is coming...";
        public string TEXT_StormBegin { get; set; } = "Harketh the storm doth come, Wary be thine self, as for thy own end be near.";
        public string TEXT_StormEnd { get; set; } = "The temporal storm seems to be waning...";
        public string TEXT_ServerStart { get; set; } = "Server is now up and running. Come on in!";
        public string TEXT_ServerStop { get; set; } = "Server is shutting down. Goodbye!"; 
        public string TEXT_Time { get; set; } = "Time and season:";
    }
}
