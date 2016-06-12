using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Camera
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class Application : Windows.UI.Xaml.Application
    {
        internal static string selectedString;
        internal static string selectedUserNamePassword;
        internal static ObservableCollection<string> addressListS { get; private set; }
        internal static ObservableCollection<string> userPasswd;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public Application()
        {
            this.InitializeComponent();

            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            object value = Convert.ToString(localSettings.Values["Tema"]);
            if (value == null || value.ToString() == "")
            {
                Application.Current.RequestedTheme = ApplicationTheme.Dark;
            }
            else
            {
                string s = Convert.ToString(value);
                if (s.ToLower().Contains("light"))
                    Application.Current.RequestedTheme = ApplicationTheme.Light;
            }
            this.Suspending += OnSuspending;
            addressListS = new ObservableCollection<string>();
            userPasswd = new ObservableCollection<string>();
            selectedUserNamePassword = "";
            selectedString = "";
        }


        public static string ToXml(ObservableCollection<string> value)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<string>));
            StringBuilder stringBuilder = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings()
            {
                Indent = true,
                OmitXmlDeclaration = true,
            };
            using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder, settings))
            {
                serializer.Serialize(xmlWriter, value);
            }
            return stringBuilder.ToString();
        }

        public static ObservableCollection<string> FromXml(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<string>));
            ObservableCollection<string> value;
            using (StringReader stringReader = new StringReader(xml))
            {
                object deserialized = serializer.Deserialize(stringReader);
                value = (ObservableCollection<string>)deserialized;
            }
            return value;
        }



        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

               // if ( (e.PreviousExecutionState == ApplicationExecutionState.Terminated) || (e.PreviousExecutionState == ApplicationExecutionState.ClosedByUser))
                {
                    var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                    object value = Convert.ToString(localSettings.Values["AddressList"]);
                    if (value == null || value.ToString()=="")
                    {
                        addressListS.Clear();
                        userPasswd.Clear();
                    }
                    else
                    {
                        string s = Convert.ToString(value);
                        addressListS = FromXml(s);
                        value = Convert.ToString(localSettings.Values["UserPasswdList"]);
                        if (value == null)
                            userPasswd.Clear();
                        else
                        {
                            s = Convert.ToString(value);
                            userPasswd = FromXml(s);
                        }
                    }

                    

                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            string localData = ToXml(addressListS);
            
            localSettings.Values["AddressList"] = localData;
            string localPass = ToXml(userPasswd);
            localSettings.Values["UserPasswdList"] = localPass;

            deferral.Complete();
        }

       
    }
}
