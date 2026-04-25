using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using D3DMeshUtilities.Code;

namespace D3DMeshUtilities;

public partial class Converting : BaseProjectWindow
{
    public List<string> ModelsToConvert
    {
        get => _modelsToConvert;

        set => _modelsToConvert = value;
    }

    private List<string> _modelsToConvert;
    
    public Converting(List<string> modelsToConvert)
    {
        _modelsToConvert = modelsToConvert;
        InitializeComponent();
        
        if (string.IsNullOrWhiteSpace(App.StartupOutputDir)) return;
        FilePath.Text = App.StartupOutputDir;

        if (!App.StartupConvertModels) return;
        
        D3DMeshManager manager = new D3DMeshManager(modelsToConvert, App.StartupOutputDir);

        Dispatcher.InvokeAsync(manager.LoadMeshes);

        // Console.Out.WriteLine("Read mesh data from ttarchive!");
        
    }

    public Converting()
    {
        _modelsToConvert = new List<string>();
        InitializeComponent();

        ConvertButton.IsEnabled = false;
    }

    public void SetModelsToConvert(List<string> models)
    {
        _modelsToConvert = models;

        ConvertButton.IsEnabled = true;
    }
    
    private async void OpenFile()
    {
        IReadOnlyList<IStorageFolder> dialouge = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            AllowMultiple = false,
            Title = "Select Output Directory"
        });

        if (dialouge.Count <= 0) return;
        
        string? folder = dialouge[0].TryGetLocalPath();
            
        if(!string.IsNullOrWhiteSpace(folder))
            FilePath.Text = folder;

        // bool? result = dialouge.ShowDialog();

        // if (dialouge.Count > 0)
        // {
        //     IStorageFolder file = dialouge[0];
        //
        //     FilePath.Text = file.TryGetLocalPath();
        //
        // }
    }
    

    private void Convert_OnClick(object? sender, RoutedEventArgs e)
    {

        if (_modelsToConvert.Count == 0)
        {
            Console.Out.WriteLine("No models provided!");
        }

        if (string.IsNullOrWhiteSpace(FilePath.Text))
            return;
        
        D3DMeshManager manager = new D3DMeshManager(_modelsToConvert, FilePath.Text);

        manager.LoadMeshes();

        Console.Out.WriteLine("Read mesh data from ttarchive!");
    }

    private void Back_OnClick(object sender, RoutedEventArgs e)
    {
        ArchiveModelList list = new ArchiveModelList()
        {
            OverriddenOwner = this
        };

        list.Show();

        this.Hide();

        SetMainWindow(list);

        list.OverriddenOwner = null;

        this.Close();
    }
    
    public override Window GetWindow()
    {
        return Window.ConvertModels;
    }

    private void OutFile_OnClick(object? sender, RoutedEventArgs e)
    {
        Dispatcher.InvokeAsync(OpenFile);
    }
}