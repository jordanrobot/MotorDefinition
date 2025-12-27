using CurveEditor.Services;
using CurveEditor.ViewModels;
using JordanRobot.MotorDefinition.Model;
using Moq;
using System.Threading.Tasks;

namespace CurveEditor.Tests.ViewModels;

public class MainWindowViewModelDirtyPromptTests
{
    [Fact]
    public void NewMotor_SetsIsDirtyWhenFileServiceMarksDirty()
    {
        var fileServiceMock = new Mock<IFileService>();
        var curveGeneratorMock = new Mock<ICurveGeneratorService>();

        var motor = new ServoMotor { MotorName = "New Motor" };
        fileServiceMock
            .Setup(f => f.CreateNew(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>(), It.IsAny<double>()))
            .Returns(motor);
        fileServiceMock
            .SetupGet(f => f.IsDirty)
            .Returns(true);

        var vm = new MainWindowViewModel(fileServiceMock.Object, curveGeneratorMock.Object);

        var command = vm.NewMotorCommand;
        Assert.NotNull(command);

        command.Execute(null);

        Assert.True(vm.IsDirty);
    }

    [Fact]
    public void SaveCommand_DoesNotExecuteWhenCannotSave()
    {
        var fileServiceMock = new Mock<IFileService>();
        var curveGeneratorMock = new Mock<ICurveGeneratorService>();

        var vm = new MainWindowViewModel(fileServiceMock.Object, curveGeneratorMock.Object);

        var command = vm.SaveCommand;
        Assert.NotNull(command);

        var canExecute = command.CanExecute(null);

        Assert.False(canExecute);
    }
}
