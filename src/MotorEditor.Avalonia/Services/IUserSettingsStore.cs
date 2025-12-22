using System.Collections.Generic;
using CurveEditor.Behaviors;

namespace CurveEditor.Services;

public interface IUserSettingsStore
{
    string? LoadString(string settingsKey);
    void SaveString(string settingsKey, string? value);

    bool LoadBool(string settingsKey, bool defaultValue);
    void SaveBool(string settingsKey, bool value);

    double LoadDouble(string settingsKey, double defaultValue);
    void SaveDouble(string settingsKey, double value);

    IReadOnlyList<string> LoadStringArrayFromJson(string settingsKey);
    void SaveStringArrayAsJson(string settingsKey, IReadOnlyList<string> values);
}

public sealed class PanelLayoutUserSettingsStore : IUserSettingsStore
{
    public string? LoadString(string settingsKey) => PanelLayoutPersistence.LoadString(settingsKey);
    public void SaveString(string settingsKey, string? value) => PanelLayoutPersistence.SaveString(settingsKey, value);

    public bool LoadBool(string settingsKey, bool defaultValue) => PanelLayoutPersistence.LoadBool(settingsKey, defaultValue);
    public void SaveBool(string settingsKey, bool value) => PanelLayoutPersistence.SaveBool(settingsKey, value);

    public double LoadDouble(string settingsKey, double defaultValue) => PanelLayoutPersistence.LoadDouble(settingsKey, defaultValue);
    public void SaveDouble(string settingsKey, double value) => PanelLayoutPersistence.SaveDouble(settingsKey, value);

    public IReadOnlyList<string> LoadStringArrayFromJson(string settingsKey) => PanelLayoutPersistence.LoadStringArrayFromJson(settingsKey);
    public void SaveStringArrayAsJson(string settingsKey, IReadOnlyList<string> values) => PanelLayoutPersistence.SaveStringArrayAsJson(settingsKey, values);
}
