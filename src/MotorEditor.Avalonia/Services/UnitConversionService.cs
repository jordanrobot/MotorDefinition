using System;
using JordanRobot.MotorDefinition.Model;
using JordanRobot.MotorDefinition.Services;

namespace CurveEditor.Services;

/// <summary>
/// Provides unit conversion services for the UI layer.
/// Supports "Convert on Display" and "Convert Stored Data" modes.
/// </summary>
public class UnitConversionService
{
    private readonly UnitService _unitService;
    private readonly IUserSettingsStore? _settingsStore;

    /// <summary>
    /// Gets or sets whether to convert stored data or only display values.
    /// </summary>
    /// <remarks>
    /// When true, conversions update the stored data in the model.
    /// When false (default), conversions only affect displayed values.
    /// </remarks>
    public bool ConvertStoredData { get; set; }

    /// <summary>
    /// Gets or sets the number of decimal places for display formatting.
    /// </summary>
    public int DisplayDecimalPlaces { get; set; } = 2;

    /// <summary>
    /// Initializes a new instance of the UnitConversionService class.
    /// </summary>
    /// <param name="settingsStore">Optional user settings store for persisting preferences.</param>
    public UnitConversionService(IUserSettingsStore? settingsStore = null)
    {
        _unitService = new UnitService();
        _settingsStore = settingsStore;
    }

    /// <summary>
    /// Converts a torque value for display or storage.
    /// </summary>
    /// <param name="value">The torque value to convert.</param>
    /// <param name="fromUnit">The source unit.</param>
    /// <param name="toUnit">The target unit.</param>
    /// <returns>The converted torque value.</returns>
    public decimal ConvertTorque(decimal value, string fromUnit, string toUnit)
    {
        return _unitService.Convert(value, fromUnit, toUnit);
    }

    /// <summary>
    /// Converts speed (RPM) value for display or storage.
    /// </summary>
    /// <param name="value">The speed value to convert.</param>
    /// <param name="fromUnit">The source unit.</param>
    /// <param name="toUnit">The target unit.</param>
    /// <returns>The converted speed value.</returns>
    public decimal ConvertSpeed(decimal value, string fromUnit, string toUnit)
    {
        return _unitService.Convert(value, fromUnit, toUnit);
    }

    /// <summary>
    /// Converts power value for display or storage.
    /// </summary>
    /// <param name="value">The power value to convert.</param>
    /// <param name="fromUnit">The source unit.</param>
    /// <param name="toUnit">The target unit.</param>
    /// <returns>The converted power value.</returns>
    public decimal ConvertPower(decimal value, string fromUnit, string toUnit)
    {
        return _unitService.Convert(value, fromUnit, toUnit);
    }

    /// <summary>
    /// Converts mass (weight) value for display or storage.
    /// </summary>
    /// <param name="value">The mass value to convert.</param>
    /// <param name="fromUnit">The source unit.</param>
    /// <param name="toUnit">The target unit.</param>
    /// <returns>The converted mass value.</returns>
    public decimal ConvertMass(decimal value, string fromUnit, string toUnit)
    {
        return _unitService.Convert(value, fromUnit, toUnit);
    }

    /// <summary>
    /// Formats a value with unit for display.
    /// </summary>
    /// <param name="value">The numeric value.</param>
    /// <param name="unit">The unit string.</param>
    /// <returns>Formatted string with value and unit.</returns>
    public string FormatValue(decimal value, string unit)
    {
        return _unitService.Format(value, unit, DisplayDecimalPlaces);
    }

    /// <summary>
    /// Converts all torque values in a curve from one unit to another.
    /// </summary>
    /// <param name="curve">The curve to convert.</param>
    /// <param name="fromUnit">The source unit.</param>
    /// <param name="toUnit">The target unit.</param>
    /// <remarks>
    /// This method modifies the curve data in place if ConvertStoredData is true.
    /// </remarks>
    public void ConvertCurveTorque(Curve curve, string fromUnit, string toUnit)
    {
        ArgumentNullException.ThrowIfNull(curve);

        if (fromUnit == toUnit || !ConvertStoredData)
        {
            return;
        }

        foreach (var point in curve.Data)
        {
            point.Torque = ConvertTorque(point.Torque, fromUnit, toUnit);
        }
    }

    /// <summary>
    /// Converts all speed values in a curve from one unit to another.
    /// </summary>
    /// <param name="curve">The curve to convert.</param>
    /// <param name="fromUnit">The source unit.</param>
    /// <param name="toUnit">The target unit.</param>
    /// <remarks>
    /// This method modifies the curve data in place if ConvertStoredData is true.
    /// </remarks>
    public void ConvertCurveSpeed(Curve curve, string fromUnit, string toUnit)
    {
        ArgumentNullException.ThrowIfNull(curve);

        if (fromUnit == toUnit || !ConvertStoredData)
        {
            return;
        }

        foreach (var point in curve.Data)
        {
            point.Rpm = ConvertSpeed(point.Rpm, fromUnit, toUnit);
        }
    }

    /// <summary>
    /// Converts all relevant units in a motor definition.
    /// </summary>
    /// <param name="motor">The motor to convert.</param>
    /// <param name="oldUnits">The old unit settings.</param>
    /// <param name="newUnits">The new unit settings.</param>
    /// <remarks>
    /// This method modifies the motor data in place if ConvertStoredData is true.
    /// Only converts values if ConvertStoredData is true and units differ.
    /// </remarks>
    public void ConvertMotorUnits(ServoMotor motor, UnitSettings oldUnits, UnitSettings newUnits)
    {
        ArgumentNullException.ThrowIfNull(motor);
        ArgumentNullException.ThrowIfNull(oldUnits);
        ArgumentNullException.ThrowIfNull(newUnits);

        if (!ConvertStoredData)
        {
            return;
        }

        // Convert motor-level properties
        if (oldUnits.Torque != newUnits.Torque)
        {
            motor.RatedContinuousTorque = ConvertTorque(motor.RatedContinuousTorque, oldUnits.Torque, newUnits.Torque);
            motor.RatedPeakTorque = ConvertTorque(motor.RatedPeakTorque, oldUnits.Torque, newUnits.Torque);
            motor.BrakeTorque = ConvertTorque(motor.BrakeTorque, oldUnits.Torque, newUnits.Torque);
        }

        if (oldUnits.Speed != newUnits.Speed)
        {
            motor.MaxSpeed = ConvertSpeed(motor.MaxSpeed, oldUnits.Speed, newUnits.Speed);
            motor.RatedSpeed = ConvertSpeed(motor.RatedSpeed, oldUnits.Speed, newUnits.Speed);
        }

        if (oldUnits.Power != newUnits.Power)
        {
            motor.Power = ConvertPower(motor.Power, oldUnits.Power, newUnits.Power);
        }

        if (oldUnits.Weight != newUnits.Weight)
        {
            motor.Weight = ConvertMass(motor.Weight, oldUnits.Weight, newUnits.Weight);
        }

        if (oldUnits.Inertia != newUnits.Inertia)
        {
            motor.RotorInertia = _unitService.Convert(motor.RotorInertia, oldUnits.Inertia, newUnits.Inertia);
        }

        if (oldUnits.Current != newUnits.Current)
        {
            motor.BrakeAmperage = _unitService.Convert(motor.BrakeAmperage, oldUnits.Current, newUnits.Current);
        }

        if (oldUnits.ResponseTime != newUnits.ResponseTime)
        {
            motor.BrakeReleaseTime = _unitService.Convert(motor.BrakeReleaseTime, oldUnits.ResponseTime, newUnits.ResponseTime);
            motor.BrakeEngageTimeDiode = _unitService.Convert(motor.BrakeEngageTimeDiode, oldUnits.ResponseTime, newUnits.ResponseTime);
            motor.BrakeEngageTimeMov = _unitService.Convert(motor.BrakeEngageTimeMov, oldUnits.ResponseTime, newUnits.ResponseTime);
        }

        if (oldUnits.Backlash != newUnits.Backlash)
        {
            motor.BrakeBacklash = _unitService.Convert(motor.BrakeBacklash, oldUnits.Backlash, newUnits.Backlash);
        }

        // Convert drive/voltage level properties
        foreach (var drive in motor.Drives)
        {
            foreach (var voltage in drive.Voltages)
            {
                if (oldUnits.Torque != newUnits.Torque)
                {
                    voltage.RatedContinuousTorque = ConvertTorque(voltage.RatedContinuousTorque, oldUnits.Torque, newUnits.Torque);
                    voltage.RatedPeakTorque = ConvertTorque(voltage.RatedPeakTorque, oldUnits.Torque, newUnits.Torque);
                }

                if (oldUnits.Speed != newUnits.Speed)
                {
                    voltage.MaxSpeed = ConvertSpeed(voltage.MaxSpeed, oldUnits.Speed, newUnits.Speed);
                    voltage.RatedSpeed = ConvertSpeed(voltage.RatedSpeed, oldUnits.Speed, newUnits.Speed);
                }

                if (oldUnits.Power != newUnits.Power)
                {
                    voltage.Power = ConvertPower(voltage.Power, oldUnits.Power, newUnits.Power);
                }

                if (oldUnits.Current != newUnits.Current)
                {
                    voltage.ContinuousAmperage = _unitService.Convert(voltage.ContinuousAmperage, oldUnits.Current, newUnits.Current);
                    voltage.PeakAmperage = _unitService.Convert(voltage.PeakAmperage, oldUnits.Current, newUnits.Current);
                }

                // Convert curve data points
                foreach (var curve in voltage.Curves)
                {
                    if (oldUnits.Torque != newUnits.Torque)
                    {
                        ConvertCurveTorque(curve, oldUnits.Torque, newUnits.Torque);
                    }

                    if (oldUnits.Speed != newUnits.Speed)
                    {
                        ConvertCurveSpeed(curve, oldUnits.Speed, newUnits.Speed);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets the display value for a torque, converting if needed.
    /// </summary>
    /// <param name="storedValue">The value as stored in the model.</param>
    /// <param name="storedUnit">The unit as stored in the model.</param>
    /// <param name="displayUnit">The unit to display.</param>
    /// <returns>The value in the display unit.</returns>
    public double GetDisplayTorque(double storedValue, string storedUnit, string displayUnit)
    {
        if (ConvertStoredData || storedUnit == displayUnit)
        {
            return storedValue;
        }

        return ConvertTorque(storedValue, storedUnit, displayUnit);
    }

    /// <summary>
    /// Gets the stored value from a display value, converting if needed.
    /// </summary>
    /// <param name="displayValue">The value being displayed.</param>
    /// <param name="displayUnit">The display unit.</param>
    /// <param name="storedUnit">The storage unit.</param>
    /// <returns>The value in the storage unit.</returns>
    public double GetStoredTorque(double displayValue, string displayUnit, string storedUnit)
    {
        if (ConvertStoredData || displayUnit == storedUnit)
        {
            return displayValue;
        }

        return ConvertTorque(displayValue, displayUnit, storedUnit);
    }

    /// <summary>
    /// Validates whether a unit string is supported.
    /// </summary>
    /// <param name="unit">The unit string to validate.</param>
    /// <returns>True if supported; otherwise, false.</returns>
    public bool IsUnitSupported(string unit)
    {
        return _unitService.IsUnitSupported(unit);
    }
}
