using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Tamagotchi.Entities;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using XamlAnimatedGif;

namespace Tamagotchi
{
    public sealed partial class MainPage : Page
    {
        Pet PET;
        Player PLAYER;
        ThreadPoolTimer MainTimer, CubeTimer;
        public string Action = "", SlimeGif = "";
        public int cubeRollsLeft = 0, lastRoll = 0;
        public Random RND = new Random();

        public MainPage()
        {
            this.InitializeComponent();
            Application.Current.Suspending += new SuspendingEventHandler(App_Suspending);

            string tmp = Pet.Load(); 
            if(tmp == string.Empty)
                PET = new Pet("MyPet", "default");
            else
                PET = new Pet(tmp);

            tmp = Player.Load();
            if (tmp == string.Empty)
                PLAYER = new Player();
            else
                PLAYER = new Player(tmp);

            MainTimer = ThreadPoolTimer.CreatePeriodicTimer(MainTimer_Tick, TimeSpan.FromSeconds(1));
            ButtonCorrection();
        }

        private void App_Suspending(object sender, SuspendingEventArgs e)
        {
            PLAYER.Save();
            PET.Save();
        }

        private void btFeed_Click(object sender, RoutedEventArgs e)
        {
            if (PET.state == "asleep")
                return;

            if (PLAYER.coins < 2)
                return;

            PLAYER.coins -= 2;
            Action = "feed";

            PET.Feed();
            UpdateStatus();
        }

        private void btHeal_Click(object sender, RoutedEventArgs e)
        {
            if (PET.state == "asleep")
                return;

            if (PLAYER.coins < 4)
                return;

            PLAYER.coins -= 4;
            Action = "heal";

            PET.Heal();
            UpdateStatus();
        }

        private void btPlay_Click(object sender, RoutedEventArgs e)
        {
            if (PET.state == "asleep")
                return;

            PrepareCubeThrow();
        }

        private void btSleep_Click(object sender, RoutedEventArgs e)
        {
            if (PET.state == "asleep")
                return;

            Action = "";

            PET.Sleep();
            ButtonCorrection();
        }

        private void btWake_Click(object sender, RoutedEventArgs e)
        {
            if (PET.state == "awake")
                return;

            Action = "";

            PET.Wake();
            ButtonCorrection();
        }

        Popup popup = new Popup();
        private void btMenu_Click(object sender, RoutedEventArgs e)
        {
            if (!popup.IsOpen)
            {
                PopupMenu pm = new PopupMenu(PET.name);
                pm.updatePetNameD += ((string pn) => { PET.name = pn; });
                popup.Child = pm;
                popup.VerticalAlignment = VerticalAlignment.Center;
                popup.HorizontalAlignment = HorizontalAlignment.Center;
                popup.VerticalOffset = (this.ActualHeight / 2) - (300 / 2);
                popup.HorizontalOffset = (this.ActualWidth / 2) - (300 / 2);
                popup.IsOpen = true;
            }
        }

        private async void MainTimer_Tick(object state)
        {
            PET.Think();

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => 
            {
                if (PET.state == "awake")
                {
                    if (Action != "")
                    {
                        if (Action == "feed")
                            SetAnim("eat");
                        else if (Action == "heal" || Action == "play")
                            SetAnim("happy");

                        Action = "";
                    }
                    else
                    {
                        if (PET.isSad())
                            SetAnim("sad");
                        else
                            SetAnim("idle");
                    }
                }
                else
                {
                    Action = "";
                    SetAnim("asleep");
                }

                ButtonCorrection();
                UpdateStatus();
            });
        }

        public void SetAnim(string anim)
        {
            /*foreach(Image img in spSlime.Children)
            {
                if (img.Visibility == Visibility.Visible && img.Name == ("s_" + anim))
                    return;
            }*/

            int idx = -1;
            for(int i = 0; i < spSlime.Children.Count; i++)
            {
                if (spSlime.Children[i].GetType() == typeof(Image))
                {
                    if (((Image)spSlime.Children[i]).Name == ("s_" + anim))
                        idx = i;
                    else
                        ((Image)spSlime.Children[i]).Visibility = Visibility.Collapsed;
                }
            }
            ((Image)spSlime.Children[idx]).Visibility = Visibility.Visible;
        }

        public void ButtonCorrection()
        {
            if (PET.state == "asleep")
            {
                btSleep.Visibility = Visibility.Collapsed;
                btWake.Visibility = Visibility.Visible;
            }
            else
            {
                btSleep.Visibility = Visibility.Visible;
                btWake.Visibility = Visibility.Collapsed;
            }

            if (((TimeSpan)(DateTime.Now - PLAYER.playDT)).TotalSeconds > 60)
                btPlay.IsEnabled = true;
            else
                btPlay.IsEnabled = false;
        }

        public void PrepareCubeThrow()
        {
            PLAYER.playDT = DateTime.Now;
            ButtonCorrection();

            spSlime.Visibility = Visibility.Collapsed;
            spCube.Visibility = Visibility.Visible;

            cubeRollsLeft = RND.Next(4, 9);
            CubeTimer = ThreadPoolTimer.CreatePeriodicTimer(CubeTimer_Tick, TimeSpan.FromMilliseconds(625));
        }

        private async void CubeTimer_Tick(object state)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (cubeRollsLeft == 0)
                    EndCubeThrow();
                else
                {
                    lastRoll = RND.Next(1, 7);
                    ShowCube(lastRoll);
                }
            });
            cubeRollsLeft--;
        }

        public void EndCubeThrow()
        {
            CubeTimer.Cancel();
            PLAYER.coins += lastRoll;

            spCube.Visibility = Visibility.Collapsed;
            spSlime.Visibility = Visibility.Visible;

            HideCubes();
            ButtonCorrection();
            UpdateStatus();
        }

        public void ShowCube(int nr)
        {
            int idx = -1;
            for (int i = 0; i < spCube.Children.Count; i++)
            {
                if (spCube.Children[i].GetType() == typeof(Image))
                {
                    if (((Image)spCube.Children[i]).Name == ("cube_" + nr))
                        idx = i;
                    else
                        ((Image)spCube.Children[i]).Visibility = Visibility.Collapsed;
                }
            }
            ((Image)spCube.Children[idx]).Visibility = Visibility.Visible;
        }

        public void HideCubes()
        {
            for (int i = 0; i < spCube.Children.Count; i++)
            {
                if (spCube.Children[i].GetType() == typeof(Image))
                    ((Image)spCube.Children[i]).Visibility = Visibility.Collapsed;
            }
        }

        public void UpdateStatus()
        {
            // progressbars etc
            tbCoinValue.Text = PLAYER.coins.ToString();
            tbSlime.Text = PET.name + " (" + PET.age + " | " + PET.weight + " dkg)";

            pbHunger.Value = PET.hunger;
            pbFatigue.Value = PET.fatigue;
            pbHealth.Value = PET.health;
            pbFun.Value = PET.fun;
        }
    }
}
