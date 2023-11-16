using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using MyAvaloniaApp.DB;
using MyAvaloniaApp.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyAvaloniaApp.Views;

public partial class Main : Window
{
    public DataViewModel _viewModel = new DataViewModel();

    private readonly DataGrid? _grid;
    private readonly ListBox? _listBox;
    private readonly TextBox? _txtServerAddress;
    private readonly ListBox? _listLiveTag;
    private readonly Label? _labelClock;
    private readonly Ellipse? _liveLight;
    private readonly Ellipse? _dbLiveLight;

    private readonly OracleDb _oracleDB;

    public Main()
    {
        InitializeComponent();
        DataContext = _viewModel;

        _grid = this.FindControl<DataGrid>("DataGridTags");
        _listBox = this.FindControl<ListBox>("ListBoxConsole");
        _txtServerAddress = this.FindControl<TextBox>("TxtServerAddress");
        _listLiveTag = this.FindControl<ListBox>("ListLiveTag");
        _labelClock = this.FindControl<Label>("LblClock");
        _liveLight = this.FindControl<Ellipse>("LiveLight");
        _dbLiveLight = this.FindControl<Ellipse>("DBLiveLight");

        _viewModel.ListBoxConsole = _listBox;
        _viewModel.DataGridTags = _grid;
        _grid!.ItemsSource = _viewModel.OpcTags;

        _oracleDB = _viewModel.OracleDb;

        _txtServerAddress!.Text = $@"{_viewModel!.OpcHost}\{_viewModel!.OpcServer}";
    }

    // Update Click Label every 1 sec with datetime.now
    private async Task Clock()
    {
        while (true)
        {
            _labelClock!.Content = DateTime.Now.ToLongTimeString();
            _liveLight!.Fill = Brushes.Red;
            _dbLiveLight!.Fill = _viewModel.DbConnect ? Brushes.GreenYellow : Brushes.Red;
            await Task.Delay(1000);
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
        LoadDataGridItem(0);
        if (_viewModel!.AutoStart)
        {
            BtnStartReading_OnClick(null, null);
        }

        _ = Clock();
    }

    private void BtnAddTag_OnClick(object? sender, RoutedEventArgs e)
    {
        var winForm = new WinTagInfo
        {
            Model = null,
            DataContext = _viewModel
        };
        _ = ShowTagInfoWindows(winForm);
    }

    private void BtnEditTag_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel.OpcTags.Count == 0) return;
        if (_grid!.SelectedItem == null) return;
        var selectedTag = (OpcTag)_grid.SelectedItem;
        var winForm = new WinTagInfo
        {
            Model = selectedTag,
            DataContext = _viewModel
        };
        _ = ShowTagInfoWindows(winForm);
    }

    private async Task ShowTagInfoWindows(WinTagInfo winForm)
    {
        var dialogResult = await winForm.ShowDialog<bool>(this);
        if (dialogResult == false) return;
        var result = winForm.Model;
        if (result == null) return;
        if (result.ID == 0)
        {
            //New Tag
            _viewModel.SaveOpcTag(result);
            LoadDataGridItem(-1);
        }
        else
        {
            //Update Tag
            var index = _viewModel.OpcTags.IndexOf(_viewModel.OpcTags.FirstOrDefault(p => p.ID == result.ID)!);
            if (index == -1) return;
            _viewModel.SaveOpcTag(result);
            LoadDataGridItem(index);
        }
    }

    private void LoadDataGridItem(int selectedIndex)
    {
        if (!_viewModel.DbConnect) return;
        var listCount = _viewModel!.OpcTags.Count;
        _grid!.ItemsSource = null;
        _viewModel.LoadTags();
        _grid!.ItemsSource = _viewModel.OpcTags;
        if (selectedIndex < 0)
        {
            selectedIndex = _viewModel!.OpcTags.Count - 1;
        }
        if (_viewModel!.OpcTags.Count != listCount) _listLiveTag!.Items.Clear();
        if (_viewModel!.OpcTags.Count == 0) return;
        _grid!.SelectedIndex = selectedIndex;
        _grid!.ScrollIntoView(_viewModel.OpcTags[selectedIndex], _grid!.Columns[0]);
    }

    private void BtnDeleteTag_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_viewModel.OpcTags.Count == 0) return;
        if (_grid!.SelectedItem == null) return;
        _ = ShowMessageDialog((OpcTag)_grid!.SelectedItem);
    }

    private async Task ShowMessageDialog(OpcTag? tag)
    {
        if (tag == null) return;
        var msgDialog = new MessageDialog();
        var dialogResult = await msgDialog.ShowDialog<bool>(this);
        if (dialogResult == true)
        {
            _viewModel.RemoveTag(tag);
            LoadDataGridItem(0);
        }
    }

    private void TxtSearchTag_OnTextChanging(object? sender, TextChangingEventArgs e)
    {
        if (!_viewModel.DbConnect) return;
        var txtSearchTag = (TextBox)sender!;
        var index = _viewModel.OpcTags.IndexOf(
            _viewModel.OpcTags.FirstOrDefault(
                p => String.Equals(p.TagName, txtSearchTag.Text!, StringComparison.CurrentCultureIgnoreCase)
                     || String.Equals(p.SaveTagName, txtSearchTag.Text!, StringComparison.CurrentCultureIgnoreCase))!);
        if (index == -1) return;
        _grid!.SelectedIndex = index;
    }

    private void BtnBrowseServer_OnClick(object? sender, RoutedEventArgs e)
    {
        _ = ShowServerBrowserDialog();
    }

    private async Task ShowServerBrowserDialog()
    {
        var browseServer = new BrowseServer(_viewModel!.OpcServer, _viewModel!.OpcHost);
        var dialogResult = await browseServer.ShowDialog<bool>(this);
        if (dialogResult)
        {
            _txtServerAddress!.Text = $@"{browseServer.Host}\{browseServer.ServerAddress}";
            _viewModel!.OpcServer = browseServer.ServerAddress;
            _viewModel!.OpcHost = browseServer.Host;
        }
    }

    private void BtnStartReading_OnClick(object? sender, RoutedEventArgs? e)
    {
        _viewModel!.StartReadingValues();
        _ = LiveValueBillboard();
    }

    private void BtnStopReading_OnClick(object? sender, RoutedEventArgs e)
    {
        _viewModel!.StopReadingValues();
        _listLiveTag!.Items.Clear();
    }

    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        _viewModel!.StopReadingValues();
        _listLiveTag!.Items.Clear();
    }

    private async Task LiveValueBillboard()
    {
        await Task.Delay(2500);

        try
        {
            while (_viewModel.CycleReadingState)
            {
                var isInserted = false;
                var dateTimeNow = DateTime.Now;

                foreach (var opcTag in _viewModel.OpcTags)
                {
                    var index = _viewModel.OpcTags.IndexOf(_viewModel.OpcTags.FirstOrDefault(p => p.ID == opcTag.ID)!);
                    if (index > _listLiveTag!.Items.Count - 1)
                        _listLiveTag!.Items.Add("");
                    _listLiveTag!.Items[index] = $"[{opcTag.ID}] {opcTag.TagName} : {(string.IsNullOrEmpty(opcTag.LastValue) ? "Null" : opcTag.LastValue)}";
                    _liveLight!.Fill = Brushes.GreenYellow;

                    //----

                    if (dateTimeNow >= opcTag.StartReadingDateTime)
                    {
                        if (opcTag.RunEveryDayOnce)
                        {
                            if (opcTag.LastReadingDateTime.Date < dateTimeNow.Date
                                && opcTag.EveryDayAt.Hour <= dateTimeNow.Hour
                                && opcTag.EveryDayAt.Minute <= dateTimeNow.Minute)
                            {
                                if (string.IsNullOrEmpty(opcTag.LastValue))
                                {
                                    var text = $"[{opcTag.ID}] {opcTag.TagName} => Error : Value Is Null Or Empty!";
                                    _viewModel.AddConsole(text);
                                    var logtext = $"{_viewModel!.OpcServer},{opcTag.ID},{opcTag.TagName},Error,Value Is Null Or Empty";
                                    _viewModel!.OracleDb.InsertLog(logtext);
                                    continue;
                                }

                                opcTag.LastReadingDateTime = dateTimeNow;
                                _oracleDB.InsertValue(opcTag, opcTag.LastValue);
                                _viewModel.AddConsole(opcTag);
                                isInserted = true;
                            }
                        }
                        else
                        {
                            if (opcTag.LastReadingDateTime.AddMilliseconds(opcTag.ReadingCycle) <= dateTimeNow)
                            {
                                if (string.IsNullOrEmpty(opcTag.LastValue))
                                {
                                    var text = $"[{opcTag.ID}] {opcTag.TagName} => Error : Value Is Null Or Empty!";
                                    _viewModel.AddConsole(text);
                                    var logtext = $"{_viewModel!.OpcServer},{opcTag.ID},{opcTag.TagName},Error,Value Is Null Or Empty";
                                    _viewModel!.OracleDb.InsertLog(logtext);
                                    continue;
                                }

                                opcTag.LastReadingDateTime = dateTimeNow;
                                _oracleDB.InsertValue(opcTag, opcTag.LastValue);
                                _viewModel.AddConsole(opcTag);
                                isInserted = true;

                            }
                        }
                    }
                }

                if (isInserted) LoadDataGridItem(_grid!.SelectedIndex);

                await Task.Delay(1000);
            }
        }
        catch (Exception e)
        {
            _viewModel.CycleReadingState = false;
            Console.WriteLine(e);
            throw;
        }
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        _ = ShowDbConfigDialog();
    }

    private async Task ShowDbConfigDialog()
    {
        var dbConfig = new DbConfig(_oracleDB.OracleDbUser, _oracleDB.OracleDbPassword, _oracleDB.OracleDbDataSource, _viewModel);
        var dialogResult = await dbConfig.ShowDialog<bool>(this);
        if (dialogResult)
        {
            _oracleDB.OracleDbDataSource = dbConfig.DbDataSource!;
            _oracleDB.OracleDbUser = dbConfig.DbUserName!;
            _oracleDB.OracleDbPassword = dbConfig.DbPassword!;
            _viewModel.SaveSetting();
            _viewModel.ReconnectDb();

        }
    }
}