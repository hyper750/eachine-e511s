using System.Collections.Generic;
using System;


public class EachinePanelFrame : AbstractEachineFrame
{
    public const byte BUFFER_LENGTH = 31;
    private static readonly Dictionary<byte, string> FLIGHT_MODE_GROUP = new Dictionary<byte, string>()
    {
        // First 4 bits
        {0x0, "Self-stabilizing"},
        {0x1, "Point"},
        {0x2, "Takeoff"},
        {0x3, "Landing"},
        {0x4, "Homeward"},
        {0x5, "WayPoint"},
        {0x6, "Follow"},
        {0x7, "Orbit"},
    };

    private static readonly Dictionary<byte, string> FLIGHT_MODE_TYPE = new Dictionary<byte, string>()
    {
        // Second 4 bits
        {0x0, "Lock"},
        {0x1, "Unlock"},
        {0x2, "Unlock&Take off"},
        {0x3, "Out of control"},
        {0x4, "Homeward 1"},
        {0x5, "Homeward 2"},
        {0x6, "Homeward"},
        {0x7, "Low power"},
        {0x8, "Landing"},
        {0x9, "Take off"},
    };

    private static readonly Dictionary<byte, string> GPS_SIGNAL = new Dictionary<byte, string>()
    {
        {0x0, "GPS satellite signal search, please wait!"},
        {0x1, "GPS signal is correct, ready to take off"}
    };

    private const double SIMPLE_FLOAT = 0.1;
    private const double COMPLEX_FLOAT = 0.32809948882276646;

    // Cooked data
    private double roll, pitch, yaw, distance, speedH, altitude, speedV, gpsLatitude, gpsLongitude;
    private byte gpsNumberStellites, battery, checksum;
    private string flightMode, gpsSignal;

    public EachinePanelFrame() : this(new List<byte[]>(){
        new byte[EachinePanelFrame.BUFFER_LENGTH]
    })
    {

    }
    
    public EachinePanelFrame(List<byte[]> sequence) : base(sequence)
    {
        // Roll unsigned 2 bytes
        this.roll = BitConverter.ToUInt16(this.sequence, 6) * EachinePanelFrame.SIMPLE_FLOAT;

        // Pitch unsigned 2 bytes
        this.pitch = BitConverter.ToUInt16(this.sequence, 8) * EachinePanelFrame.SIMPLE_FLOAT;

        // Yaw signed 2 bytes
        this.yaw = BitConverter.ToInt16(this.sequence, 10) * EachinePanelFrame.SIMPLE_FLOAT;

        // Flight mode string
        byte rawFlightMode = this.sequence[12];

        // Eg: 0x36
        // Flight group will be 3
        byte flightGroupValue = (byte)(rawFlightMode >> 4);
        string flightGroup = EachinePanelFrame.FLIGHT_MODE_GROUP[0];
        if (EachinePanelFrame.FLIGHT_MODE_GROUP.ContainsKey(flightGroupValue))
        {
            flightGroup = EachinePanelFrame.FLIGHT_MODE_GROUP[flightGroupValue];
        }

        // Flight type will be 6
        byte flightTypeValue = (byte)(rawFlightMode & 0x0f);
        string flightType = EachinePanelFrame.FLIGHT_MODE_TYPE[0];
        if (EachinePanelFrame.FLIGHT_MODE_TYPE.ContainsKey(flightTypeValue))
        {
            flightType = EachinePanelFrame.FLIGHT_MODE_TYPE[flightTypeValue];
        }

        this.flightMode = flightGroup + "/" + flightType;

        // Distance unsigned 2 bytes
        this.distance = Math.Round(BitConverter.ToUInt16(this.sequence, 13) * EachinePanelFrame.COMPLEX_FLOAT, 1);

        // Horizontal speed signed 1 byte
        // There's no method to convert byte to signed byte
        // Use temporal byte
        byte[] speedHBuffer = new byte[] { 0x0, this.sequence[15] };
        // After convert to signed byte remove the temporal byte with bitwise right shift
        this.speedH = Math.Round((BitConverter.ToInt16(speedHBuffer, 0) >> 8) * EachinePanelFrame.COMPLEX_FLOAT, 1);

        // Altitude signed 2 bytes
        this.altitude = Math.Round(BitConverter.ToInt16(this.sequence, 16) * EachinePanelFrame.COMPLEX_FLOAT, 1);

        // SpeedV signed 1 byte
        byte[] speedVBuffer = new byte[] { 0x0, this.sequence[18] };
        this.speedV = Math.Round((BitConverter.ToInt16(speedVBuffer, 0) >> 8) * EachinePanelFrame.COMPLEX_FLOAT, 1);

        // Number of satellites GPS n/s unsigned 1 byte
        this.gpsNumberStellites = this.sequence[19];

        // Gps signal unsigned 1 byte (0 or 1)
        byte gpsSignalValue = this.sequence[20];
        string gpsSignal = EachinePanelFrame.GPS_SIGNAL[0];
        if (EachinePanelFrame.GPS_SIGNAL.ContainsKey(gpsSignalValue))
        {
            gpsSignal = EachinePanelFrame.GPS_SIGNAL[gpsSignalValue];
        }
        this.gpsSignal = gpsSignal;

        // Gps latitude signed 4 bytes
        this.gpsLatitude = Math.Round(BitConverter.ToInt32(this.sequence, 21) * EachinePanelFrame.COMPLEX_FLOAT, 6);

        // Gps longitude signed 4 bytes
        this.gpsLongitude = Math.Round(BitConverter.ToInt32(this.sequence, 25) * EachinePanelFrame.COMPLEX_FLOAT, 6);

        // Battery percentage unsigned 1 byte
        this.battery = this.sequence[29];

        // Checksum of the package unsigned 1 byte
        this.checksum = this.sequence[30];
    }

    public EachinePanelFrame(AbstractEachineFrame frame) : this(new List<byte[]>(){((EachinePanelFrame)frame).sequence})
    {

    }

    public byte computeChecksum()
    {
        byte computeChecksum = 0;

        // Skip lewei signature
        for(byte i = 3; i < this.sequence.Length - 1; i++)
        {
            computeChecksum ^= this.sequence[i];
        }

        return computeChecksum;
    }

    public bool IsContentCorrect()
    {
        // Computed checksum and if the checksum inside the package and the computed checksum is the same, the package is correct
        return this.checksum == this.computeChecksum();
    }

    public double getRoll()
    {
        return this.roll;
    }

    public double getPitch()
    {
        return this.pitch;
    }

    public double getYaw()
    {
        return this.yaw;
    }

    public string getFlightMode()
    {
        return this.flightMode;
    }

    public double getDistance()
    {
        return this.distance;
    }

    public double getHorizontalSpeed()
    {
        return this.speedH;
    }

    public double getVerticalSpeed()
    {
        return this.speedV;
    }

    public double getAltitude()
    {
        return this.altitude;
    }

    public byte getGpsNumberOfSatellites()
    {
        return this.gpsNumberStellites;
    }

    public string getGpsSignal()
    {
        return this.gpsSignal;
    }

    public double getLatitude()
    {
        return this.gpsLatitude;
    }

    public string getFormattedLatitude()
    {
        // Positive latitude for North
        // Negative latitude for South
        return ((this.gpsLatitude >= 0) ? "N" : "S") + this.gpsLatitude;
    }

    public double getLongitude()
    {
        return this.gpsLongitude;
    }

    public string getFormattedLongitude()
    {
        // Positive longitude for East
        // Negative longitude for West
        return ((this.gpsLongitude >= 0) ? "E" : "W") + this.gpsLongitude;
    }

    public byte getBattery()
    {
        return this.battery;
    }

    public byte getChecksum()
    {
        return this.checksum;
    }
}
