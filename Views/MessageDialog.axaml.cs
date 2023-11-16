using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace MyAvaloniaApp.Views;

public partial class MessageDialog : Window
{
    public MessageDialog()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void BtnYes_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(true);
    }

    private void BtnNo_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}