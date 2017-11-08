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
    public class Pet
    {
        private const int AFK_MULTIPLIER = 5;

        private Random RND = new Random();
        public string type;

        public string name;
        public int age; // "napos"
        public int weight, weight_c; // dkg
        public int hunger, fatigue, fun, health; // %
        public string state;

        public DateTime actDT, startDT;
        public int health_s, fatigue_s, fun_s, hunger_s;

        public Pet()
        {
        }

        public Pet(string _name, string _type)
        {
            this.type = _type;
            this.name = _name;
            this.age = 0;
            this.weight = 1;
            this.hunger = 100;
            this.fatigue = 100;
            this.health = 100;
            this.fun = 100;
            this.state = "awake";
            this.startDT = DateTime.Now;

            this.health_s = 0;
            this.fatigue_s = 0;
            this.fun_s = 0;
            this.hunger_s = 0;
            this.weight_c = 0;
        }

        public Pet(string jsonString)
        {
            try
            {
                Pet p = JsonConvert.DeserializeObject<Pet>(jsonString);

                this.type = p.type;
                this.name = p.name;
                this.age = p.age;
                this.weight = p.weight;
                this.hunger = p.hunger;
                this.fatigue = p.fatigue;
                this.fun = p.fun;
                this.health = p.health;
                this.state = p.state;
                this.startDT = p.startDT;
                this.actDT = p.actDT;

                this.health_s = p.health_s;
                this.fatigue_s = p.health_s;
                this.fun_s = p.health_s;
                this.hunger_s = p.health_s;
                this.weight_c = p.health_s;


                // lekezelni ha eddig ébren volt / aludt, és kivonni/hozzáadni a dolgokat (éhség, fáradtság, stb...)
                TimeSpan ts = (TimeSpan) (DateTime.Now - this.actDT);
                ulong totalsecs = Convert.ToUInt64(ts.TotalSeconds);

                if (this.state == "awake")
                {                  
                    this.health -= (int)(totalsecs / (115 * AFK_MULTIPLIER));
                    if (this.health < 0) this.health = 0;

                    this.fatigue -= (int)(totalsecs / (30 * AFK_MULTIPLIER));
                    if (this.fatigue < 0) this.fatigue = 0;

                    this.fun -= (int)(totalsecs / (65 * AFK_MULTIPLIER));
                    if (this.fun < 0) this.fun = 0;

                    this.hunger -= (int)(totalsecs / (50 * AFK_MULTIPLIER));
                    if (this.hunger < 0)
                    {
                        int sulyminusz = Math.Abs(this.hunger) / 120;
                        this.weight -= sulyminusz;

                        if (this.weight < 1)
                            this.weight = 1;

                        this.hunger = 0;
                    }
                }
                else if (state == "asleep")
                {
                    this.health += (int)(totalsecs / (230 * AFK_MULTIPLIER));
                    if (this.health > 100) this.health = 100;

                    this.fatigue += (int)(totalsecs / (60 * AFK_MULTIPLIER));
                    if (this.fatigue > 100) this.fatigue = 100;

                    //this.hunger += (int)(totalsecs / (100 * 3));
                    //if (this.hunger > 100) this.hunger = 100;
                }

                Think();
            }
            catch (Exception)
            {
            }
        }

        public static string Load()
        {
            try
            {
                /*StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFile jsonFile = await localFolder.GetFileAsync("config_pet.json");

                string result = await FileIO.ReadTextAsync(jsonFile);
                return result;*/
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                return localSettings.Values["config_pet"].ToString();
            }
            catch (Exception) { }
            return string.Empty;
        }

        public void Save()
        {
            this.actDT = DateTime.Now;
            string content = JsonConvert.SerializeObject(this); //arr.ToString();
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["config_pet"] = content;
        }

        public void Think()
        {
            try
            {
                if (this.state == "awake")
                {
                    this.health_s++;
                    this.fatigue_s++;
                    this.fun_s++;
                    this.hunger_s++;

                    if (this.health_s >= 115)
                    {
                        if (this.health > 0)
                            this.health--;

                        this.health_s = 0;
                    }

                    if (this.fatigue_s > 30)
                    {
                        if (this.fatigue > 0)
                            this.fatigue--;

                        this.fatigue_s = 0;
                    }

                    if (this.fun_s > 65)
                    {
                        if (this.fun > 0)
                            this.fun--;

                        this.fun_s = 0;
                    }

                    if (this.hunger_s > 50)
                    {
                        if (this.hunger > 0)
                            this.hunger--;

                        this.hunger_s = 0;
                    }

                    if (this.hunger < 51)
                    {
                        this.weight_c++;

                        if (this.weight_c > 120)
                        {
                            if (this.weight > 1)
                            {
                                this.weight--;
                                this.weight_c = 0;
                            }
                        }
                    }
                }
                else if (this.state == "asleep")
                {
                    this.health_s++;
                    this.fatigue_s++;
                    this.hunger_s++;

                    if (this.health_s > 230)
                    {
                        if (this.health < 100)
                            this.health++;

                        this.health_s = 0;
                    }

                    if (this.fatigue_s > 60)
                    {
                        if (this.fatigue < 100)
                            this.fatigue++;

                        this.fatigue_s = 0;
                    }
                }

                TimeSpan ts = (TimeSpan) (DateTime.Now - this.startDT);
                this.age = ((int) (ts.TotalHours / 12));
            }
            catch (Exception) { }
        }

        public bool isSad()
        {
            if (this.health < 25 || this.fatigue < 25 || this.hunger < 25 || this.fun < 25)
                return true;

            return false;
        }

        public void Feed()
        {
            // etetés, súly!!
            int tmp = this.hunger + RND.Next(1, 5);
            if (tmp > 100)
            {
                this.weight += 1 + RND.Next(1, 3); // hízás
                tmp = 100;
            }
            else
                this.weight++;
            
            this.hunger = tmp;
        }

        public bool Heal()
        {
            // gyógyítás
            if (this.health >= 100)
                return false;

            int tmp = this.health + RND.Next(1, 7);
            if (tmp > 100)
                tmp = 100;

            this.health = tmp;
            return true;
        }

        public void Play()
        {
            // játszani vele
            int tmp = this.fun + RND.Next(1, 9);
            if (tmp > 100)
                tmp = 100;

            this.fun = tmp;
        }

        public bool Sleep()
        {
            // altatni
            if (this.state == "asleep")
                return false;

            this.state = "asleep";
            this.actDT = DateTime.Now;
            return true;
        }

        public bool Wake()
        {
            // ébreszteni
            if (this.state == "awake")
                return false;

            this.state = "awake";
            this.actDT = DateTime.Now;
            return true;
        }
    }
}
