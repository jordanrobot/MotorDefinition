using System.Collections.Generic;
using JordanRobot.MotorDefinitions.Model;
using CurveEditor.Services;
using Xunit;

namespace CurveEditor.Tests.Services;

public class EditPointCommandTests
{
    [Fact]
    public void Execute_ChangesPointValues()
    {
        var series = new CurveSeries
        {
            Name = "Test",
            Data = new List<DataPoint>
            {
                new() { Rpm = 1000, Torque = 1.0 },
                new() { Rpm = 2000, Torque = 2.0 }
            }
        };

        var command = new EditPointCommand(series, 1, 2500, 2.5);

        command.Execute();

        Assert.Equal(2500, series.Data[1].Rpm);
        Assert.Equal(2.5, series.Data[1].Torque);
    }

    [Fact]
    public void Undo_RestoresPreviousPointValues()
    {
        var series = new CurveSeries
        {
            Name = "Test",
            Data = new List<DataPoint>
            {
                new() { Rpm = 1000, Torque = 1.0 },
                new() { Rpm = 2000, Torque = 2.0 }
            }
        };

        var command = new EditPointCommand(series, 1, 2500, 2.5);

        command.Execute();
        command.Undo();

        Assert.Equal(2000, series.Data[1].Rpm);
        Assert.Equal(2.0, series.Data[1].Torque);
    }
}
