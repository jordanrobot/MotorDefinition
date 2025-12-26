using System.Collections.Generic;
using System.Collections.ObjectModel;
using JordanRobot.MotorDefinitions.Model;
using CurveEditor.Services;
using CurveEditor.ViewModels;
using Moq;
using Xunit;

namespace CurveEditor.Tests.ViewModels;

public class MainWindowViewModelTests
{
    private static (MainWindowViewModel vm, MotorDefinition motor) CreateViewModelWithMotor()
    {
        var fileServiceMock = new Mock<IFileService>();
        var curveGeneratorMock = new Mock<ICurveGeneratorService>();

        var motor = new MotorDefinition
        {
            MotorName = "Test Motor",
            MaxSpeed = 5000,
            Units = new UnitSettings { Torque = "Nm" },
            Drives = new List<DriveConfiguration>
            {
                new()
                {
                    Name = "Drive A",
                    Voltages = new List<VoltageConfiguration>
                    {
                        new()
                        {
                            Voltage = 208,
                            MaxSpeed = 4000,
                            Series = new List<CurveSeries>()
                        },
                        new()
                        {
                            Voltage = 400,
                            MaxSpeed = 4500,
                            Series = new List<CurveSeries>()
                        }
                    }
                },
                new()
                {
                    Name = "Drive B",
                    Voltages = new List<VoltageConfiguration>
                    {
                        new()
                        {
                            Voltage = 120,
                            MaxSpeed = 3500,
                            Series = new List<CurveSeries>()
                        }
                    }
                }
            }
        };

        var vm = new MainWindowViewModel(fileServiceMock.Object, curveGeneratorMock.Object)
        {
            CurrentMotor = motor
        };

        return (vm, motor);
    }

    [Fact]
    public void SettingCurrentMotor_SelectsFirstDriveByDefault()
    {
        var (vm, motor) = CreateViewModelWithMotor();

        Assert.Same(motor.Drives[0], vm.SelectedDrive);
    }

    [Fact]
    public void ChangingSelectedDrive_RefreshesAvailableVoltagesAndSelectsPreferred208V()
    {
        var (vm, motor) = CreateViewModelWithMotor();

        vm.SelectedDrive = motor.Drives[0];

        Assert.Equal(2, vm.AvailableVoltages.Count);
        Assert.Equal(208, vm.SelectedVoltage?.Voltage);
    }

    [Fact]
    public void ChangingSelectedDrive_UsesFirstVoltageWhen208NotAvailable()
    {
        var (vm, motor) = CreateViewModelWithMotor();

        vm.SelectedDrive = motor.Drives[1];

        Assert.Single(vm.AvailableVoltages);
        Assert.Same(motor.Drives[1].Voltages[0], vm.SelectedVoltage);
    }

    [Fact]
    public void ChangingSelectedVoltage_RefreshesAvailableSeriesAndUpdatesChartAndTable()
    {
        var (vm, motor) = CreateViewModelWithMotor();
        var drive = motor.Drives[0];
        var voltage = drive.Voltages[0];

        // Mirror real lifecycle: select drive/voltage first so
        // OnSelectedVoltageChanged runs, then mutate series and
        // explicitly refresh the AvailableSeries collection.
        vm.SelectedDrive = drive;
        vm.SelectedVoltage = voltage;

        voltage.Series.Add(new CurveSeries { Name = "Peak" });
        voltage.Series.Add(new CurveSeries { Name = "Continuous" });
        vm.RefreshAvailableSeriesPublic();

        Assert.Equal(2, vm.AvailableSeries.Count);
        Assert.Equal(voltage, vm.ChartViewModel.CurrentVoltage);
        Assert.Equal(voltage, vm.CurveDataTableViewModel.CurrentVoltage);
        Assert.True(vm.SelectedSeries == null || vm.AvailableSeries.Contains(vm.SelectedSeries));
        Assert.Equal(motor.MaxSpeed, vm.ChartViewModel.MotorMaxSpeed);
    }

    [Fact]
    public void OnSelectedDriveChanged_ClearsSelectedVoltageWhenDriveIsNull()
    {
        var (vm, _) = CreateViewModelWithMotor();

        vm.SelectedDrive = null;

        Assert.Null(vm.SelectedVoltage);
        Assert.Empty(vm.AvailableVoltages);
    }

}
