using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using UWPIoTDevice;

static class AzureIoTHub
{
    //
    // Note: this connection string is specific to the device "UWPDevice". To configure other devices,
    // see information on iothub-explorer at http://aka.ms/iothubgetstartedVSCS
    //

    //
    // To monitor messages sent to device "UWPDevice" use iothub-explorer as follows:
    //    iothub-explorer HostName=jongblogiothub.azure-devices.net;SharedAccessKeyName=service;SharedAccessKey=cQ/jupjdU5cTisSVTKr+XaDhX4WBK3Su26oMFApfiew= monitor-events "UWPDevice"
    //

    // Refer to http://aka.ms/azure-iot-hub-vs-cs-wiki for more information on Connected Service for Azure IoT Hub

    public static async Task SendDeviceToCloudMessageAsync()
    {
        var deviceClient = DeviceClient.CreateFromConnectionString(Config.UWPDevice, TransportType.Amqp);

#if WINDOWS_UWP
        var str = "Hello, Cloud from a UWP C# app!";
#else
        var str = "Hello, Cloud from a C# app!";
#endif
        var message = new Message(Encoding.ASCII.GetBytes(str));

        await deviceClient.SendEventAsync(message);
    }

    public static async Task<string> ReceiveCloudToDeviceMessageAsync()
    {
        var deviceClient = DeviceClient.CreateFromConnectionString(Config.UWPDevice, TransportType.Amqp);

        while (true)
        {
            var receivedMessage = await deviceClient.ReceiveAsync();

            if (receivedMessage != null)
            {
                var messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                await deviceClient.CompleteAsync(receivedMessage);
                return messageData;
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
}
