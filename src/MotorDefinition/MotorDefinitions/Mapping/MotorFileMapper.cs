using CurveEditor.Models;
using JordanRobot.MotorDefinitions.Dtos;
using JordanRobot.MotorDefinitions.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JordanRobot.MotorDefinitions.Mapping;

/// <summary>
/// Converts between persisted motor definition DTOs and runtime models.
/// </summary>
internal static class MotorFileMapper
{
    /// <summary>
    /// Maps a runtime <see cref="MotorDefinition"/> into a persistence DTO.
    /// </summary>
    /// <param name="motor">The runtime motor definition.</param>
    /// <returns>A DTO ready for serialization.</returns>
    public static MotorDefinitionFileDto ToFileDto(MotorDefinition motor)
    {
        ArgumentNullException.ThrowIfNull(motor);

        var dto = new MotorDefinitionFileDto
        {
            SchemaVersion = MotorDefinition.CurrentSchemaVersion,
            MotorName = motor.MotorName,
            Manufacturer = motor.Manufacturer,
            PartNumber = motor.PartNumber,
            Power = motor.Power,
            MaxSpeed = motor.MaxSpeed,
            RatedSpeed = motor.RatedSpeed,
            RatedContinuousTorque = motor.RatedContinuousTorque,
            RatedPeakTorque = motor.RatedPeakTorque,
            Weight = motor.Weight,
            RotorInertia = motor.RotorInertia,
            FeedbackPpr = motor.FeedbackPpr,
            HasBrake = motor.HasBrake,
            BrakeTorque = motor.BrakeTorque,
            BrakeAmperage = motor.BrakeAmperage,
            BrakeVoltage = motor.BrakeVoltage,
            BrakeReleaseTime = motor.BrakeReleaseTime,
            BrakeEngageTimeDiode = motor.BrakeEngageTimeDiode,
            BrakeEngageTimeMov = motor.BrakeEngageTimeMov,
            BrakeBacklash = motor.BrakeBacklash,
            Units = MapUnits(motor.Units),
            Metadata = MapMetadata(motor.Metadata)
        };

        foreach (var drive in motor.Drives)
        {
            var driveDto = new DriveFileDto
            {
                Manufacturer = drive.Manufacturer,
                PartNumber = drive.PartNumber,
                Name = drive.Name
            };

            foreach (var voltage in drive.Voltages)
            {
                MotorFileShapeValidator.ValidateRuntimeVoltage(voltage);

                var axisSource = voltage.Series[0];
                var pointCount = axisSource.Data.Count;
                var percentAxis = axisSource.Data.Select(p => p.Percent).ToArray();
                var rpmAxis = axisSource.Data.Select(p => p.Rpm).ToArray();

                var seriesMap = new Dictionary<string, SeriesEntryDto>(StringComparer.Ordinal);
                foreach (var series in voltage.Series)
                {
                    if (seriesMap.ContainsKey(series.Name))
                    {
                        throw new InvalidOperationException($"Duplicate series name '{series.Name}' found for {voltage.Voltage}V in drive '{drive.Name}'.");
                    }

                    var torque = new double[pointCount];
                    for (var i = 0; i < pointCount; i++)
                    {
                        torque[i] = series.Data[i].Torque;
                    }

                    seriesMap.Add(series.Name, new SeriesEntryDto
                    {
                        Locked = series.Locked,
                        Notes = string.IsNullOrWhiteSpace(series.Notes) ? null : series.Notes,
                        Torque = torque
                    });
                }

                var voltageDto = new VoltageFileDto
                {
                    Voltage = voltage.Voltage,
                    Power = voltage.Power,
                    MaxSpeed = voltage.MaxSpeed,
                    RatedSpeed = voltage.RatedSpeed,
                    RatedContinuousTorque = voltage.RatedContinuousTorque,
                    RatedPeakTorque = voltage.RatedPeakTorque,
                    ContinuousAmperage = voltage.ContinuousAmperage,
                    PeakAmperage = voltage.PeakAmperage,
                    Percent = percentAxis,
                    Rpm = rpmAxis,
                    Series = seriesMap
                };

                driveDto.Voltages.Add(voltageDto);
            }

            dto.Drives.Add(driveDto);
        }

        return dto;
    }

    /// <summary>
    /// Maps a persistence DTO into a runtime <see cref="MotorDefinition"/>.
    /// </summary>
    /// <param name="dto">The deserialized DTO.</param>
    /// <returns>A runtime motor definition model.</returns>
    public static MotorDefinition ToRuntimeModel(MotorDefinitionFileDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var motor = new MotorDefinition(dto.MotorName)
        {
            SchemaVersion = string.IsNullOrWhiteSpace(dto.SchemaVersion) ? MotorDefinition.CurrentSchemaVersion : dto.SchemaVersion,
            Manufacturer = dto.Manufacturer,
            PartNumber = dto.PartNumber,
            Power = dto.Power,
            MaxSpeed = dto.MaxSpeed,
            RatedSpeed = dto.RatedSpeed,
            RatedContinuousTorque = dto.RatedContinuousTorque,
            RatedPeakTorque = dto.RatedPeakTorque,
            Weight = dto.Weight,
            RotorInertia = dto.RotorInertia,
            FeedbackPpr = dto.FeedbackPpr,
            HasBrake = dto.HasBrake,
            BrakeTorque = dto.BrakeTorque,
            BrakeAmperage = dto.BrakeAmperage,
            BrakeVoltage = dto.BrakeVoltage,
            BrakeReleaseTime = dto.BrakeReleaseTime,
            BrakeEngageTimeDiode = dto.BrakeEngageTimeDiode,
            BrakeEngageTimeMov = dto.BrakeEngageTimeMov,
            BrakeBacklash = dto.BrakeBacklash,
            Units = MapUnits(dto.Units ?? new UnitSettingsDto()),
            Metadata = MapMetadata(dto.Metadata)
        };

        if (dto.Drives is null || dto.Drives.Count == 0)
        {
            throw new InvalidOperationException("Motor definition must include at least one drive.");
        }

        foreach (var driveDto in dto.Drives)
        {
            if (string.IsNullOrWhiteSpace(driveDto.Name))
            {
                throw new InvalidOperationException("Drive entry is missing a name value.");
            }

            var drive = new DriveConfiguration(driveDto.Name)
            {
                Manufacturer = driveDto.Manufacturer ?? string.Empty,
                PartNumber = driveDto.PartNumber ?? string.Empty
            };

            if (driveDto.Voltages is null || driveDto.Voltages.Count == 0)
            {
                throw new InvalidOperationException($"Drive '{driveDto.Name}' must include at least one voltage configuration.");
            }

            foreach (var voltageDto in driveDto.Voltages)
            {
                var driveLabel = $"{driveDto.Name} ({voltageDto.Voltage}V)";
                MotorFileShapeValidator.ValidateVoltageDto(voltageDto, driveLabel);

                if (voltageDto.Voltage <= 0)
                {
                    throw new InvalidOperationException($"Voltage '{driveLabel}' must be positive.");
                }

                var voltage = new VoltageConfiguration(voltageDto.Voltage)
                {
                    Power = voltageDto.Power,
                    MaxSpeed = voltageDto.MaxSpeed,
                    RatedSpeed = voltageDto.RatedSpeed,
                    RatedContinuousTorque = voltageDto.RatedContinuousTorque,
                    RatedPeakTorque = voltageDto.RatedPeakTorque,
                    ContinuousAmperage = voltageDto.ContinuousAmperage,
                    PeakAmperage = voltageDto.PeakAmperage
                };

                foreach (var kvp in voltageDto.Series)
                {
                    var seriesName = kvp.Key;
                    var entry = kvp.Value;

                    var pointCount = voltageDto.Percent.Length;

                    if (entry.Torque.Length != pointCount)
                    {
                        throw new InvalidOperationException($"Series '{seriesName}' torque array length ({entry.Torque.Length}) must match axis length ({pointCount}) for drive '{driveDto.Name}'.");
                    }

                    var series = new CurveSeries(seriesName)
                    {
                        Locked = entry.Locked,
                        Notes = entry.Notes ?? string.Empty
                    };

                    for (var i = 0; i < pointCount; i++)
                    {
                        series.Data.Add(new DataPoint(voltageDto.Percent[i], voltageDto.Rpm[i], entry.Torque[i]));
                    }

                    voltage.Series.Add(series);
                }

                drive.Voltages.Add(voltage);
            }

            motor.Drives.Add(drive);
        }

        return motor;
    }

    private static UnitSettingsDto MapUnits(UnitSettings units)
    {
        ArgumentNullException.ThrowIfNull(units);
        return new UnitSettingsDto
        {
            Torque = units.Torque,
            Speed = units.Speed,
            Power = units.Power,
            Weight = units.Weight,
            Voltage = units.Voltage,
            Current = units.Current,
            Inertia = units.Inertia,
            TorqueConstant = units.TorqueConstant,
            Backlash = units.Backlash,
            ResponseTime = units.ResponseTime,
            Percentage = units.Percentage,
            Temperature = units.Temperature
        };
    }

    private static UnitSettings MapUnits(UnitSettingsDto dto)
    {
        return new UnitSettings
        {
            Torque = dto.Torque,
            Speed = dto.Speed,
            Power = dto.Power,
            Weight = dto.Weight,
            Voltage = dto.Voltage,
            Current = dto.Current,
            Inertia = dto.Inertia,
            TorqueConstant = dto.TorqueConstant,
            Backlash = dto.Backlash,
            ResponseTime = dto.ResponseTime,
            Percentage = dto.Percentage,
            Temperature = dto.Temperature
        };
    }

    private static MotorMetadata MapMetadata(MotorMetadataDto? dto)
    {
        var metadata = new MotorMetadata();
        if (dto is null)
        {
            return metadata;
        }

        metadata.Created = dto.Created;
        metadata.Modified = dto.Modified;
        metadata.Notes = dto.Notes ?? string.Empty;
        return metadata;
    }

    private static MotorMetadataDto MapMetadata(MotorMetadata metadata)
    {
        return new MotorMetadataDto
        {
            Created = metadata.Created,
            Modified = metadata.Modified,
            Notes = string.IsNullOrWhiteSpace(metadata.Notes) ? null : metadata.Notes
        };
    }
}
