using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Tamagotchi
{
    public sealed partial class PopupMenu : UserControl
    {
        public delegate void updatePetNameDelegate(string petname);
        public updatePetNameDelegate updatePetNameD;
        public PopupMenu(string defPetName)
        {
            this.InitializeComponent();

            tbPetName.Text = defPetName;
        }

        private void btApplyName_Click(object sender, RoutedEventArgs e)
        {
            updatePetNameD(tbPetName.Text);
        }

        private void btExit_Click(object sender, RoutedEventArgs e)
        {
            Popup hostPopup = this.Parent as Popup;
            hostPopup.IsOpen = false;
        }
    }
}
