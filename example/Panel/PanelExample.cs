using UnityEngine;
using UnityEngine.UI;


public class PanelExample : MonoBehaviour
{
    // Main text to show the raw content
    public Text yawText, pitchText, rollText, flightMode, distance, speedH, altitude, speedV, gpsNumberOfSatellites, gpsSignal, gpsLatitude, gpsLongitude, battery;
    // Core panel
    private EachinePanelClient client;

    void Start()
    {
        // Initialize
        this.client = new EachinePanelClient();
        // Start to receive data
        this.client.start();
    }

    void Update()
    {
        // Get the most recent frame and update unity text objects with the current value
        EachinePanelFrame frame = (EachinePanelFrame)client.getFrame();
        yawText.text = "Yaw: " + frame.getYaw();
        rollText.text = "Roll: " + frame.getRoll();
        pitchText.text = "Pitch: " + frame.getPitch();
        flightMode.text = "Flight mode: " + frame.getFlightMode();
        distance.text = "Distance: " + frame.getDistance();
        speedH.text = "Horizontal speed: " + frame.getHorizontalSpeed();
        altitude.text = "Altitude: " + frame.getAltitude();
        speedV.text = "Vertical speed: " + frame.getVerticalSpeed();
        gpsNumberOfSatellites.text = "GPS number of satellites: " + frame.getGpsNumberOfSatellites();
        gpsSignal.text = "GPS signal: " + frame.getGpsSignal();
        gpsLatitude.text = "GPS latitude: " + frame.getFormattedLatitude();
        gpsLongitude.text = "GPS longitude: " + frame.getFormattedLongitude();
        battery.text = "Battery: " + frame.getBattery() + "%";
    }

    void OnDestroy()
    {
        // Stop receiving data
        this.client.stop();
    }
}
