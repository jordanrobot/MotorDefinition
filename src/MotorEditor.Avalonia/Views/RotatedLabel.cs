using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;

namespace CurveEditor.Views;

public sealed class RotatedLabel : Decorator
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<RotatedLabel, string?>(nameof(Text));

    public static readonly StyledProperty<double> AngleProperty =
        AvaloniaProperty.Register<RotatedLabel, double>(nameof(Angle), -90);

    private readonly TextBlock _textBlock;

    public RotatedLabel()
    {
        _textBlock = new TextBlock
        {
            FontSize = 11,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.NoWrap,
            TextTrimming = TextTrimming.None,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative),
        };

        _textBlock.Bind(TextBlock.TextProperty, new Binding { Source = this, Path = nameof(Text) });

        // Keep text color in sync with the containing Button's Foreground (Active state, etc.).
        _textBlock.Bind(
            TextBlock.ForegroundProperty,
            new Binding
            {
                Path = nameof(TemplatedControl.Foreground),
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor)
                {
                    AncestorType = typeof(Button)
                }
            });

        Child = _textBlock;
        UpdateRotation();

        AngleProperty.Changed.AddClassHandler<RotatedLabel>((control, _) => control.UpdateRotation());
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public double Angle
    {
        get => GetValue(AngleProperty);
        set => SetValue(AngleProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        _textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        var desired = _textBlock.DesiredSize;

        if (IsQuarterTurn(Angle))
        {
            return new Size(desired.Height, desired.Width);
        }

        return desired;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var desired = _textBlock.DesiredSize;

        var x = (finalSize.Width - desired.Width) / 2;
        var y = (finalSize.Height - desired.Height) / 2;

        _textBlock.Arrange(new Rect(x, y, desired.Width, desired.Height));
        return finalSize;
    }

    private void UpdateRotation()
    {
        _textBlock.RenderTransform = new RotateTransform(Angle);
    }

    private static bool IsQuarterTurn(double angle)
    {
        // Treat +/-90, +/-270, etc. as quarter turns.
        var normalized = angle % 180;
        return Math.Abs(normalized) > 0.0001;
    }
}
