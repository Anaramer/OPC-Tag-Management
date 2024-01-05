using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using OpcLabs.EasyOpc;
using OpcLabs.EasyOpc.DataAccess;
using OpcLabs.EasyOpc.OperationModel;

namespace MyAvaloniaApp.Views;

public partial class BrowseServer : Window
{
    public string ServerAddress { get; protected set; }
    public string Host { get; protected set; }

    private readonly TextBox? _txtCustomAddress;
    private readonly TextBox? _txtHost;
    private readonly ListBox? _lstServerList;
    private readonly Label? _lblLoading;
    private readonly RadioButton? _rdBtnServerList;
    private readonly RadioButton? _rdBtnCustomAddress;

    public BrowseServer(string? customAddress = "", string? host = "")
    {
        InitializeComponent();

        _txtCustomAddress = this.FindControl<TextBox>("TxtCustomAddress");
        _txtHost = this.FindControl<TextBox>("TxtHost");

        _lstServerList = this.FindControl<ListBox>("LstServerList");
        _lstServerList!.Items.Clear();

        _lblLoading = this.FindControl<Label>("LblLoading");

        _rdBtnServerList = this.FindControl<RadioButton>("RdBtnList");
        _rdBtnCustomAddress = this.FindControl<RadioButton>("RdBtnCustom");

        if (!string.IsNullOrEmpty(customAddress))
        {
            _rdBtnCustomAddress!.IsChecked = true;
            _txtCustomAddress!.Text = customAddress;
        }

        if (!string.IsNullOrEmpty(host))
        {
            _txtHost!.Text = host;
        }

    }

    public BrowseServer()
    {
        
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void BtnSelect_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_rdBtnServerList?.IsChecked == true)
        {
            if(_lstServerList?.Items.Count == 0 ) return;
            var result = _lstServerList?.SelectedItem as string;
            ServerAddress = result!;
            Host = _txtHost!.Text!;
            Close(true);
        }
        else
        {
            if (!(_txtCustomAddress?.Text?.Length > 0)) return;
            ServerAddress = _txtCustomAddress!.Text!;
            Host = _txtHost!.Text!;
            Close(true);
        }
        
    }

    private void BtnCancel_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private void RdBtnList_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        var rdBtn = (RadioButton)sender!;
        if (rdBtn.IsChecked == true)
        {
            _txtCustomAddress!.IsEnabled = false;
            _lstServerList!.IsEnabled = true;
        }
        else
        {
            _txtCustomAddress!.IsEnabled = true;
            _lstServerList!.IsEnabled = false;
        }
    }

    private void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
        SearchServers();
    }

    private void SearchServers()
    {
        _lblLoading!.Content = "Loading...";
        _lblLoading!.Classes.Clear();
        _lblLoading!.Classes.Add("Orange");
        _lstServerList!.Items.Clear();
        this.UpdateLayout();

        var client = new EasyDAClient();
        client.QueueCallbacks = false;
        ServerElementCollection serverElements;
        try
        {
            serverElements = client.BrowseServers(Host ?? _txtHost!.Text ?? "");
        }
        catch (OpcException exp)
        {
            _lblLoading!.Content = "Failure Loading !";
            _lblLoading!.Classes.Clear();
            _lblLoading!.Classes.Add("Red");
            _rdBtnCustomAddress!.IsChecked = true;
            return;
        }

        if (serverElements.Count == 0)
        {
            _lblLoading!.Content = "Not Found!";
            _rdBtnCustomAddress!.IsChecked = true;
        }

        foreach (var serverElement in serverElements)
        {
            if (serverElement != null)
            {
                _lstServerList!.Items.Add(serverElement.ProgId);
            }
        }

        _lblLoading!.IsVisible = false;
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        SearchServers();
        _rdBtnServerList!.IsChecked = true;
    }
}