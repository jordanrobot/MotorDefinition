using JordanRobot.MotorDefinitions.Model;
using CurveEditor.Services;
using Xunit;

namespace CurveEditor.Tests.Services;

public class EditMotorPropertyCommandTests
{
    [Fact]
    public void Execute_UpdatesMotorProperty()
    {
        var motor = new MotorDefinition
        {
            MaxSpeed = 3000
        };

        var command = new EditMotorPropertyCommand(motor, nameof(MotorDefinition.MaxSpeed), 3000d, 3500d);

        command.Execute();

        Assert.Equal(3500, motor.MaxSpeed);
    }

    [Fact]
    public void Undo_RestoresPreviousMotorPropertyValue()
    {
        var motor = new MotorDefinition
        {
            MaxSpeed = 3000
        };

        var command = new EditMotorPropertyCommand(motor, nameof(MotorDefinition.MaxSpeed), 3000d, 3500d);

        command.Execute();
        command.Undo();

        Assert.Equal(3000, motor.MaxSpeed);
    }
}
