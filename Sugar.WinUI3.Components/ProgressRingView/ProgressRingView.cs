using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Sugar.WinUI3.Components;

public class ProgressRingView : Control
{
    public ProgressRingView()
    {
        DefaultStyleKey = typeof(ProgressRingView);
    }

    public static readonly DependencyProperty RingHeightProperty = DependencyProperty.Register(
        nameof(RingHeight), typeof(double), typeof(ProgressRingView), new PropertyMetadata(150));

    public double RingHeight
    {
        get => (double)GetValue(RingHeightProperty);
        set => SetValue(RingHeightProperty, value);
    }

    public static readonly DependencyProperty RingWidthProperty = DependencyProperty.Register(
        nameof(RingWidth), typeof(double), typeof(ProgressRingView), new PropertyMetadata(150));

    public double RingWidth
    {
        get => (double)GetValue(RingWidthProperty);
        set => SetValue(RingWidthProperty, value);
    }

    public static readonly DependencyProperty RingValueProperty = DependencyProperty.Register(
        nameof(RingValue), typeof(double), typeof(ProgressRingView), new PropertyMetadata(0.0, OnRingValueChanged));

    public double RingValue
    {
        get => (double)GetValue(RingValueProperty);
        set => SetValue(RingValueProperty, value);
    }

    private static void OnRingValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ProgressRingView ctrl && e.NewValue is double newValue)
        {
            ctrl.RingValueText = $"{newValue}%";
            ctrl.Visibility = newValue > 0.0001 ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    internal static readonly DependencyProperty RingValueTextProperty = DependencyProperty.Register(
        nameof(RingValueText), typeof(string), typeof(ProgressRingView), new PropertyMetadata("0%"));

    internal string RingValueText
    {
        get => (string)GetValue(RingValueTextProperty);
        set => SetValue(RingValueTextProperty, value);
    }
}