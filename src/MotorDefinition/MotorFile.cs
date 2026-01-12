using JordanRobot.MotorDefinition.Model;
using JordanRobot.MotorDefinition.Persistence.Dtos;
using JordanRobot.MotorDefinition.Persistence.Mapping;
using JordanRobot.MotorDefinition.Persistence.Probing;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace JordanRobot.MotorDefinition;

/// <summary>
/// Provides entrypoints for loading and saving motor definition files.
/// </summary>
public static class MotorFile
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            // Keep files human-readable while avoiding huge vertical expansion of numeric arrays.
            new Persistence.Json.CompactInt32ArrayJsonConverter(valuesPerLine: 12),
            new Persistence.Json.CompactDecimalArrayJsonConverter(valuesPerLine: 12)
        }
    };

    /// <summary>
    /// Determines whether a JSON document resembles a motor definition file.
    /// </summary>
    /// <param name="document">The JSON document to probe.</param>
    /// <returns>True when the document shape matches the motor definition format.</returns>
    public static bool IsLikelyMotorDefinition(JsonDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        return MotorFileProbe.IsLikelyMotorDefinition(document);
    }

    /// <summary>
    /// Loads a motor definition from the specified path.
    /// </summary>
    /// <param name="path">The file path to read.</param>
    /// <returns>The parsed motor definition.</returns>
    public static Model.ServoMotor Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        using var stream = File.OpenRead(path);
        return Load(stream);
    }

    /// <summary>
    /// Loads a motor definition from the specified path asynchronously.
    /// </summary>
    /// <param name="path">The file path to read.</param>
    /// <param name="cancellationToken">The cancellation token to observe.</param>
    /// <returns>The parsed motor definition.</returns>
    public static async Task<Model.ServoMotor> LoadAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        await using var stream = File.OpenRead(path);
        return await LoadAsync(stream, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Saves a motor definition to the specified path.
    /// </summary>
    /// <param name="motor">The motor definition to persist.</param>
    /// <param name="path">The destination file path.</param>
    public static void Save(Model.ServoMotor motor, string path)
    {
        ArgumentNullException.ThrowIfNull(motor);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var dto = MotorFileMapper.ToFileDto(motor);
        var json = JsonSerializer.Serialize(dto, JsonOptions);
        File.WriteAllText(path, json);
    }

    /// <summary>
    /// Saves a motor definition to the specified path asynchronously.
    /// </summary>
    /// <param name="motor">The motor definition to persist.</param>
    /// <param name="path">The destination file path.</param>
    /// <param name="cancellationToken">The cancellation token to observe.</param>
    public static async Task SaveAsync(Model.ServoMotor motor, string path, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(motor);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var dto = MotorFileMapper.ToFileDto(motor);
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, dto, JsonOptions, cancellationToken).ConfigureAwait(false);
    }

    private static Model.ServoMotor Load(Stream stream)
    {
        var dto = JsonSerializer.Deserialize<MotorDefinitionFileDto>(stream, JsonOptions);
        if (dto is null)
        {
            throw new InvalidOperationException("Failed to deserialize motor definition file.");
        }

        return MotorFileMapper.ToRuntimeModel(dto);
    }

    private static async Task<Model.ServoMotor> LoadAsync(Stream stream, CancellationToken cancellationToken)
    {
        var dto = await JsonSerializer.DeserializeAsync<MotorDefinitionFileDto>(stream, JsonOptions, cancellationToken).ConfigureAwait(false);
        if (dto is null)
        {
            throw new InvalidOperationException("Failed to deserialize motor definition file.");
        }

        return MotorFileMapper.ToRuntimeModel(dto);
    }
}
