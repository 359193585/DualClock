using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace DualClock.Controls
{
    public partial class AnalogClock : UserControl
    {
        public static readonly StyledProperty<DateTime> TimeProperty =
            AvaloniaProperty.Register<AnalogClock, DateTime>(nameof(Time), defaultValue: DateTime.Now);

        public DateTime Time
        {
            get => GetValue(TimeProperty);
            set => SetValue(TimeProperty, value);
        }

        public static readonly StyledProperty<IBrush?> BackgroundBrushProperty =
            AvaloniaProperty.Register<AnalogClock, IBrush?>(nameof(BackgroundBrush), defaultValue: null);

        public IBrush? BackgroundBrush
        {
            get => GetValue(BackgroundBrushProperty);
            set => SetValue(BackgroundBrushProperty, value);
        }

        private Canvas? _clockCanvas;
        private Ellipse? _faceEllipse;
        private Path? _hourHand;
        private Path? _minuteHand;
        private Path? _secondHand;
        private Ellipse? _centerDot;
        private TextBlock? _dateText;

        public AnalogClock()
        {
            InitializeComponent();

            _clockCanvas = this.FindControl<Canvas>("ClockCanvas");
            _faceEllipse = this.FindControl<Ellipse>("FaceEllipse");
            _hourHand = this.FindControl<Path>("HourHand");
            _minuteHand = this.FindControl<Path>("MinuteHand");
            _secondHand = this.FindControl<Path>("SecondHand");
            _centerDot = this.FindControl<Ellipse>("CenterDot");
            _dateText = this.FindControl<TextBlock>("DateText");

            if (_clockCanvas != null)
            {
                _clockCanvas.SizeChanged += (s, e) => ArrangeClock();
                // 确保初始尺寸触发一次布局
                _clockCanvas.InvalidateMeasure();
            }

            this.PropertyChanged += OnPropertyChanged;

            UpdateClock();
            UpdateBackground();
        }

     
        private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == TimeProperty)
            {
                UpdateClock();
                UpdateBackground();
            }
            else if (e.Property == BackgroundBrushProperty)
            {
                if (_faceEllipse != null && e.NewValue is IBrush brush)
                    _faceEllipse.Fill = brush;
            }
        }

        private void UpdateClock()
        {
            var time = Time;
            double hourAngle = (time.Hour % 12) * 30 + time.Minute * 0.5;
            double minuteAngle = time.Minute * 6 + time.Second * 0.1;
            double secondAngle = time.Second * 6;

            if (_hourHand != null)
                ((RotateTransform)_hourHand.RenderTransform!).Angle = hourAngle;
            if (_minuteHand != null)
                ((RotateTransform)_minuteHand.RenderTransform!).Angle = minuteAngle;
            if (_secondHand != null)
                ((RotateTransform)_secondHand.RenderTransform!).Angle = secondAngle;

            if (_dateText != null)
                _dateText.Text = time.ToString("MM/dd");
        }

        private void UpdateBackground()
        {
            if (BackgroundBrush != null)
                return;

            var time = Time;
            bool isDay = time.Hour >= 6 && time.Hour < 18;
            if (_faceEllipse != null)
                _faceEllipse.Fill = isDay ? new SolidColorBrush(Colors.LightGoldenrodYellow) : new SolidColorBrush(Colors.DarkSlateGray);
        }

        private void GenerateTicks(double radius, double centerX, double centerY)
        {
            if (_clockCanvas == null) return;

            // 移除旧的刻度线（保留已命名的控件）
            var toRemove = _clockCanvas.Children.Where(c => string.IsNullOrEmpty(c.Name)).ToList();
            foreach (var child in toRemove)
                _clockCanvas.Children.Remove(child);

            if (radius <= 0) return;

            for (int i = 0; i < 12; i++)
            {
                double angle = i * 30 - 90;
                double rad = angle * Math.PI / 180;
                double inner = radius * 0.85;
                double outer = radius;
                double x1 = centerX + inner * Math.Cos(rad);
                double y1 = centerY + inner * Math.Sin(rad);
                double x2 = centerX + outer * Math.Cos(rad);
                double y2 = centerY + outer * Math.Sin(rad);

                var line = new Line
                {
                    StartPoint = new Avalonia.Point(x1, y1),
                    EndPoint = new Avalonia.Point(x2, y2),
                    Stroke = Brushes.Black,
                    StrokeThickness = 2
                };
                _clockCanvas.Children.Add(line);
            }
        }

        private void ArrangeClock()
        {
            if (_clockCanvas == null || _faceEllipse == null) return;
            double width = _clockCanvas.Bounds.Width;
            double height = _clockCanvas.Bounds.Height;
            if (width <= 0 || height <= 0) return;

            double centerX = width / 2;
            double centerY = height / 2;
            double radius = Math.Min(width, height) / 2 - 10; // 减去边框

            // 1. 表盘
            double faceSize = radius * 2 + 4;
            _faceEllipse.Width = faceSize;
            _faceEllipse.Height = faceSize;
            Canvas.SetLeft(_faceEllipse, centerX - faceSize / 2);
            Canvas.SetTop(_faceEllipse, centerY - faceSize / 2);

            // 2. 指针长度：根据半径按比例确定
            double hourLength = radius * 0.5;   // 时针长度
            double minuteLength = radius * 0.7; // 分针长度
            double secondLength = radius * 0.8; // 秒针长度

            // 更新指针几何图形
            if (_hourHand != null)
            {
                _hourHand.Data = new LineGeometry(new Point(0, 0), new Point(0, -hourLength));
                // 指针宽度也可随大小调整（可选）
                _hourHand.StrokeThickness = Math.Max(3, radius * 0.04);
            }
            if (_minuteHand != null)
            {
                _minuteHand.Data = new LineGeometry(new Point(0, 0), new Point(0, -minuteLength));
                _minuteHand.StrokeThickness = Math.Max(2, radius * 0.03);
            }
            if (_secondHand != null)
            {
                _secondHand.Data = new LineGeometry(new Point(0, 0), new Point(0, -secondLength));
                _secondHand.StrokeThickness = Math.Max(1, radius * 0.02);
            }

            // 3. 指针定位：支点在 (0,0)，所以左上角置于中心
            Canvas.SetLeft(_hourHand, centerX);
            Canvas.SetTop(_hourHand, centerY);
            Canvas.SetLeft(_minuteHand, centerX);
            Canvas.SetTop(_minuteHand, centerY);
            Canvas.SetLeft(_secondHand, centerX);
            Canvas.SetTop(_secondHand, centerY);

            // 4. 中心点大小按比例缩放
            if (_centerDot != null)
            {
                double dotSize = Math.Max(6, radius * 0.06);
                _centerDot.Width = dotSize;
                _centerDot.Height = dotSize;
                Canvas.SetLeft(_centerDot, centerX - dotSize / 2);
                Canvas.SetTop(_centerDot, centerY - dotSize / 2);
            }

            // 5. 日期文本
            if (_dateText != null)
            {
                _dateText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                double textWidth = _dateText.DesiredSize.Width;
                double textHeight = _dateText.DesiredSize.Height;
                double dateY = centerY + radius * 0.65 - textHeight / 2;
                Canvas.SetLeft(_dateText, centerX - textWidth / 2);
                Canvas.SetTop(_dateText, dateY);
            }

            // 6. 生成刻度（刻度线长度也会按半径调整，已在 GenerateTicks 中处理）
            GenerateTicks(radius, centerX, centerY);
        }
    }
}