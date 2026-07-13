using System;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace DualClock.Controls
{
    public enum DateFormatMode
    {
        Full,           // yyyy-MM-dd
        MonthDay,       // MM-dd
        MonthDayWeek    // MM-dd ddd
    }

    public class AnalogClockPlus : AnalogClock
    {
        #region // 新增依赖属性

        // 1. 显示小时数字（12、3、6、9）
        public static readonly StyledProperty<bool> ShowHourNumbersProperty =
            AvaloniaProperty.Register<AnalogClockPlus, bool>(nameof(ShowHourNumbers), defaultValue: false);

        // 2. 显示分钟刻度
        public static readonly StyledProperty<bool> ShowMinuteTicksProperty =
            AvaloniaProperty.Register<AnalogClockPlus, bool>(nameof(ShowMinuteTicks), defaultValue: false);

        // 3. 日期格式
        public static readonly StyledProperty<DateFormatMode> DateFormatProperty =
            AvaloniaProperty.Register<AnalogClockPlus, DateFormatMode>(nameof(DateFormat), defaultValue: DateFormatMode.MonthDay);

        // 4. 整点刻度颜色
        public static readonly StyledProperty<IBrush> TickColorProperty =
    AvaloniaProperty.Register<AnalogClockPlus, IBrush>(nameof(TickColor), defaultValue: Brushes.Black);

        // 5. 整点刻度粗细
        public static readonly StyledProperty<double> TickThicknessProperty =
            AvaloniaProperty.Register<AnalogClockPlus, double>(nameof(TickThickness), defaultValue: 2.0);

        // 6. 分钟刻度颜色
        public static readonly StyledProperty<IBrush> MinuteTickColorProperty =
            AvaloniaProperty.Register<AnalogClockPlus, IBrush>(nameof(MinuteTickColor), defaultValue: Brushes.Black);

        // 7. 分钟刻度粗细
        public static readonly StyledProperty<double> MinuteTickThicknessProperty =
            AvaloniaProperty.Register<AnalogClockPlus, double>(nameof(MinuteTickThickness), defaultValue: 1.0);

        // 8. 日期文字颜色
        public static readonly StyledProperty<IBrush> DateTextColorProperty =
            AvaloniaProperty.Register<AnalogClockPlus, IBrush>(nameof(DateTextColor), defaultValue: Brushes.Black);

        // 9. 日期文字大小
        public static readonly StyledProperty<double> DateFontSizeProperty =
            AvaloniaProperty.Register<AnalogClockPlus, double>(nameof(DateFontSize), defaultValue: 12.0);
        #endregion

        #region // ==================== CLR 属性 ====================
        public IBrush TickColor
        {
            get => GetValue(TickColorProperty);
            set => SetValue(TickColorProperty, value);
        }


        public bool ShowHourNumbers
        {
            get => GetValue(ShowHourNumbersProperty);
            set => SetValue(ShowHourNumbersProperty, value);
        }

        public bool ShowMinuteTicks
        {
            get => GetValue(ShowMinuteTicksProperty);
            set => SetValue(ShowMinuteTicksProperty, value);
        }

        public DateFormatMode DateFormat
        {
            get => GetValue(DateFormatProperty);
            set => SetValue(DateFormatProperty, value);
        }
        public double TickThickness
        {
            get => GetValue(TickThicknessProperty);
            set => SetValue(TickThicknessProperty, value);
        }

        public IBrush MinuteTickColor
        {
            get => GetValue(MinuteTickColorProperty);
            set => SetValue(MinuteTickColorProperty, value);
        }

        public double MinuteTickThickness
        {
            get => GetValue(MinuteTickThicknessProperty);
            set => SetValue(MinuteTickThicknessProperty, value);
        }

        public IBrush DateTextColor
        {
            get => GetValue(DateTextColorProperty);
            set => SetValue(DateTextColorProperty, value);
        }

        public double DateFontSize
        {
            get => GetValue(DateFontSizeProperty);
            set => SetValue(DateFontSizeProperty, value);
        }
        #endregion

        static AnalogClockPlus()
        {
            // 属性变化时重新绘制
            ShowHourNumbersProperty.Changed.AddClassHandler<AnalogClockPlus>((o, e) => o.OnAdditionalPropertyChanged(e));
            ShowMinuteTicksProperty.Changed.AddClassHandler<AnalogClockPlus>((o, e) => o.OnAdditionalPropertyChanged(e));
            DateFormatProperty.Changed.AddClassHandler<AnalogClockPlus>((o, e) => o.OnAdditionalPropertyChanged(e));
            TickColorProperty.Changed.AddClassHandler<AnalogClockPlus>((o, e) => o.OnAdditionalPropertyChanged(e));
            TickThicknessProperty.Changed.AddClassHandler<AnalogClockPlus>((o, e) => o.OnAdditionalPropertyChanged(e));
            MinuteTickColorProperty.Changed.AddClassHandler<AnalogClockPlus>((o, e) => o.OnAdditionalPropertyChanged(e));
            MinuteTickThicknessProperty.Changed.AddClassHandler<AnalogClockPlus>((o, e) => o.OnAdditionalPropertyChanged(e));
            DateTextColorProperty.Changed.AddClassHandler<AnalogClockPlus>((o, e) => o.OnAdditionalPropertyChanged(e));
            DateFontSizeProperty.Changed.AddClassHandler<AnalogClockPlus>((o, e) => o.OnAdditionalPropertyChanged(e));
        }

        public AnalogClockPlus()
        {
        }
        private void OnAdditionalPropertyChanged(AvaloniaPropertyChangedEventArgs e)
        {
            // 触发重新绘制
            InvalidateVisual();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            // 如果新属性变化，重新绘制
            if (e.Property == ShowHourNumbersProperty ||
                e.Property == ShowMinuteTicksProperty ||
                e.Property == DateFormatProperty)
            {
                // 触发重新布局，将调用 ArrangeClock
                InvalidateVisual();
            }
        }


        // 重写 ArrangeClock，在基类绘制后添加额外元素
        protected override void ArrangeClock()
        {
            base.ArrangeClock();

            // 由于我们不想频繁修改基类，我们临时通过 FindControl 获取。
            var canvas = this.FindControl<Canvas>("ClockCanvas");
            if (canvas == null) return;

            double width = canvas.Bounds.Width;
            double height = canvas.Bounds.Height;
            if (width <= 0 || height <= 0) return;

            double centerX = width / 2;
            double centerY = height / 2;
            double radius = Math.Min(width, height) / 2 - 10;

            // 生成数字和分钟刻度
            GenerateHourNumbers(radius, centerX, centerY);
            GenerateMinuteTicks(radius, centerX, centerY);
        }

        // 重写 UpdateClock 以应用日期格式
        protected override void UpdateClock()
        {
            base.UpdateClock(); // 基类会更新日期文本，此处需覆盖其内容

            // 获取日期文本控件
            var dateText = this.FindControl<TextBlock>("DateText");
            if (dateText == null) return;

            var time = Time;
            string dateString;
            var culture = new CultureInfo("zh-CN");
            switch (DateFormat)
            {
                case DateFormatMode.Full:
                    dateString = time.ToString("yyyy-MM-dd", culture);
                    break;
                case DateFormatMode.MonthDay:
                    dateString = time.ToString("MM-dd", culture);
                    break;
                case DateFormatMode.MonthDayWeek:
                default:
                    dateString = time.ToString("MM-dd ddd", culture);
                    break;
            }
            dateText.Text = dateString;
            dateText.Foreground = DateTextColor;
            dateText.FontSize = DateFontSize;
        }

        protected override void GenerateTicks(double radius, double centerX, double centerY)
        {
            var canvas = ClockCanvas;
            if (canvas == null) return;

            // 移除旧的整点刻度
            var toRemove = canvas.Children.Where(c => c.Name != null && c.Name.StartsWith("HourTick_")).ToList();
            foreach (var child in toRemove)
                canvas.Children.Remove(child);

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
                    Name = $"HourTick_{i}",
                    StartPoint = new Avalonia.Point(x1, y1),
                    EndPoint = new Avalonia.Point(x2, y2),
                    Stroke = TickColor,
                    StrokeThickness = TickThickness
                };
                canvas.Children.Add(line);
            }
        }

        // 新增方法：生成小时数字（12, 3, 6, 9）
        private void GenerateHourNumbers(double radius, double centerX, double centerY)
        {
            var canvas = ClockCanvas;
            if (canvas == null) return;

            var toRemove = canvas.Children.Where(c => c.Name != null && c.Name.StartsWith("HourNum_")).ToList();
            foreach (var child in toRemove)
                canvas.Children.Remove(child);

            if (!ShowHourNumbers || radius <= 0) return;

            double fontSize = Math.Max(12, radius * 0.12);
            for (int i = 0; i < 12; i++)
            {
                int hour = (i == 0) ? 12 : i;
                if (hour % 3 != 0) continue; // 只显示 12, 3, 6, 9

                double angle = i * 30 - 90;
                double rad = angle * Math.PI / 180;
                double distance = radius * 0.78;
                double x = centerX + distance * Math.Cos(rad);
                double y = centerY + distance * Math.Sin(rad);

                var text = new TextBlock
                {
                    Name = $"HourNum_{i}",
                    Text = hour.ToString(),
                    FontSize = fontSize,
                    FontWeight = FontWeight.Bold,
                    Foreground = TickColor, // 与整点刻度颜色一致，也可以单独用属性
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                };
                // 粗略居中
                Canvas.SetLeft(text, x - fontSize * 0.3);
                Canvas.SetTop(text, y - fontSize * 0.4);
                canvas.Children.Add(text);
            }
        }

        // 新增方法：生成分钟刻度（除整点外的所有刻度）
        private void GenerateMinuteTicks(double radius, double centerX, double centerY)
        {
            var canvas = ClockCanvas;
            if (canvas == null) return;

            var toRemove = canvas.Children.Where(c => c.Name != null && c.Name.StartsWith("MinTick_")).ToList();
            foreach (var child in toRemove)
                canvas.Children.Remove(child);

            if (!ShowMinuteTicks || radius <= 0) return;

            for (int i = 0; i < 60; i++)
            {
                if (i % 5 == 0) continue; // 跳过整点位置（由整点刻度覆盖）
                double angle = i * 6 - 90;
                double rad = angle * Math.PI / 180;
                double inner = radius * 0.88;
                double outer = radius * 0.93;
                double x1 = centerX + inner * Math.Cos(rad);
                double y1 = centerY + inner * Math.Sin(rad);
                double x2 = centerX + outer * Math.Cos(rad);
                double y2 = centerY + outer * Math.Sin(rad);

                var line = new Line
                {
                    Name = $"MinTick_{i}",
                    StartPoint = new Avalonia.Point(x1, y1),
                    EndPoint = new Avalonia.Point(x2, y2),
                    Stroke = MinuteTickColor,
                    StrokeThickness = MinuteTickThickness
                };
                canvas.Children.Add(line);
            }
        }
    }
}
