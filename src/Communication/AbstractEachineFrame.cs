using System.Collections.Generic;


public abstract class AbstractEachineFrame
{
    protected byte[] sequence;

    public AbstractEachineFrame() : this(new List<byte[]>())
    {

    }

    public AbstractEachineFrame(List<byte[]> sequence)
    {
        // Flat sequence
        // Total bytes to reserve
        int numberOfBytes = 0;
        foreach(byte[] package in sequence)
        {
            numberOfBytes += package.Length;
        }
        
        this.sequence = new byte[numberOfBytes];
        int bytePointer = 0;
        foreach(byte[] package in sequence)
        {
            package.CopyTo(this.sequence, bytePointer);
            bytePointer += package.Length;
        }
    }

    public AbstractEachineFrame(byte[] sequence)
    {
        this.sequence = sequence;
    }

    public AbstractEachineFrame(AbstractEachineFrame frame) : this(frame.sequence)
    {

    }

    public byte[] getSequence()
    {
        return this.sequence;
    }
}