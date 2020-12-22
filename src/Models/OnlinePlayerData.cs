using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vschatbot.src.Models
{
    public class OnlinePlayerData
    {
        public string PlayerName { get; set; }
        public int SessionLengthInMinutes { get; set; }
        public int TotalPlaytimeInMinutes { get; set; }
    }
}
