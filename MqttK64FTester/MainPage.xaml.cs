using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MQTTnet.Client;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.UI.Xaml.Shapes;
using MQTTnet.Client.Options;
using MQTTnet;
using MQTTnet.Client.Connecting;
using System.Text;
using MQTTnet.Client.Receiving;
using Windows.UI.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MqttK64FTester
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, IMqttApplicationMessageReceivedHandler
    {
        private IMqttClient _client;
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer("192.168.1.246")
                .Build();
            var factory = new MqttFactory();
            _client = factory.CreateMqttClient();
            _client.ApplicationMessageReceivedHandler = this;
            /*var result = await _client.ConnectAsync(options);
            if(result.ResultCode == MqttClientConnectResultCode.Success)
            {
                await _client.SubscribeAsync("device/led_hw/+/r");
            }
            */
        }

        private async Task SendMessage(string topic, string payload)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(Encoding.ASCII.GetBytes(payload))
                .Build();
            await _client.PublishAsync(message);
        }

        public async Task ChangeColor(Ellipse ellipse, SolidColorBrush color)
            => await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => ellipse.Fill = color);

        public async Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs eventArgs)
        {
            var topic = eventArgs.ApplicationMessage.Topic;
            var payload = eventArgs.ApplicationMessage.Payload;
            var message = payload is null ? string.Empty : Encoding.ASCII.GetString(payload);
            var splittedTopic = topic.Split('/');
            if (splittedTopic.Length < 3) return;
            var deviceNumber = splittedTopic[2];
            var ellipse = deviceNumber == "1" ? RedLed : GreenLed;
            await ChangeColor(ellipse, GetColor(ellipse, message));
        }

        private SolidColorBrush GetColor(Ellipse ellipse, string message)
        {
            if (message == "OFF") return new SolidColorBrush(Colors.Black);
            else return ellipse == RedLed ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Green);
        }

        private async void RedButton_Click(object sender, RoutedEventArgs e)
            => await SendMessage("device/led_hw/1", string.Empty);

        private async void GreenButton_Click(object sender, RoutedEventArgs e)
            => await SendMessage("device/led_hw/2", string.Empty);

        private async void VirtualButton_Click(object sender, RoutedEventArgs e)
            => await SendMessage("device/button/v", string.Empty);
    }
}
