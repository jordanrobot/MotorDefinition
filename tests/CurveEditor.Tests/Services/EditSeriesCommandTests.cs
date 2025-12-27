using CurveEditor.Services;
using JordanRobot.MotorDefinition.Model;
using System.Collections.Generic;
using Xunit;

namespace CurveEditor.Tests.Services;

public class EditSeriesCommandTests
{
    [Fact]
    public void Execute_UpdatesNameAndLocked()
    {
        var series = new Curve
        {
            Name = "Old",
            Locked = false,
            Data = new List<DataPoint>()
        };

        var command = new EditSeriesCommand(series, "New", true);

        command.Execute();

        Assert.Equal("New", series.Name);
        Assert.True(series.Locked);
    }

    [Fact]
    public void Undo_RestoresPreviousNameAndLocked()
    {
        var series = new Curve
        {
            Name = "Old",
            Locked = false,
            Data = new List<DataPoint>()
        };

        var command = new EditSeriesCommand(series, "New", true);

        command.Execute();
        command.Undo();

        Assert.Equal("Old", series.Name);
        Assert.False(series.Locked);
    }
}
