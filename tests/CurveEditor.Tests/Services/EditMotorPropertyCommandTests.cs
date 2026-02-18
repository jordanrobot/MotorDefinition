using CurveEditor.Services;
using JordanRobot.MotorDefinition.Model;
using Xunit;

namespace CurveEditor.Tests.Services;

public class EditMotorPropertyCommandTests
{
    [Fact]
    public void Execute_UpdatesMotorProperty()
    {
        var motor = new ServoMotor
        {
            MaxSpeed = 3000
        };

        var command = new EditMotorPropertyCommand(motor, nameof(ServoMotor.MaxSpeed), 3000m, 3500m);

        command.Execute();

        Assert.Equal(3500m, motor.MaxSpeed);
    }

    [Fact]
    public void Undo_RestoresPreviousMotorPropertyValue()
    {
        var motor = new ServoMotor
        {
            MaxSpeed = 3000
        };

        var command = new EditMotorPropertyCommand(motor, nameof(ServoMotor.MaxSpeed), 3000m, 3500m);

        command.Execute();
        command.Undo();

        Assert.Equal(3000m, motor.MaxSpeed);
    }
}
