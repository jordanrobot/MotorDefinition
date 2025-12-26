using JordanRobot.MotorDefinitions.Model;
using System.Threading.Tasks;

namespace CurveEditor.Services;

/// <summary>
/// Service interface for file operations on motor definition files.
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Gets the path of the currently loaded file.
    /// </summary>
    string? CurrentFilePath { get; }

    /// <summary>
    /// Gets whether the current file has unsaved changes.
    /// </summary>
    bool IsDirty { get; }

    /// <summary>
    /// Loads a motor definition from a JSON file.
    /// </summary>
    /// <param name="filePath">The path to the JSON file.</param>
    /// <returns>The loaded motor definition.</returns>
    Task<MotorDefinition> LoadAsync(string filePath);

    /// <summary>
    /// Saves a motor definition to the current file path.
    /// </summary>
    /// <param name="motorDefinition">The motor definition to save.</param>
    Task SaveAsync(MotorDefinition motorDefinition);

    /// <summary>
    /// Saves a motor definition to a new file path and makes it the current file.
    /// </summary>
    /// <param name="motorDefinition">The motor definition to save.</param>
    /// <param name="filePath">The path to save to.</param>
    Task SaveAsAsync(MotorDefinition motorDefinition, string filePath);

    /// <summary>
    /// Saves a copy of the motor definition to a new file without changing the current file.
    /// </summary>
    /// <param name="motorDefinition">The motor definition to save.</param>
    /// <param name="filePath">The path to save the copy to.</param>
    Task SaveCopyAsAsync(MotorDefinition motorDefinition, string filePath);

    /// <summary>
    /// Marks the current file as having unsaved changes.
    /// </summary>
    void MarkDirty();

    /// <summary>
    /// Clears the dirty flag (called after saving).
    /// </summary>
    void ClearDirty();

    /// <summary>
    /// Creates a new motor definition and sets it as the current file (unsaved).
    /// </summary>
    /// <param name="motorName">The name for the new motor.</param>
    /// <param name="maxRpm">The maximum RPM.</param>
    /// <param name="maxTorque">The maximum torque.</param>
    /// <param name="maxPower">The maximum power.</param>
    /// <returns>The new motor definition.</returns>
    MotorDefinition CreateNew(string motorName, double maxRpm, double maxTorque, double maxPower);

    /// <summary>
    /// Resets the service state (clears current file path and dirty flag).
    /// </summary>
    void Reset();
}
