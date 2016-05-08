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
using System.Threading;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Security.Cryptography.Certificates;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http.Headers;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Camera
{
    public enum NotifyType
    {
        StatusMessage,
        ErrorMessage
    };

    public class PlugInFilter : IHttpFilter
    {
        private IHttpFilter innerFilter;

        public PlugInFilter(IHttpFilter innerFilter)
        {
            if (innerFilter == null)
            {
                throw new ArgumentException("innerFilter cannot be null.");
            }
            this.innerFilter = innerFilter;
        }

        public IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> SendRequestAsync(HttpRequestMessage request)
        {
            return AsyncInfo.Run<HttpResponseMessage, HttpProgress>(async (cancellationToken, progress) =>
            {
                request.Headers.Add("Custom-Header", "CustomRequestValue");
                HttpResponseMessage response = await innerFilter.SendRequestAsync(request).AsTask(cancellationToken, progress);

                cancellationToken.ThrowIfCancellationRequested();

                response.Headers.Add("Custom-Header", "CustomResponseValue");
                return response;
            });
        }

        public void Dispose()
        {
            innerFilter.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    internal static class Helpers
    {
        internal static async Task DisplayTextResultAsync(
            HttpResponseMessage response,
            TextBox output,
            CancellationToken token)
        {
          
            output.Text += SerializeHeaders(response);
            string responseBodyAsText = await response.Content.ReadAsStringAsync().AsTask(token);

            token.ThrowIfCancellationRequested();

            // Insert new lines.
             responseBodyAsText = responseBodyAsText.Replace("<br>", Environment.NewLine);

            output.Text += responseBodyAsText;
        }

        internal static string SerializeHeaders(HttpResponseMessage response)
        {
            StringBuilder output = new StringBuilder();

            // We cast the StatusCode to an int so we display the numeric value (e.g., "200") rather than the
            // name of the enum (e.g., "OK") which would often be redundant with the ReasonPhrase.
            output.Append(((int)response.StatusCode) + " " + response.ReasonPhrase + "\r\n");

            SerializeHeaderCollection(response.Headers, output);
            SerializeHeaderCollection(response.Content.Headers, output);
            output.Append("\r\n");
            return output.ToString();
        }

        internal static void SerializeHeaderCollection(
            IEnumerable<KeyValuePair<string, string>> headers,
            StringBuilder output)
        {
            foreach (var header in headers)
            {
                output.Append(header.Key + ": " + header.Value + "\r\n");
            }
        }

        internal static void CreateHttpClient(ref HttpClient httpClient)
        {
            if (httpClient != null)
            {
                httpClient.Dispose();
            }

            // HttpClient functionality can be extended by plugging multiple filters together and providing
            // HttpClient with the configured filter pipeline.
            IHttpFilter filter = new HttpBaseProtocolFilter();
            filter = new PlugInFilter(filter); // Adds a custom header to every request and response message.
            httpClient = new HttpClient(filter);

            // The following line sets a "User-Agent" request header as a default header on the HttpClient instance.
            // Default headers will be sent with every request sent from this HttpClient instance.
            httpClient.DefaultRequestHeaders.UserAgent.Add(new HttpProductInfoHeaderValue("Sample", "v8"));
        }

        internal static void ScenarioStarted(Button startButton, Button cancelButton)
        {
            startButton.IsEnabled = false;
            cancelButton.IsEnabled = true;
           
        }

        internal static void ScenarioCompleted(Button startButton, Button cancelButton)
        {
            startButton.IsEnabled = true;
            cancelButton.IsEnabled = false;
        }

        internal static void ReplaceQueryString(TextBox addressField, string newQueryString)
        {
            string resourceAddress = addressField.Text;

            // Remove previous query string.
            int questionMarkIndex = resourceAddress.IndexOf("?", StringComparison.Ordinal);
            if (questionMarkIndex != -1)
            {
                resourceAddress = resourceAddress.Substring(0, questionMarkIndex);
            }

            addressField.Text = resourceAddress + newQueryString;
        }

        internal static bool TryGetUri(string uriString, out Uri uri)
        {
            // Note that this app has both "Internet (Client)" and "Home and Work Networking" capabilities set,
            // since the user may provide URIs for servers located on the internet or intranet. If apps only
            // communicate with servers on the internet, only the "Internet (Client)" capability should be set.
            // Similarly if an app is only intended to communicate on the intranet, only the "Home and Work
            // Networking" capability should be set.
            if (!Uri.TryCreate(uriString.Trim(), UriKind.Absolute, out uri))
            {
                return false;
            }

            if (uri.Scheme != "http" && uri.Scheme != "https")
            {
                return false;
            }

            return true;
        }
    }
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MainPage Current;
        MainPage rootPage;

        private HttpBaseProtocolFilter filter;
        private HttpClient httpClient;
        private CancellationTokenSource cts;
     
        public MainPage()
        {
            this.InitializeComponent();
            Current = this;
            rootPage = MainPage.Current;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // In this scenario we just create an HttpClient instance with default settings. I.e. no custom filters. 
            // For examples on how to use custom filters see other scenarios.
            filter = new HttpBaseProtocolFilter();

            httpClient = new HttpClient(filter);
            cts = new CancellationTokenSource();
           

            HttpVersion httpVersion = HttpVersion.Http11;
            httpVersion = HttpVersion.Http20;
            filter.MaxVersion = httpVersion;

            var byteArray = Encoding.ASCII.GetBytes(App.selectedUserNamePassword);
            httpClient.DefaultRequestHeaders.Authorization = new Windows.Web.Http.Headers.HttpCredentialsHeaderValue("Basic", Convert.ToBase64String(byteArray));

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // If the navigation is external to the app do not clean up.
            // This can occur on Phone when suspending the app.
            if (e.NavigationMode == NavigationMode.Forward && e.Uri == null)
            {
                return;
            }

            Dispose();
        }

        public void Dispose()
        {
            if (filter != null)
            {
                filter.Dispose();
                filter = null;
            }

            if (httpClient != null)
            {
                httpClient.Dispose();
                httpClient = null;
            }

            if (cts != null)
            {
                cts.Dispose();
                cts = null;
            }
        }

        /// <summary>
        /// Used to display messages to the user
        /// </summary>
        /// <param name="strMessage"></param>
        /// <param name="type"></param>
        public void NotifyUser(string strMessage, NotifyType type)
        {
            switch (type)
            {
                case NotifyType.StatusMessage:
                    StatusBorder.Background = new SolidColorBrush(Windows.UI.Colors.Green);
                    break;
                case NotifyType.ErrorMessage:
                    StatusBorder.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                    break;
            }
            StatusBlock.Text = strMessage;

            // Collapse the StatusBlock if it has no text to conserve real estate.
            StatusBorder.Visibility = (StatusBlock.Text != String.Empty) ? Visibility.Visible : Visibility.Collapsed;
            if (StatusBlock.Text != String.Empty)
            {
                StatusBorder.Visibility = Visibility.Visible;
                StatusPanel.Visibility = Visibility.Visible;
            }
            else
            {
                StatusBorder.Visibility = Visibility.Collapsed;
                StatusPanel.Visibility = Visibility.Collapsed;
            }
        }

        private async void  OnClick(object sender, RoutedEventArgs e)
        {
            Uri resourceUri;

            // The value of 'AddressField' is set by the user and is therefore untrusted input. If we can't create a
            // valid, absolute URI, we'll notify the user about the incorrect input.
            if (!Helpers.TryGetUri(App.selectedString, out resourceUri))
            {
                rootPage.NotifyUser("Invalid URI.", NotifyType.ErrorMessage);
                return;
            }

            Helpers.ScenarioStarted(StartButton, CancelButton);
            rootPage.NotifyUser("In progress", NotifyType.StatusMessage);

            try
            {
                filter.CacheControl.ReadBehavior = HttpCacheReadBehavior.MostRecent;
                // ---------------------------------------------------------------------------
                // WARNING: Only test applications should ignore SSL errors.
                // In real applications, ignoring server certificate errors can lead to MITM
                // attacks (while the connection is secure, the server is not authenticated).
                //
                // The SetupServer script included with this sample creates a server certificate that is self-signed
                // and issued to fabrikam.com, and hence we need to ignore these errors here. 
                // ---------------------------------------------------------------------------
                filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.Untrusted);
                filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.InvalidName);
                filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.Expired);
                HttpResponseMessage response = await httpClient.GetAsync(resourceUri).AsTask(cts.Token);
                //         IBuffer rb = await httpClient.GetBufferAsync(resourceUri).AsTask(cts.Token); 
                IBuffer rb = await response.Content.ReadAsBufferAsync();
                byte[] by = rb.ToArray();

                if (by != null)
                {
                    using (var stream = new InMemoryRandomAccessStream())
                    {
                        await stream.WriteAsync(by.AsBuffer());
                        var image = new BitmapImage();
                        stream.Seek(0);
                        image.SetSource(stream);
                        ImageControl.Source = image;
                    }
                }

                string text= Helpers.SerializeHeaders(response);

//                await Helpers.DisplayTextResultAsync(response, OutputField, cts.Token);

                rootPage.NotifyUser(
                    "Completed. Response came from " + response.Source + ". HTTP version used: " + response.Version.ToString() + ".",
                    NotifyType.StatusMessage);
            }
            catch (TaskCanceledException)
            {
                rootPage.NotifyUser("Request canceled.", NotifyType.ErrorMessage);
            }
            catch (Exception ex)
            {
                rootPage.NotifyUser("Error: " + ex.Message, NotifyType.ErrorMessage);
            }
            finally
            {
                Helpers.ScenarioCompleted(StartButton, CancelButton);
            }
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            cts.Cancel();
            cts.Dispose();
            // Re-create the CancellationTokenSource.
            cts = new CancellationTokenSource();
        }

        private void GoToConfigButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ConfigPage), null);
        }
    }
}
