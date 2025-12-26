using JordanRobot.MotorDefinitions.Model;
using CurveEditor.Services;
using JordanRobot.MotorDefinitions;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CurveEditor.Tests.Services;

public class FileServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly FileService _service;

    public FileServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
        _service = new FileService();
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_tempDir, recursive: true);
        }
        catch
        {
        }

        GC.SuppressFinalize(this);
    }

    private static MotorDefinition CreateTestMotorDefinition()
    {
        var motor = new MotorDefinition("Test Motor")
        {
            Manufacturer = "Test Mfg",
            PartNumber = "TM-1234",
            MaxSpeed = 5000,
            RatedPeakTorque = 55.0,
            RatedContinuousTorque = 45.0,
            HasBrake = true,
            BrakeReleaseTime = 12,
            BrakeEngageTimeDiode = 5,
            BrakeEngageTimeMov = 7,
            BrakeBacklash = 0.5,
            Power = 1500
        };

        motor.Units.ResponseTime = "ms";
        motor.Units.Percentage = "%";
        motor.Units.Temperature = "C";

        var drive = motor.AddDrive("Test Drive");
        var voltage = drive.AddVoltageConfiguration(220);
        voltage.MaxSpeed = 5000;
        voltage.Power = 1500;
        voltage.RatedPeakTorque = 55.0;
        voltage.RatedContinuousTorque = 45.0;

        var series = new CurveSeries("Peak");
        series.InitializeData(5000, 55.0);
        voltage.Series.Add(series);

        return motor;
    }

    private static CurveSeries[] GetAllSeries(MotorDefinition motor)
    {
        return motor.Drives
            .SelectMany(d => d.Voltages)
            .SelectMany(v => v.Series)
            .ToArray();
    }

    [Fact]
    public async Task LoadAsync_ValidFile_ReturnsMotorDefinition()
    {
        var motor = CreateTestMotorDefinition();
        var filePath = Path.Combine(_tempDir, "test-motor.json");
        await MotorFile.SaveAsync(motor, filePath);

        var loaded = await new FileService().LoadAsync(filePath);

        Assert.NotNull(loaded);
        Assert.Equal(motor.MotorName, loaded.MotorName);
    }

    [Fact]
    public async Task LoadAsync_FileNotFound_ThrowsFileNotFoundException()
    {
        var nonExistentPath = Path.Combine(_tempDir, "non-existent.json");

        await Assert.ThrowsAsync<FileNotFoundException>(() => _service.LoadAsync(nonExistentPath));
    }

    [Fact]
    public async Task LoadAsync_InvalidJson_ThrowsInvalidOperationException()
    {
        var filePath = Path.Combine(_tempDir, "invalid.json");
        await File.WriteAllTextAsync(filePath, "{ invalid json }");

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.LoadAsync(filePath));
    }

    [Fact]
    public async Task LoadAsync_SetsCurrentFilePath()
    {
        var motor = CreateTestMotorDefinition();
        var filePath = Path.Combine(_tempDir, "test-motor.json");
        await MotorFile.SaveAsync(motor, filePath);

        var loader = new FileService();
        await loader.LoadAsync(filePath);

        Assert.Equal(filePath, loader.CurrentFilePath);
    }

    [Fact]
    public async Task LoadAsync_ClearsDirtyFlag()
    {
        var motor = CreateTestMotorDefinition();
        var filePath = Path.Combine(_tempDir, "test-motor.json");
        await MotorFile.SaveAsync(motor, filePath);

        var loader = new FileService();
        loader.MarkDirty();
        await loader.LoadAsync(filePath);

        Assert.False(loader.IsDirty);
    }

    [Fact]
    public async Task SaveAsAsync_CreatesFile()
    {
        var motor = CreateTestMotorDefinition();
        var filePath = Path.Combine(_tempDir, "new-motor.json");

        await _service.SaveAsAsync(motor, filePath);

        Assert.True(File.Exists(filePath));
    }

    [Fact]
    public async Task SaveAsAsync_UpdatesCurrentFilePath()
    {
        var motor = CreateTestMotorDefinition();
        var filePath = Path.Combine(_tempDir, "new-motor.json");

        await _service.SaveAsAsync(motor, filePath);

        Assert.Equal(filePath, _service.CurrentFilePath);
    }

    [Fact]
    public async Task SaveAsAsync_ClearsDirtyFlag()
    {
        var motor = CreateTestMotorDefinition();
        var filePath = Path.Combine(_tempDir, "new-motor.json");
        _service.MarkDirty();

        await _service.SaveAsAsync(motor, filePath);

        Assert.False(_service.IsDirty);
    }

    [Fact]
    public async Task SaveCopyAsAsync_CreatesFile()
    {
        var motor = CreateTestMotorDefinition();
        var filePath = Path.Combine(_tempDir, "copy-motor.json");

        await _service.SaveCopyAsAsync(motor, filePath);

        Assert.True(File.Exists(filePath));
    }

    [Fact]
    public async Task SaveCopyAsAsync_DoesNotUpdateCurrentFilePath()
    {
        var motor = CreateTestMotorDefinition();
        var originalPath = Path.Combine(_tempDir, "original.json");
        var copyPath = Path.Combine(_tempDir, "copy-motor.json");
        await _service.SaveAsAsync(motor, originalPath);

        await _service.SaveCopyAsAsync(motor, copyPath);

        Assert.Equal(originalPath, _service.CurrentFilePath);
    }

    [Fact]
    public async Task SaveCopyAsAsync_DoesNotClearDirtyFlag()
    {
        var motor = CreateTestMotorDefinition();
        var originalPath = Path.Combine(_tempDir, "original.json");
        var copyPath = Path.Combine(_tempDir, "copy-motor.json");
        await _service.SaveAsAsync(motor, originalPath);
        _service.MarkDirty();

        await _service.SaveCopyAsAsync(motor, copyPath);

        Assert.True(_service.IsDirty);
    }

    [Fact]
    public async Task SaveAsync_NoCurrentFile_ThrowsInvalidOperationException()
    {
        var motor = CreateTestMotorDefinition();

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.SaveAsync(motor));
    }

    [Fact]
    public async Task SaveAsync_WithCurrentFile_OverwritesFile()
    {
        var motor = CreateTestMotorDefinition();
        var filePath = Path.Combine(_tempDir, "test-motor.json");
        await _service.SaveAsAsync(motor, filePath);

        motor.MotorName = "Updated Name";
        await _service.SaveAsync(motor);

        var json = await File.ReadAllTextAsync(filePath);
        Assert.Contains("Updated Name", json);
    }

    [Fact]
    public async Task SaveAndLoad_RoundTrip_PreservesAllProperties()
    {
        var original = CreateTestMotorDefinition();
        original.Manufacturer = "Test Corp";
        original.Units.Torque = "lbf-in";
        original.Units.ResponseTime = "ms";
        original.Units.Backlash = "arcmin";
        original.Metadata.Notes = "Test notes";

        var filePath = Path.Combine(_tempDir, "roundtrip.json");
        await _service.SaveAsAsync(original, filePath);
        var loaded = await _service.LoadAsync(filePath);

        Assert.Equal(original.MotorName, loaded.MotorName);
        Assert.Equal(original.Manufacturer, loaded.Manufacturer);
        Assert.Equal(original.PartNumber, loaded.PartNumber);
        Assert.Equal(original.MaxSpeed, loaded.MaxSpeed);
        Assert.Equal(original.RatedPeakTorque, loaded.RatedPeakTorque);
        Assert.Equal(original.HasBrake, loaded.HasBrake);
        Assert.Equal(original.BrakeReleaseTime, loaded.BrakeReleaseTime);
        Assert.Equal(original.Units.Torque, loaded.Units.Torque);
        Assert.Equal(original.Units.Backlash, loaded.Units.Backlash);
        Assert.Equal(original.Metadata.Notes, loaded.Metadata.Notes);
    }

    [Fact]
    public async Task SaveAndLoad_RoundTrip_PreservesDriveVoltageSeriesHierarchy()
    {
        var original = CreateTestMotorDefinition();
        var filePath = Path.Combine(_tempDir, "hierarchy-roundtrip.json");

        await _service.SaveAsAsync(original, filePath);
        var loaded = await _service.LoadAsync(filePath);

        Assert.Equal(original.Drives.Count, loaded.Drives.Count);

        var origDrive = original.Drives[0];
        var loadDrive = loaded.Drives[0];
        Assert.Equal(origDrive.Name, loadDrive.Name);
        Assert.Equal(origDrive.Voltages.Count, loadDrive.Voltages.Count);

        var origVoltage = origDrive.Voltages[0];
        var loadVoltage = loadDrive.Voltages[0];
        Assert.Equal(origVoltage.Voltage, loadVoltage.Voltage);
        Assert.Equal(origVoltage.Power, loadVoltage.Power);
        Assert.Equal(origVoltage.MaxSpeed, loadVoltage.MaxSpeed);
        Assert.Equal(origVoltage.Series.Count, loadVoltage.Series.Count);
    }

    [Fact]
    public async Task SaveAndLoad_RoundTrip_PreservesSeriesData()
    {
        var original = CreateTestMotorDefinition();
        var filePath = Path.Combine(_tempDir, "series-roundtrip.json");

        await _service.SaveAsAsync(original, filePath);
        var loaded = await _service.LoadAsync(filePath);

        var originalAllSeries = GetAllSeries(original);
        var loadedAllSeries = GetAllSeries(loaded);

        Assert.Equal(originalAllSeries.Length, loadedAllSeries.Length);
        for (var s = 0; s < originalAllSeries.Length; s++)
        {
            var origSeries = originalAllSeries[s];
            var loadSeries = loadedAllSeries[s];

            Assert.Equal(origSeries.Name, loadSeries.Name);
            Assert.Equal(origSeries.Data.Count, loadSeries.Data.Count);

            for (var p = 0; p < origSeries.Data.Count; p++)
            {
                Assert.Equal(origSeries.Data[p].Percent, loadSeries.Data[p].Percent);
                Assert.Equal(origSeries.Data[p].Rpm, loadSeries.Data[p].Rpm);
                Assert.Equal(origSeries.Data[p].Torque, loadSeries.Data[p].Torque);
            }
        }
    }

    [Fact]
    public void MarkDirty_SetsDirtyFlag()
    {
        _service.MarkDirty();

        Assert.True(_service.IsDirty);
    }

    [Fact]
    public void ClearDirty_ClearsDirtyFlag()
    {
        _service.MarkDirty();
        _service.ClearDirty();

        Assert.False(_service.IsDirty);
    }

    [Fact]
    public void CreateNew_CreatesMotorWithDefaultDriveAndSeries()
    {
        var motor = _service.CreateNew("New Motor", 5000, 50, 1500);

        Assert.Equal("New Motor", motor.MotorName);
        Assert.Single(motor.Drives);
        Assert.Single(motor.Drives[0].Voltages);
        Assert.Equal(2, motor.Drives[0].Voltages[0].Series.Count);
        Assert.NotNull(motor.Drives[0].Voltages[0].GetSeriesByName("Peak"));
        Assert.NotNull(motor.Drives[0].Voltages[0].GetSeriesByName("Continuous"));
    }

    [Fact]
    public void CreateNew_SetsDirtyFlag()
    {
        _service.CreateNew("New Motor", 5000, 50, 1500);

        Assert.True(_service.IsDirty);
    }

    [Fact]
    public void CreateNew_ClearsCurrentFilePath()
    {
        _service.CreateNew("New Motor", 5000, 50, 1500);

        Assert.Null(_service.CurrentFilePath);
    }

    [Fact]
    public void CreateNew_SeriesHave101Points()
    {
        var motor = _service.CreateNew("New Motor", 5000, 50, 1500);

        var allSeries = GetAllSeries(motor);
        Assert.All(allSeries, s => Assert.Equal(101, s.Data.Count));
    }

    [Fact]
    public void Reset_ClearsState()
    {
        _service.MarkDirty();

        _service.Reset();

        Assert.Null(_service.CurrentFilePath);
        Assert.False(_service.IsDirty);
    }
}
