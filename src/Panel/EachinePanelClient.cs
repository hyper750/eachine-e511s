using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;


public class EachinePanelClient
{
    // Milliseconds to refresh the last frame
    private const int DATA_REFRESH = 100;
    private static readonly byte[] DATA_TO_SEND = new byte[]
    {
        // Start at 4 byte
        0x46, 0x48, 0x3c, 0x64, 0x01, 0x00, 0x01, 0x64
    };

    private IPEndPoint serverEndpoint;
    private Socket client;
    private Thread socketThread;
    private Mutex mutex;
    private bool continueRecivingData;
    private long lastDataReceivedTimestamp;
    private EachinePanelFrame frame;

    public EachinePanelClient(string ip = "192.168.0.1", int port = 50000)
    {
        this.mutex = new Mutex();
        this.continueRecivingData = false;
        this.serverEndpoint = new IPEndPoint(IPAddress.Parse(ip), port);
        this.socketThread = new Thread(new ThreadStart(receiveData));
        this.lastDataReceivedTimestamp = Timer.GetTimestamp();

        // Default frame value
        this.frame = new EachinePanelFrame();
    }

    ~EachinePanelClient()
    {
        this.stop();
        this.socketThread.Join();

        if (this.client.Connected)
        {
            this.client.Shutdown(SocketShutdown.Both);
        }
        this.client.Close();
        
        this.mutex.Close();
    }

    public void start()
    {
        this.mutex.WaitOne();
        this.continueRecivingData = true;
        this.socketThread.Start();
        this.mutex.ReleaseMutex();
    }

    public void stop()
    {
        this.mutex.WaitOne();
        this.continueRecivingData = false;
        this.mutex.ReleaseMutex();
    }
    public bool isRecivingData()
    {
        bool result;
        // Copy looking if anyone is not writing/reading the value
        this.mutex.WaitOne();
        result = this.continueRecivingData;
        this.mutex.ReleaseMutex();

        return result;
    }

    public EachinePanelFrame getFrame()
    {
        EachinePanelFrame frame;
        this.mutex.WaitOne();
        frame = new EachinePanelFrame(this.frame);
        this.mutex.ReleaseMutex();
        return frame;
    }

    public void setFrame(EachinePanelFrame frame)
    {
        this.mutex.WaitOne();
        this.frame = frame;
        this.mutex.ReleaseMutex();
    }

    public void receiveData()
    {
        while (this.isRecivingData())
        {
            try
            {
                if (this.client == null)
                {
                    // Connect and send who am i to receive the data
                    this.client = new Socket(this.serverEndpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                    this.client.Connect(this.serverEndpoint);
                    this.client.Send(EachinePanelClient.DATA_TO_SEND);
                }
                long currentTimestamp = Timer.GetTimestamp();
                long differenceTs = Timer.getElapsedTime(currentTimestamp, this.lastDataReceivedTimestamp, Timer.Type.MILLISECOND);
                if (EachinePanelClient.DATA_REFRESH < differenceTs)
                {
                    int millisecondsToSleep = (int)(EachinePanelClient.DATA_REFRESH - differenceTs);
                    Thread.Sleep(millisecondsToSleep);
                }
                this.lastDataReceivedTimestamp = Timer.GetTimestamp();
                // Receive data package
                byte[] rawContent = new byte[EachinePanelFrame.BUFFER_LENGTH];
                int byteReceived = this.client.Receive(rawContent);
                EachinePanelFrame currentFrame = new EachinePanelFrame(rawContent);
                if (currentFrame.IsContentCorrect())
                {
                    this.setFrame(currentFrame);
                }
            }
            catch (SocketException)
            {
                // Close the socket
                if (this.client.Connected)
                {
                    this.client.Shutdown(SocketShutdown.Both);
                }
                this.client.Close();
                this.client = null;
            }
        }
    }
}
