using CurveEditor.Services;
using JordanRobot.MotorDefinition.Model;
using System.Collections.Generic;
using Xunit;

namespace CurveEditor.Tests.Services;

public class EditPointCommandTests
{
    [Fact]
    public void Execute_ChangesPointValues()
    {
        var series = new Curve
        {
            Name = "Test",
            Data = new List<DataPoint>
            {
                new() { Rpm = 1000, Torque = 1.0m },
                new() { Rpm = 2000, Torque = 2.0m }
            }
        };

        var command = new EditPointCommand(series, 1, 2500m, 2.5m);

        command.Execute();

        Assert.Equal(2500m, series.Data[1].Rpm);
        Assert.Equal(2.5m, series.Data[1].Torque);
    }

    [Fact]
    public void Undo_RestoresPreviousPointValues()
    {
        var series = new Curve
        {
            Name = "Test",
            Data = new List<DataPoint>
            {
                new() { Rpm = 1000, Torque = 1.0m },
                new() { Rpm = 2000, Torque = 2.0m }
            }
        };

        var command = new EditPointCommand(series, 1, 2500m, 2.5m);

        command.Execute();
        command.Undo();

        Assert.Equal(2000m, series.Data[1].Rpm);
        Assert.Equal(2.0m, series.Data[1].Torque);
    }
}
