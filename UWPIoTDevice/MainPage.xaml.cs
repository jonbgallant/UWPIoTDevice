using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWPIoTDevice
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        CancellationTokenSource cts;

        public MainPage()
        {
            this.InitializeComponent();

            Task.Run(
                async () => {
                    while (true) {
                        var message = await AzureIoTHub.ReceiveCloudToDeviceMessageAsync();
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                            textBlock.Text += Environment.NewLine + message;
                        });
                    }
                }
                );
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            cts = new CancellationTokenSource();
            var number = int.Parse(textBox.Text);
            Task.Run(async () => { await SendDeviceToCloudMessageAsync(number); });
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            cts.Cancel();
            Task.Delay(2500);
            cts.Dispose();
        }

        public async Task SendDeviceToCloudMessageAsync(int number)
        {
            var deviceClient = DeviceClient.CreateFromConnectionString(Config.UWPDevice, TransportType.Amqp);
            int i = 0;

            while (i < number && !cts.Token.IsCancellationRequested)
            {
                var participant = new Participant
                {
                    Position = new Random().Next(1, 10),
                    Count = new Random().Next(0, 10000)
                };

                var message = new Message(
                    Encoding.UTF8.GetBytes(
                        JsonConvert.SerializeObject(participant)
                    ));

                await deviceClient.SendEventAsync(message);

                await Task.Delay(500, cts.Token);

                i++;
            }
        }
    }
}
