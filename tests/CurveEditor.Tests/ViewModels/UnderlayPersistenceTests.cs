using System;
using System.IO;
using CurveEditor.Services;
using CurveEditor.ViewModels;
using MotorEditor.Avalonia.Models;
using Xunit;

namespace MotorEditor.Tests.ViewModels;

public sealed class UnderlayPersistenceTests : IDisposable
{
    private readonly string tempRoot;

    public UnderlayPersistenceTests()
    {
        tempRoot = Path.Combine(Path.GetTempPath(), $"underlay-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);
    }

    [Fact]
    public void UnderlayMetadataService_PersistsLockZero()
    {
        var motorPath = Path.Combine(tempRoot, "motor.json");
        File.WriteAllText(motorPath, "{}");

        var service = new UnderlayMetadataService();
        var metadata = new UnderlayMetadata
        {
            ImagePath = "/tmp/example.png",
            IsVisible = true,
            LockZero = true,
            XScale = 1.5,
            YScale = 0.8,
            OffsetX = 0.1,
            OffsetY = -0.2
        };

        service.Save(motorPath, "DriveA", 480, metadata);
        var loaded = service.Load(motorPath, "DriveA", 480);

        Assert.NotNull(loaded);
        Assert.Equal(metadata.ImagePath, loaded!.ImagePath);
        Assert.True(loaded.LockZero);
        Assert.Equal(metadata.XScale, loaded.XScale);
        Assert.Equal(metadata.YScale, loaded.YScale);
        Assert.Equal(metadata.OffsetX, loaded.OffsetX);
        Assert.Equal(metadata.OffsetY, loaded.OffsetY);
        Assert.True(loaded.IsVisible);
    }

    [Fact]
    public void ChartViewModel_AppliesMetadataWithLockZero()
    {
        var vm = new ChartViewModel();
        vm.SetActiveUnderlayKey("drive|480");

        var metadata = new UnderlayMetadata
        {
            ImagePath = null,
            IsVisible = false,
            LockZero = false,
            XScale = 2.0,
            YScale = 0.5,
            OffsetX = 0.3,
            OffsetY = -0.1
        };

        var applied = vm.TryApplyUnderlayMetadata(metadata, out var error);

        Assert.True(applied);
        Assert.Null(error);
        Assert.False(vm.UnderlayLockZero);
        Assert.Equal(2.0, vm.UnderlayXScale);
        Assert.Equal(0.5, vm.UnderlayYScale);
        Assert.Equal(0.3, vm.UnderlayOffsetX);
        Assert.Equal(-0.1, vm.UnderlayOffsetY);
        Assert.False(vm.UnderlayVisible);
        Assert.False(vm.HasUnderlayImage);
    }

    [Fact]
    public void UnderlayScaleChange_WithPositiveAnchor_KeepsOriginAligned()
    {
        var vm = new ChartViewModel();
        vm.SetActiveUnderlayKey("drive|480");
        vm.UnderlayOffsetX = -0.5;
        vm.UnderlayOffsetY = -0.25;

        vm.UnderlayXScale = 2.0;
        vm.UnderlayYScale = 0.5;

        Assert.Equal(-1.0, vm.UnderlayOffsetX);
        Assert.Equal(-0.125, vm.UnderlayOffsetY);
        Assert.Equal(0.5, -vm.UnderlayOffsetX / vm.UnderlayXScale);
        Assert.Equal(0.25, -vm.UnderlayOffsetY / vm.UnderlayYScale);
    }

    [Fact]
    public void UnderlayScaleChange_WithNegativeAnchor_KeepsOriginAligned()
    {
        var vm = new ChartViewModel();
        vm.SetActiveUnderlayKey("drive|208");
        vm.UnderlayOffsetX = 0.25;
        vm.UnderlayOffsetY = 0.1;

        vm.UnderlayXScale = 3.0;
        vm.UnderlayYScale = 2.0;

        Assert.Equal(0.75, vm.UnderlayOffsetX);
        Assert.Equal(0.2, vm.UnderlayOffsetY);
        Assert.Equal(-0.25, -vm.UnderlayOffsetX / vm.UnderlayXScale);
        Assert.Equal(-0.1, -vm.UnderlayOffsetY / vm.UnderlayYScale);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(tempRoot, true);
        }
        catch
        {
            // best effort cleanup
        }
    }
}
