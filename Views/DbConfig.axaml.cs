using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MyAvaloniaApp.ViewModels;

namespace MyAvaloniaApp.Views;

public partial class DbConfig : Window
{
    public string? DbUserName { get; protected set; }
    public string? DbPassword { get; protected set; }
    public string? DbDataSource { get; protected set; }

    private readonly TextBox? _txtusername;
    private readonly TextBox? _txtpassword;
    private readonly TextBox? _txtdatasource;

    private readonly Label? _txtusernameErr;
    private readonly Label? _txtpasswordErr;
    private readonly Label? _txtdatasourceErr;

    private readonly Label? _btnTestErr;

    public DataViewModel _ViewModel;

    public DbConfig(string dbUserName, string dbPassword, string dbDataSource,DataViewModel viewModel)
    {
        InitializeComponent();
        _txtusername = this.FindControl<TextBox>("TxtUsername");
        _txtpassword = this.FindControl<TextBox>("TxtPassword");
        _txtdatasource = this.FindControl<TextBox>("TxtDataSource");

        _txtusernameErr = this.FindControl<Label>("TxtUsernameErr");
        _txtpasswordErr = this.FindControl<Label>("TxtPasswordErr");
        _txtdatasourceErr = this.FindControl<Label>("TxtDataSourceErr");

        _btnTestErr = this.FindControl<Label>("BtnTestErr");

        _txtusername!.Text = dbUserName;
        _txtpassword!.Text = dbPassword;
        _txtdatasource!.Text = dbDataSource;

        _ViewModel = viewModel;

        _txtusernameErr!.IsVisible = _txtpasswordErr!.IsVisible = _txtdatasourceErr!.IsVisible = false;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }


    private void BtnTest_OnClick(object? sender, RoutedEventArgs e)
    {
        if(!Validation()) return;
        _btnTestErr!.IsVisible = true;
        if (_ViewModel!.TestDbConnection(_txtusername!.Text!, _txtpassword!.Text!, _txtdatasource!.Text!))
        {
            _btnTestErr!.Classes.Clear();
            _btnTestErr!.Classes.Add("Green");
            _btnTestErr!.Content = "Success  :)";
        }
        else
        {
            _btnTestErr!.Classes.Clear();
            _btnTestErr!.Classes.Add("Red");
            _btnTestErr!.Content = "Failed  :(";
        }

    }

    private bool Validation()
    {
        _txtusernameErr!.IsVisible = _txtpasswordErr!.IsVisible = _txtdatasourceErr!.IsVisible = false;

        if (_txtusername == null || string.IsNullOrEmpty(_txtusername.Text))
        {
            _txtusernameErr!.IsVisible = true;
            _txtusername!.Classes.Clear();
            _txtusername!.Classes.Add("Red");
            _txtusername!.Focus();
            return false;
        }

        if (_txtpassword == null || string.IsNullOrEmpty(_txtpassword.Text))
        {
            _txtpasswordErr!.IsVisible = true;
            _txtpassword!.Classes.Clear();
            _txtpassword!.Classes.Add("Red");
            _txtpassword!.Focus();
            return false;
        }

        if (_txtdatasource == null || string.IsNullOrEmpty(_txtdatasource.Text))
        {
            _txtdatasourceErr!.IsVisible = true;
            _txtdatasource!.Classes.Clear();
            _txtdatasource!.Classes.Add("Red");
            _txtdatasource!.Focus();
            return false;
        }

        DbUserName = _txtusername.Text;
        DbPassword = _txtpassword.Text;
        DbDataSource = _txtdatasource.Text;

        return true;
    }

    private void TxtUsername_OnTextChanging(object? sender, TextChangingEventArgs e)
    {
        _txtusernameErr!.IsVisible = false;
        _btnTestErr!.IsVisible = false;
        _txtusername!.Classes.Clear();
    }

    private void TxtPassword_OnTextChanging(object? sender, TextChangingEventArgs e)
    {
        _txtpasswordErr!.IsVisible = false;
        _btnTestErr!.IsVisible = false;
        _txtpassword!.Classes.Clear();
    }

    private void TxtDataSource_OnTextChanging(object? sender, TextChangingEventArgs e)
    {
        _txtdatasourceErr!.IsVisible = false;
        _btnTestErr!.IsVisible = false;
        _txtdatasource!.Classes.Clear();
    }

    private void BtnSave_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!Validation()) return;
        Close(true);
    }

    private void BtnCancel_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}