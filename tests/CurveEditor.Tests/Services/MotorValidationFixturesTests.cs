using CurveEditor.Services;
using JordanRobot.MotorDefinition;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace CurveEditor.Tests.Services;

public class MotorValidationFixturesTests
{
    [Fact]
    public void Validate_InvalidMotor_EmptyName_ProducesMotorNameError()
    {
        var filePath = FindRepoFilePath("tests", "TestMotorFiles", "Invalid", "invalid-motor-empty-name.json");

        Assert.True(File.Exists(filePath), $"Test file not found: {filePath}");

        var motor = MotorFile.Load(filePath);
        var validationService = new ValidationService();

        var errors = validationService.ValidateServoMotor(motor);

        Assert.Contains(errors, e => e.Contains("Motor name cannot be empty.", StringComparison.Ordinal));
    }

    [Fact]
    public void Validate_InvalidMotor_NegativeScalars_ProducesMotorScalarErrors()
    {
        var filePath = FindRepoFilePath("tests", "TestMotorFiles", "Invalid", "invalid-motor-negative-values.json");

        Assert.True(File.Exists(filePath), $"Test file not found: {filePath}");

        var motor = MotorFile.Load(filePath);
        var validationService = new ValidationService();

        var errors = validationService.ValidateServoMotor(motor);

        Assert.Contains(errors, e => e.Contains("Max speed cannot be negative", StringComparison.Ordinal));
        Assert.Contains(errors, e => e.Contains("Power cannot be negative", StringComparison.Ordinal));
        Assert.Contains(errors, e => e.Contains("Peak torque cannot be negative", StringComparison.Ordinal));
        Assert.Contains(errors, e => e.Contains("Continuous torque cannot be negative", StringComparison.Ordinal));
        Assert.Contains(errors, e => e.Contains("Brake release time cannot be negative", StringComparison.Ordinal));
        Assert.Contains(errors, e => e.Contains("Brake backlash cannot be negative", StringComparison.Ordinal));
    }

    [Fact]
    public void Validate_InvalidMotor_VoltageNegativeScalars_ProducesVoltageScalarErrors()
    {
        var filePath = FindRepoFilePath("tests", "TestMotorFiles", "Invalid", "invalid-voltage-negative-values.json");

        Assert.True(File.Exists(filePath), $"Test file not found: {filePath}");

        var motor = MotorFile.Load(filePath);
        var validationService = new ValidationService();

        var errors = validationService.ValidateServoMotor(motor);

        Assert.Contains(errors, e => e.Contains("48V: Max speed cannot be negative", StringComparison.Ordinal));
        Assert.Contains(errors, e => e.Contains("48V: Power cannot be negative", StringComparison.Ordinal));
        Assert.Contains(errors, e => e.Contains("48V: Peak torque cannot be negative", StringComparison.Ordinal));
        Assert.Contains(errors, e => e.Contains("48V: Continuous torque cannot be negative", StringComparison.Ordinal));
    }

    [Fact]
    public void Validate_InvalidMotor_ContinuousTorqueExceedsPeak_ProducesRelationError()
    {
        var filePath = FindRepoFilePath("tests", "TestMotorFiles", "Invalid", "invalid-voltage-continuous-over-peak.json");

        Assert.True(File.Exists(filePath), $"Test file not found: {filePath}");

        var motor = MotorFile.Load(filePath);
        var validationService = new ValidationService();

        var errors = validationService.ValidateServoMotor(motor);

        Assert.Contains(errors, e => e.Contains("Continuous torque (12)", StringComparison.Ordinal));
        Assert.Contains(errors, e => e.Contains("cannot exceed peak torque (8)", StringComparison.Ordinal));
    }

    [Fact]
    public void Validate_InvalidMotor_MultipleErrors_ProducesMultipleMessages()
    {
        var filePath = FindRepoFilePath("tests", "TestMotorFiles", "Invalid", "invalid-mixed-multiple-errors.json");

        Assert.True(File.Exists(filePath), $"Test file not found: {filePath}");

        var motor = MotorFile.Load(filePath);
        var validationService = new ValidationService();

        var errors = validationService.ValidateServoMotor(motor);

        Assert.True(errors.Count >= 4, $"Expected multiple validation errors, got {errors.Count}.\n{string.Join("\n", errors)}");
    }

    private static string FindRepoFilePath(params string[] relativePathSegments)
    {
        // Test runners vary in their working directory; resolve by walking upwards until we find the repo root.
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        for (var i = 0; i < 10 && current is not null; i++)
        {
            var candidate = Path.Combine(current.FullName, Path.Combine(relativePathSegments));
            if (File.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, Path.Combine(relativePathSegments)));
    }
}
