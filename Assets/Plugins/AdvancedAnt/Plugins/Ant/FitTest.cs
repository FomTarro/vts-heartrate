using UnityEngine;
using Dynastream.Fit;
using System.IO;

public class FitTest : MonoBehaviour
{

    private void Start()
    {
        EncodeFitFile();
    }

    //FIT USAGE EXAMPLE
    void EncodeFitFile()
    {

        System.DateTime systemStartTime = System.DateTime.Now;
        System.DateTime systemTimeNow = systemStartTime;

        FileStream fitDest = new FileStream("ExampleMonitoringFile.fit", FileMode.Create, FileAccess.ReadWrite, FileShare.Read);

        // Create file encode object
        Encode encodeDemo = new Encode();

        // Write our header
        encodeDemo.Open(fitDest);

        // Generate some FIT messages
        FileIdMesg fileIdMesg = new FileIdMesg(); // Every FIT file MUST contain a 'File ID' message as the first message
        fileIdMesg.SetSerialNumber(54321);
        fileIdMesg.SetTimeCreated(new Dynastream.Fit.DateTime(systemTimeNow));
        fileIdMesg.SetManufacturer(Manufacturer.Dynastream);
        fileIdMesg.SetProduct(1001);
        fileIdMesg.SetNumber(0);
        fileIdMesg.SetType(Dynastream.Fit.File.Activity); // See the 'FIT FIle Types Description' document for more information about this file type.
        encodeDemo.Write(fileIdMesg); // Write the 'File ID Message'

        DeviceInfoMesg deviceInfoMesg = new DeviceInfoMesg();
        deviceInfoMesg.SetTimestamp(new Dynastream.Fit.DateTime(systemTimeNow));
        deviceInfoMesg.SetSerialNumber(54321);
        deviceInfoMesg.SetManufacturer(Manufacturer.Dynastream);
        deviceInfoMesg.SetBatteryStatus(Dynastream.Fit.BatteryStatus.Good);
        encodeDemo.Write(deviceInfoMesg);

        MonitoringMesg monitoringMesg = new MonitoringMesg();

        // By default, each time a new message is written the Local Message Type 0 will be redefined to match the new message.
        // In this case,to avoid having a definition message each time there is a DeviceInfoMesg, we can manually set the Local Message Type of the MonitoringMessage to '1'.
        // By doing this we avoid an additional 7 definition messages in our FIT file.
        monitoringMesg.LocalNum = 1;

        // Simulate some data
        System.Random numberOfCycles = new System.Random(); // Fake a number of cycles
        for (int i = 0; i < 4; i++) // Each of these loops represent a quarter of a day
        {
            for (int j = 0; j < 6; j++) // Each of these loops represent 1 hour
            {
                monitoringMesg.SetTimestamp(new Dynastream.Fit.DateTime(systemTimeNow));
                monitoringMesg.SetActivityType(Dynastream.Fit.ActivityType.Walking); // Setting this to walking will cause Cycles to be interpretted as steps.
                monitoringMesg.SetCycles(monitoringMesg.GetCycles() + numberOfCycles.Next(0, 1000)); // Cycles are accumulated (i.e. must be increasing)
                encodeDemo.Write(monitoringMesg);
                systemTimeNow = systemTimeNow.AddHours(1); // Add an hour to our contrieved timestamp
            }

            deviceInfoMesg.SetTimestamp(new Dynastream.Fit.DateTime(systemTimeNow));
            deviceInfoMesg.SetBatteryStatus(Dynastream.Fit.BatteryStatus.Good); // Report the battery status every quarter day
            encodeDemo.Write(deviceInfoMesg);
        }

        // Update header datasize and file CRC
        encodeDemo.Close();
        fitDest.Close();
        
    }





}
