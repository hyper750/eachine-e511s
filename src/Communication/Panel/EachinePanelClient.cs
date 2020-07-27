using System.Net.Sockets;
using System.Collections.Generic;


public class EachinePanelClient : AbstractEachineClient
{
    private static readonly byte[] DATA_TO_SEND = new byte[]
    {
        0x46, 0x48, 0x3c, 0x64, 0x01, 0x00, 0x01, 0x64
    };

    private static readonly int[] SEQUENCE = new int[]
    {
        // Sequence packages with its size
        // 1 package with a size of 31 bytes
        EachinePanelFrame.BUFFER_LENGTH
    };

    public EachinePanelClient(string ip = "192.168.0.1", int port = 50000) : base(SocketType.Dgram, ProtocolType.Udp, ip, port)
    {

    }

    public override AbstractEachineFrame newDefaultFrame()
    {
        return new EachinePanelFrame();
    }

    public override AbstractEachineFrame newFrame(List<byte[]> sequence)
    {
        return new EachinePanelFrame(sequence);
    }

    public override AbstractEachineFrame copyFrame(AbstractEachineFrame frame)
    {
        return new EachinePanelFrame(frame);
    }

    public override void setFrame(AbstractEachineFrame frame)
    {
        EachinePanelFrame panelFrame = (EachinePanelFrame)frame;
        // Set only the frame that have the correct checksum
        if(panelFrame.IsContentCorrect())
        {
            base.setFrame(frame);
        }
    }

    public override int[] bufferSize()
    {
        return EachinePanelClient.SEQUENCE;
    }

    public override void sendAfterSocketCreated()
    {
        this.client.Send(EachinePanelClient.DATA_TO_SEND);
    }
}
