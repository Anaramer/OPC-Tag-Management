using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MyAvaloniaApp.ViewModels;
using System;

namespace MyAvaloniaApp.Views;

public partial class WinTagInfo : Window
{
    public OpcTag? Model = null;

    private TextBox? _txtId;
    private TextBox? _txtModule;
    private TextBox? _txtName;
    private ComboBox? _cmbDataType;
    private TextBox? _txtAliasName;
    private TextBox? _txtDescription;
    private DatePicker? _dpStartAt;
    private TimePicker? _tpStartAt;
    private CheckBox? _chkEveryDay;
    private TimePicker? _tpEveryDayAt;
    private NumericUpDown? _numCycle;


    public WinTagInfo()
    {
        InitializeComponent();

        _txtId = this.FindControl<TextBox>("TxtId");
        _txtModule = this.FindControl<TextBox>("TxtModule");
        _txtName = this.FindControl<TextBox>("TxtName");
        _cmbDataType = this.FindControl<ComboBox>("CmbDataType");
        _txtAliasName = this.FindControl<TextBox>("TxtAliasName");
        _txtDescription = this.FindControl<TextBox>("TxtDescription");
        _dpStartAt = this.FindControl<DatePicker>("DpStartAt");
        _tpStartAt = this.FindControl<TimePicker>("TpStartAt");
        _chkEveryDay = this.FindControl<CheckBox>("ChkEveryDay");
        _tpEveryDayAt = this.FindControl<TimePicker>("TpEveryDayAt");
        _numCycle = this.FindControl<NumericUpDown>("NumCycle");
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
        
        if (Model != null)
        {
            _txtId!.Text = Model.ID.ToString();
            _txtModule!.Text = Model.Module;
            _txtName!.Text = Model.TagName;
            _cmbDataType!.SelectedIndex = (int)Model.DataType;
            _txtAliasName!.Text = Model.SaveTagName;
            _txtDescription!.Text = Model.Description;
            _dpStartAt!.SelectedDate = new DateTimeOffset(new DateTime(Model.StartReadingDateTime.Year, Model.StartReadingDateTime.Month, Model.StartReadingDateTime.Day));
            _tpStartAt!.SelectedTime = Model.StartReadingDateTime.TimeOfDay;
            _chkEveryDay!.IsChecked = Model.RunEveryDayOnce;
            _tpEveryDayAt!.SelectedTime = Model.EveryDayAt.TimeOfDay;
            _numCycle!.Value = Model.ReadingCycle;
        }
        else
        {
            _dpStartAt!.SelectedDate = new DateTimeOffset(DateTime.Now);
            _tpStartAt!.SelectedTime = DateTime.Now.TimeOfDay;
            _tpEveryDayAt!.SelectedTime = DateTime.Now.TimeOfDay;
            _cmbDataType!.SelectedIndex = 0;
        }

        ChkEveryDay_OnIsCheckedChanged(_chkEveryDay, null!);
    }

    private bool Validation()
    {
        var txtModuleErr = this.FindControl<Label>("TxtModuleErr");
        txtModuleErr!.IsVisible = false;
        _txtModule!.Classes.Clear();

        var txtNameErr = this.FindControl<Label>("TxtNameErr");
        txtNameErr!.IsVisible = false;
        _txtName!.Classes.Clear();

        var txtAliasNameErr = this.FindControl<Label>("TxtAliasNameErr");
        txtAliasNameErr!.IsVisible = false;
        _txtAliasName!.Classes.Clear();

        var tpEveryDayAtErr = this.FindControl<Label>("TpEveryDayAtErr");
        tpEveryDayAtErr!.IsVisible = false;

        var numCycleErr = this.FindControl<Label>("NumCycleErr");
        numCycleErr!.IsVisible = false;

        if (_txtModule == null || _txtModule!.Text == null || _txtModule!.Text!.Length < 1)
        {
            txtModuleErr!.IsVisible = true;
            _txtModule!.Focus();
            _txtModule!.Classes.Add("Red");
            return false;
        }

        if (_txtName == null || _txtName!.Text == null || _txtName!.Text!.Length < 3)
        {
            txtNameErr!.IsVisible = true;
            _txtName!.Focus();
            _txtName!.Classes.Add("Red");
            return false;
        }

        if (_txtAliasName == null || _txtAliasName!.Text == null || _txtAliasName!.Text!.Length < 3)
        {
            txtAliasNameErr!.IsVisible = true;
            _txtAliasName!.Focus();
            _txtAliasName!.Classes.Add("Red");
            return false;
        }

        if (_chkEveryDay!.IsChecked == false && (_numCycle == null || _numCycle!.Value == null || _numCycle!.Value < 100))
        {
            numCycleErr!.IsVisible = true;
            _numCycle!.Focus();
            return false;
        }

        return true;
    }

    private void ChkEveryDay_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        _chkEveryDay = (CheckBox)sender!;
        _tpEveryDayAt!.IsEnabled = _chkEveryDay.IsChecked == true;
        _numCycle!.IsEnabled = _chkEveryDay.IsChecked == false;
    }

    private void BtnSave_OnClick(object? sender, RoutedEventArgs e)
    {
        if(Validation())
        {
            Model ??= new OpcTag();

            Model.Module = _txtModule?.Text ?? string.Empty;
            Model.TagName = _txtName?.Text ?? string.Empty;
            Model.SaveTagName = _txtAliasName?.Text ?? string.Empty;
            Model.Description = _txtDescription?.Text ?? string.Empty;
            Model.DataType = (DataType)(_cmbDataType?.SelectedItem ?? DataType.Int32);
            Model.RunEveryDayOnce = _chkEveryDay?.IsChecked == true;
            Model.EveryDayAt = (DateTime)(DateTime.Now.Date + _tpEveryDayAt.SelectedTime);
            Model.ReadingCycle = (int)(_chkEveryDay!.IsChecked == true ? 0 : _numCycle?.Value ?? 1000);
            Model.StartReadingDateTime = (DateTime)(_dpStartAt?.SelectedDate?.DateTime + _tpStartAt.SelectedTime);

            Close(true);
        }
    }

    private void BtnCancel_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}