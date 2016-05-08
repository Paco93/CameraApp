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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Camera
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ConfigPage : Page
    {
        int selectedIndex;
        public ConfigPage()
        {
            this.InitializeComponent();
        }

        private  void OnAdd(object sender, RoutedEventArgs e)
        {
            App.addressListS.Add(address.Text);
            App.userPasswd.Add(this.userPasswd.Text);
        }

        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            lView.ItemsSource = App.addressListS;
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = (ListView)sender;
            selectedIndex = lv.SelectedIndex;
            if (selectedIndex > -1)
            {
                App.selectedString = (string)lv.Items.ElementAt(selectedIndex);
                App.selectedUserNamePassword = (string)App.userPasswd.ElementAt(selectedIndex);
                address.Text = App.selectedString;
                this.userPasswd.Text = App.selectedUserNamePassword;
                lv.SelectedItem = lv.Items.ElementAt(selectedIndex);
            }
        }

        private void AppBar_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage), null);
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (selectedIndex < 0)
                return;
            int cp = selectedIndex;
            App.addressListS.RemoveAt(selectedIndex);
            App.userPasswd.RemoveAt(cp);

        }
    }
}
