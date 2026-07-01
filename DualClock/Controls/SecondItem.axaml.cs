using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DualClock.Controls;

public partial class SecondItem : UserControl
{
    public SecondItem()
    {
        InitializeComponent();
    }
    // 供外部调用的设置秒方法
    public void SetSecond(string second)
    {
        PART_SecondTextBlock.Text = second;
    }
}