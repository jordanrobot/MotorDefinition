using System.Collections.Generic;
using JordanRobot.MotorDefinitions.Model;

namespace CurveEditor.Services;

/// <summary>
/// Service interface for generating motor torque curves.
/// </summary>
public interface ICurveGeneratorService
{
    /// <summary>
    /// Generates a new curve series from motor parameters.
    /// </summary>
    /// <param name="name">The name for the curve series.</param>
    /// <param name="maxRpm">The maximum RPM of the motor.</param>
    /// <param name="maxTorque">The maximum torque of the motor.</param>
    /// <param name="maxPower">The maximum power of the motor.</param>
    /// <returns>A new curve series with interpolated data.</returns>
    CurveSeries GenerateCurve(string name, double maxRpm, double maxTorque, double maxPower);

    /// <summary>
    /// Interpolates curve data at 1% increments from motor parameters.
    /// Uses a constant torque / constant power model.
    /// </summary>
    /// <param name="maxRpm">The maximum RPM of the motor.</param>
    /// <param name="maxTorque">The maximum torque of the motor.</param>
    /// <param name="maxPower">The maximum power of the motor (Watts).</param>
    /// <returns>A list of 101 data points (0% to 100% in 1% increments).</returns>
    List<DataPoint> InterpolateCurve(double maxRpm, double maxTorque, double maxPower);

    /// <summary>
    /// Calculates power from torque and speed.
    /// Power (W) = Torque (Nm) × Angular velocity (rad/s)
    /// </summary>
    /// <param name="torqueNm">Torque in Newton-meters.</param>
    /// <param name="rpm">Rotational speed in RPM.</param>
    /// <returns>Power in Watts.</returns>
    double CalculatePower(double torqueNm, double rpm);

    /// <summary>
    /// Calculates the corner speed where the motor transitions from constant torque to constant power.
    /// </summary>
    /// <param name="maxTorque">Maximum torque in Nm.</param>
    /// <param name="maxPower">Maximum power in Watts.</param>
    /// <returns>Corner speed in RPM.</returns>
    double CalculateCornerSpeed(double maxTorque, double maxPower);
}
