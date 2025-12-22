using System;
using CurveEditor.Models;

namespace jordanrobot.MotorDefinitions;

/// <summary>
/// Provides entrypoints for loading and saving motor definition files.
/// </summary>
public static class MotorFile
{
    /// <summary>
    /// Loads a motor definition from the specified path.
    /// </summary>
    /// <param name="path">The file path to read.</param>
    /// <returns>The parsed motor definition.</returns>
    /// <exception cref="NotImplementedException">Always thrown until implemented in later steps.</exception>
    public static MotorDefinition Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        throw new NotImplementedException("Motor file loading will be implemented in a later phase.");
    }

    /// <summary>
    /// Saves a motor definition to the specified path.
    /// </summary>
    /// <param name="motor">The motor definition to persist.</param>
    /// <param name="path">The destination file path.</param>
    /// <exception cref="NotImplementedException">Always thrown until implemented in a later phase.</exception>
    public static void Save(MotorDefinition motor, string path)
    {
        ArgumentNullException.ThrowIfNull(motor);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        throw new NotImplementedException("Motor file saving will be implemented in a later phase.");
    }
}
