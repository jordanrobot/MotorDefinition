using JordanRobot.MotorDefinitions;
using System.IO;

namespace CurveEditor.Tests.Services;

public class MotorFileVariablePointsLoadTests
{
    [Fact]
    public void Load_ExampleMotor_With21PointVoltage_Succeeds()
    {
        var repoRoot = Directory.GetCurrentDirectory();
        var filePath = Path.GetFullPath(Path.Combine(repoRoot, "..", "..", "..", "schema", "example-motor.json"));

        Assert.True(File.Exists(filePath), $"Test file not found: {filePath}");

        var motor = MotorFile.Load(filePath);

        // This file contains a drive with a voltage configured at 5% increments (21 points).
        var drive = motor.Drives.Single(d => d.Name == "K5300 7.5kW");
        var voltage = drive.Voltages.Single(v => v.Voltage == 208);

        Assert.Equal(21, voltage.Series.Single(s => s.Name == "Peak").Data.Count);
        Assert.Equal(21, voltage.Series.Single(s => s.Name == "Continuous").Data.Count);

        // Sanity-check endpoints line up with the file.
        var peak = voltage.Series.Single(s => s.Name == "Peak");
        Assert.Equal(0, peak.Data[0].Percent);
        Assert.Equal(100, peak.Data[^1].Percent);
        Assert.Equal(0, peak.Data[0].Rpm);
        Assert.Equal(4000, peak.Data[^1].Rpm);
    }
}
