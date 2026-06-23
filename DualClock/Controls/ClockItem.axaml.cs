using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace DualClock.Controls;

public partial class ClockItem : UserControl
{
    public ClockItem()
    {
        InitializeComponent();
        if (TimeBackground == null)
        {
            var defaultGrad = this.FindResource("DefaultGradient") as IBrush;
            if (defaultGrad != null)
            {
                PART_TimeBorder.Background = defaultGrad;
            }
        }
    }

    // 供外部调用的设置日期方法
    public void SetDate(string date)
    {
        PART_DateTextBlock.Text = date;
    }

    // 供外部调用的设置时间方法
    public void SetTime(string time)
    {
        PART_TimeTextBlock.Text = time;
    }

    // 依赖属性，用于在 XAML 中单独设置背景渐变
    public static readonly StyledProperty<IBrush> TimeBackgroundProperty =
        AvaloniaProperty.Register<ClockItem, IBrush>(nameof(TimeBackground));

    public IBrush TimeBackground
    {
        get => GetValue(TimeBackgroundProperty);
        set => SetValue(TimeBackgroundProperty, value);
    }
}