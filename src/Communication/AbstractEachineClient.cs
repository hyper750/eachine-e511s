using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;


public abstract class AbstractEachineClient
{
    private IPEndPoint serverEndpoint;
    protected Socket client;
    private SocketType socketType;
    private ProtocolType protocolType;
    private Thread socketThread;
    private Mutex mutex;
    private bool continueRecivingData;
    private AbstractEachineFrame frame;

    public AbstractEachineClient(SocketType socketType, ProtocolType protocolType, string ip, int port)
    {
        this.continueRecivingData = false;
        this.serverEndpoint = new IPEndPoint(IPAddress.Parse(ip), port);
        this.socketType = socketType;
        this.protocolType = protocolType;

        this.mutex = new Mutex();

        // Default generic frame value
        this.frame = this.newDefaultFrame();
    }

    ~AbstractEachineClient()
    {
        this.stop();
        if(this.socketThread != null)
        {
            this.socketThread.Join();
        }

        if (this.client.Connected)
        {
            this.client.Shutdown(SocketShutdown.Both);
        }
        this.client.Close();

        this.mutex.Close();
    }

    public abstract AbstractEachineFrame newDefaultFrame();
    public abstract AbstractEachineFrame newFrame(List<byte[]> sequence);
    public abstract AbstractEachineFrame copyFrame(AbstractEachineFrame frame);

    public void start()
    {
        this.mutex.WaitOne();

        if (!this.continueRecivingData && (this.socketThread == null || !this.socketThread.IsAlive))
        {
            this.continueRecivingData = true;
            this.socketThread = new Thread(new ThreadStart(receiveData));
            this.socketThread.Start();
        }

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

        this.mutex.WaitOne();
        result = this.continueRecivingData;
        this.mutex.ReleaseMutex();

        return result;
    }

    public virtual AbstractEachineFrame getFrame()
    {
        AbstractEachineFrame frame;

        this.mutex.WaitOne();
        frame = this.copyFrame(this.frame);
        this.mutex.ReleaseMutex();

        return frame;
    }

    public virtual void setFrame(AbstractEachineFrame frame)
    {
        this.mutex.WaitOne();
        this.frame = this.copyFrame(frame);
        this.mutex.ReleaseMutex();
    }

    public virtual void sendAfterSocketCreated()
    {

    }

    // Package size of each sequence, for example the sequence have 4 packages
    // 1. Package have x bytes of buffer
    // 2. Package have y bytes of buffer
    // 3. Package have z bytes of buffer
    // 4. Package have i bytes of buffer
    // [x, y, z, i]
    public abstract int[] bufferSize();

    public virtual void notifyPackageReceived(int packageNumber, byte[] buffer)
    {

    }

    private void receiveData()
    {
        while (this.isRecivingData())
        {
            try
            {
                if (this.client == null)
                {
                    // Connect and send who am i to receive the data
                    this.client = new Socket(this.serverEndpoint.AddressFamily, this.socketType, this.protocolType);
                    this.client.Connect(this.serverEndpoint);

                    // Must send something after creating the socket
                    this.sendAfterSocketCreated();
                }

                // List of packages
                // [[buffer first package], [buffer second package] ... ]
                List<byte[]> sequence = new List<byte[]>();

                // Receive data package
                for (int packageNumber = 0; packageNumber < this.bufferSize().Length; packageNumber++)
                {
                    // Build buffer size for each package of the sequence
                    int bufferSize = this.bufferSize()[packageNumber];
                    byte[] buffer = new byte[bufferSize];

                    // Bytes received of the package
                    int totalBytesReceived = 0;
                    while (totalBytesReceived < bufferSize)
                    {
                        totalBytesReceived += this.client.Receive(buffer);
                    }

                    this.notifyPackageReceived(packageNumber + 1, buffer);
                    sequence.Add(buffer);
                }

                this.setFrame(this.newFrame(sequence));
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
