using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;

namespace Tamagotchi.Entities
{
    public class Player
    {
        public int coins;
        public DateTime playDT;
        
        public Player()
        {
            this.coins = 10;
        }

        public Player(string jsonString)
        {
            try
            {
                Player p = JsonConvert.DeserializeObject<Player>(jsonString);
                this.coins = p.coins;
                this.playDT = p.playDT;
            }
            catch (Exception)
            {
            }
        }

        public void Save()
        {
            string content = JsonConvert.SerializeObject(this);
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["config_player"] = content;
        }

        public static string Load()
        {
            try
            {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                return localSettings.Values["config_player"].ToString();
            }
            catch (Exception) { }
            return string.Empty;
        }
    }
}
