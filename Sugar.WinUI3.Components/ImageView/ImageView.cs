using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.System;

namespace Sugar.WinUI3.Components;

[TemplatePart(Name = ElementPanelMain, Type = typeof(Panel))]
[TemplatePart(Name = ElementImageMain, Type = typeof(Image))]
[TemplatePart(Name = ElementButtonBack, Type = typeof(Button))]
[TemplatePart(Name = ElementButtonFore, Type = typeof(Button))]
public sealed class ImageView : Control
{
    #region Constants

    private const string ElementPanelMain = "PART_PanelMain";

    private const string ElementImageMain = "PART_ImageMain";

    private const string ElementButtonBack = "PART_ButtonBack";

    private const string ElementButtonFore = "PART_ButtonFore";

    private const double ScaleInterval = 0.2;

    private const double MaxZoomFactor = 50;

    private const double DoubleZero = 0.001;

    #endregion Constants

    #region ctor

    public ImageView()
    {
        _isLoaded = false;
        _isOblique = false;
        _scaled = false;

        DefaultStyleKey = typeof(ImageView);
        Loaded += ImageViewer_Loaded;
    }

    #endregion ctor

    #region Variables

    private Panel _panelMain;

    private Image _imageMain;

    private Button _buttonBack;

    private Button _buttonFore;

    private bool _isLoaded;

    private readonly bool _isOblique;

    private double _scaleXInterval;

    private double _scaleYInterval;

    private double _aspectRatio;

    private Point _pointerPressedPoint;

    private bool _isPointerLeftButtonPressed;

    private Thickness _pointerPressedMargin;

    private double _minZoomFactor;

    private double _maxZoomFactor;

    private ImageProperties _imageProperties;

    private bool _scaled;

    #endregion Variables

    #region Properties

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource),
        typeof(IEnumerable<StorageFile>),
        typeof(ImageView),
        new PropertyMetadata(default(IEnumerable<StorageFile>), OnItemsSourcePropertyChanged));

    private static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue != null && e.OldValue != e.NewValue)
        {
            if (d is ImageView ctrl && e.NewValue is IEnumerable<StorageFile> newValue)
            {
                ctrl.SelectedItem = newValue.ElementAt(ctrl.SelectedIndex);
            }
        }
    }

    public IEnumerable<StorageFile> ItemsSource
    {
        get => (IEnumerable<StorageFile>)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(
        nameof(SelectedIndex), typeof(int), typeof(ImageView), new PropertyMetadata(0, OnSelectedIndexPropertyChanged));

    private static void OnSelectedIndexPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue != null && e.OldValue != e.NewValue)
        {
            if (d is ImageView ctrl && e.NewValue is int newValue)
            {
                if (ctrl.ItemsSource?.Count() > 0)
                {
                    ctrl.SelectedItem = ctrl.ItemsSource.ElementAt(newValue);
                }
            }
        }
    }

    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public static readonly DependencyProperty FilePathProperty = DependencyProperty.Register(
        nameof(FilePath), typeof(string), typeof(ImageView), new PropertyMetadata(default(string)));

    public string FilePath
    {
        get => (string)GetValue(FilePathProperty);
        set => SetValue(FilePathProperty, value);
    }

    public static readonly DependencyProperty ZoomFactorTextProperty = DependencyProperty.Register(
        nameof(ZoomFactorText), typeof(string), typeof(ImageView), new PropertyMetadata(default(string)));

    public string ZoomFactorText
    {
        get => (string)GetValue(ZoomFactorTextProperty);
        set => SetValue(ZoomFactorTextProperty, value);
    }

    internal static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
        nameof(SelectedItem), typeof(object), typeof(ImageView), new PropertyMetadata(default, OnSelectedItemPropertyChanged));

    private static async void OnSelectedItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue != e.NewValue && d is ImageView ctrl && e.NewValue is StorageFile newValue)
        {
            ctrl.FilePath = newValue.Path;
            await ctrl.InitializeAsync();
            await Task.CompletedTask;
        }
    }

    internal StorageFile SelectedItem
    {
        get => (StorageFile)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    internal static readonly DependencyProperty ZoomFactorProperty = DependencyProperty.Register(
        nameof(ZoomFactor), typeof(double), typeof(ImageView), new PropertyMetadata(1.0, OnZoomFactorPropertyChanged));

    private static void OnZoomFactorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ImageView ctrl && e.NewValue is double newValue)
        {
            ctrl.ImageWidth = ctrl.ImagePixelWidth * newValue;
            ctrl.ImageHeight = ctrl.ImagePixelHeight * newValue;
            ctrl.ZoomFactorText = $"{newValue * 100:#0}%";
        }
    }

    internal double ZoomFactor
    {
        get => (double)GetValue(ZoomFactorProperty);
        set => SetValue(ZoomFactorProperty, value);
    }

    internal static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
        nameof(Source), typeof(BitmapImage), typeof(ImageView), new PropertyMetadata(default(BitmapImage)));

    internal BitmapImage Source
    {
        get => (BitmapImage)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    internal static readonly DependencyProperty ImageWidthProperty = DependencyProperty.Register(
        nameof(ImageWidth), typeof(double), typeof(ImageView), new PropertyMetadata(0.0));

    internal double ImageWidth
    {
        get => (double)GetValue(ImageWidthProperty);
        set => SetValue(ImageWidthProperty, value);
    }

    internal static readonly DependencyProperty ImageHeightProperty = DependencyProperty.Register(
        nameof(ImageHeight), typeof(double), typeof(ImageView), new PropertyMetadata(0.0));

    internal double ImageHeight
    {
        get => (double)GetValue(ImageHeightProperty);
        set => SetValue(ImageHeightProperty, value);
    }

    internal static readonly DependencyProperty ImageMarginProperty = DependencyProperty.Register(
        nameof(ImageMargin), typeof(Thickness), typeof(ImageView), new PropertyMetadata(default(Thickness)));

    internal Thickness ImageMargin
    {
        get => (Thickness)GetValue(ImageMarginProperty);
        set => SetValue(ImageMarginProperty, value);
    }

    internal double ImagePixelWidth
    {
        get;
        private set;
    }

    internal double ImagePixelHeight
    {
        get;
        private set;
    }

    #endregion Properties

    #region Methods

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _panelMain = GetTemplateChild(ElementPanelMain) as Panel;
        _imageMain = GetTemplateChild(ElementImageMain) as Image;
        _buttonBack = GetTemplateChild(ElementButtonBack) as Button;
        _buttonFore = GetTemplateChild(ElementButtonFore) as Button;

        if (_imageMain != null)
        {
            _imageMain.PointerPressed += ImageMain_PointerPressed;
            _imageMain.PointerReleased += ImageMain_PointerReleased;
            _imageMain.PointerExited += ImageMain_PointerExited;
            _imageMain.PointerMoved += ImageMain_PointerMoved;
        }

        if (_buttonBack != null)
        {
            _buttonBack.Click += ButtonPrev_Click;
        }

        if (_buttonFore != null)
        {
            _buttonFore.Click += ButtonNext_Click;
        }

        SizeChanged += ImageViewer_SizeChanged;
    }

    private async Task InitializeAsync()
    {
        try
        {
            if (SelectedItem != null && _isLoaded)
            {
                _imageProperties = await SelectedItem.Properties.GetImagePropertiesAsync();
                SetZoomFactor();
                ImageMargin = new Thickness((ActualWidth - ImageWidth) / 2, (ActualHeight - ImageHeight) / 2, 0, 0);
                Source = new BitmapImage(new(SelectedItem.Path))
                {
                    DecodePixelWidth = (int)ImageWidth,
                    DecodePixelHeight = (int)ImageHeight,
                };
                //Source = new BitmapImage(new(SelectedItem.Path));
            }
        }
        finally
        {
            await Task.CompletedTask;
        }
    }

    private void SetZoomFactor()
    {
        double width;
        double height;
        if (!_isOblique)
        {
            width = _imageProperties.Width;
            height = _imageProperties.Height;
        }
        else
        {
            width = _imageProperties.Height;
            height = _imageProperties.Width;
        }

        if (Math.Abs(height - 0) < DoubleZero || Math.Abs(width - 0) < DoubleZero)
        {
            return;
        }

        ImagePixelWidth = width;
        ImagePixelHeight = height;

        _scaleXInterval = ImagePixelWidth * ScaleInterval;
        _scaleYInterval = ImagePixelHeight * ScaleInterval;

        _aspectRatio = height / width;
        var zoomWindow = ActualHeight / ActualWidth;
        if (_aspectRatio > zoomWindow && height > ActualHeight)
        {
            ZoomFactor = ActualHeight / height;
        }
        else if (width > ActualWidth)
        {
            ZoomFactor = ActualWidth / width;
        }
        else
        {
            ZoomFactor = 1;
        }

        _minZoomFactor = ZoomFactor;
        _maxZoomFactor = MaxZoomFactor / ZoomFactor;
    }

    private void Zoom(Point currentPoint, bool isEnlarge)
    {
        if (Source is null || !_isLoaded || _isPointerLeftButtonPressed)
        {
            return;
        }

        var oldImageWidth = ImageWidth;
        var oldImageHeight = ImageHeight;

        var zoomFactor = isEnlarge ? ZoomFactor + ScaleInterval : ZoomFactor - ScaleInterval;

        if (Math.Abs(zoomFactor) < _minZoomFactor)
        {
            if (ZoomFactor > _minZoomFactor)
            {
                ZoomFactor = _minZoomFactor;
            }
            else
            {
                return;
            }
        }
        else if (Math.Abs(zoomFactor) > _maxZoomFactor)
        {
            if (ZoomFactor < _maxZoomFactor)
            {
                ZoomFactor = _maxZoomFactor;
            }
            else
            {
                return;
            }
        }
        else
        {
            ZoomFactor = zoomFactor;
        }

        var imgPoint = new Point(currentPoint.X - ImageMargin.Left, currentPoint.Y - ImageMargin.Top);

        var marginX = .5 * _scaleXInterval;
        var marginY = .5 * _scaleYInterval;

        if (ImageWidth > ActualWidth && ImageHeight > ActualHeight)
        {
            marginX = imgPoint.X / oldImageWidth * _scaleXInterval;
            marginY = imgPoint.Y / oldImageHeight * _scaleYInterval;
        }

        Thickness thickness;
        if (isEnlarge)
        {
            thickness = new Thickness(ImageMargin.Left - marginX, ImageMargin.Top - marginY, 0, 0);
        }
        else
        {
            var marginActualX = ImageMargin.Left + marginX;
            var marginActualY = ImageMargin.Top + marginY;
            var deltaX = ImageWidth - ActualWidth;
            var deltaY = ImageHeight - ActualHeight;

            if (Math.Abs(ImageMargin.Left) < DoubleZero)
            {
                marginActualX = ImageMargin.Left;
            }

            if (Math.Abs(ImageMargin.Top) < DoubleZero)
            {
                marginActualY = ImageMargin.Top;
            }

            if (deltaX < DoubleZero)
            {
                marginActualX = (ActualWidth - ImageWidth) / 2;
            }

            if (deltaY < DoubleZero)
            {
                marginActualY = (ActualHeight - ImageHeight) / 2;
            }

            thickness = new Thickness(marginActualX, marginActualY, 0, 0);
        }

        ImageMargin = thickness;
    }

    private void SetSelectedIndex(int offset)
    {
        if (ItemsSource == null)
        {
            return;
        }

        var index = SelectedIndex + offset;
        index = Math.Clamp(index, 0, ItemsSource.Count() - 1);
        if (index == SelectedIndex)
        {
            return;
        }

        SelectedIndex = index;
    }

    private void OnSizeChanged()
    {
        var marginX = ImageMargin.Left;
        var marginY = ImageMargin.Top;

        if (ImageWidth <= ActualWidth)
        {
            marginX = (ActualWidth - ImageWidth) / 2;
        }

        if (ImageHeight <= ActualHeight)
        {
            marginY = (ActualHeight - ImageHeight) / 2;
        }

        ImageMargin = new Thickness(marginX, marginY, 0, 0);
    }

    #endregion Methods

    #region Events

    public void OnScaled()
    {
        if (_scaled)
        {
            Source.DecodePixelWidth = (int)ImageWidth;
            Source.DecodePixelHeight = (int)ImageHeight;
            _scaled = false;
        }
    }

    protected override void OnPointerWheelChanged(PointerRoutedEventArgs e)
    {
        if (e.KeyModifiers == VirtualKeyModifiers.Control)
        {
            var pointer = e.GetCurrentPoint(_panelMain);
            Zoom(pointer.Position, pointer.Properties.MouseWheelDelta > 0);
            _scaled = true;
        }
        else
        {
            SetSelectedIndex(e.GetCurrentPoint(this).Properties.MouseWheelDelta > 0 ? 1 : -1);
        }
        //e.Handled = true;
    }

    private async void ImageViewer_Loaded(object sender, RoutedEventArgs e)
    {
        _isLoaded = true;
        await InitializeAsync();
        await Task.CompletedTask;
    }

    private void ImageViewer_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (ImageWidth < DoubleZero || ImageHeight < DoubleZero)
        {
            return;
        }
        OnSizeChanged();
    }

    private void ImageMain_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (e.GetCurrentPoint(_imageMain).Properties.IsLeftButtonPressed)
        {
            _pointerPressedPoint = e.GetCurrentPoint(_panelMain).Position;
            _pointerPressedMargin = ImageMargin;
            _isPointerLeftButtonPressed = true;
        }
    }

    private void ImageMain_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!_isPointerLeftButtonPressed)
        {
            return;
        }

        var currentPoint = e.GetCurrentPoint(_panelMain).Position;
        var deltaX = currentPoint.X - _pointerPressedPoint.X;
        var deltaY = currentPoint.Y - _pointerPressedPoint.Y;

        var marginX = _pointerPressedMargin.Left;
        if (ImageWidth > ActualWidth)
        {
            marginX = _pointerPressedMargin.Left + deltaX;
            if (marginX >= 0)
            {
                marginX = 0;
            }
            else if (-marginX + ActualWidth >= ImageWidth)
            {
                marginX = ActualWidth - ImageWidth;
            }
        }

        var marginY = _pointerPressedMargin.Top;
        if (ImageHeight > ActualHeight)
        {
            marginY = _pointerPressedMargin.Top + deltaY;
            if (marginY >= 0)
            {
                marginY = 0;
            }
            else if (-marginY + ActualHeight >= ImageHeight)
            {
                marginY = ActualHeight - ImageHeight;
            }
        }

        ImageMargin = new Thickness(marginX, marginY, 0, 0);
    }

    private void ImageMain_PointerExited(object sender, PointerRoutedEventArgs e) => _isPointerLeftButtonPressed = false;

    private void ImageMain_PointerReleased(object sender, PointerRoutedEventArgs e) => _isPointerLeftButtonPressed = false;

    private void ButtonPrev_Click(object sender, RoutedEventArgs e) => SetSelectedIndex(1);

    private void ButtonNext_Click(object sender, RoutedEventArgs e) => SetSelectedIndex(-1);

    #endregion Events
}