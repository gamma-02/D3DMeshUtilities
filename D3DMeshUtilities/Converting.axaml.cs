using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
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
        
        if (_modelsToConvert.Count == 0)
        {
            MessageBox.IsVisible = true;
            Console.Out.WriteLine("No models provided!");

            AddMessageToBox("No models provided!");
        }
        
        if (string.IsNullOrWhiteSpace(App.StartupOutputDir)) return;
        FilePath.Text = App.StartupOutputDir;

        if (!App.StartupConvertModels) return;
        
        Convert_OnClick(null, new RoutedEventArgs());
        
        
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
    }
    

    private void Convert_OnClick(object? sender, RoutedEventArgs e)
    {
        MessageBox.IsVisible = true;

        if (_modelsToConvert.Count == 0)
        {
            Console.Out.WriteLine("No models provided!");

            AddMessageToBox("No models provided!");
        }
        
        if (string.IsNullOrWhiteSpace(FilePath.Text))
            return;

        if (!Directory.Exists(FilePath.Text))
        {
            Console.Out.WriteLine($"Directory {FilePath.Text} not found!");
            
            AddMessageToBox("Directory not found!");

            return;
        }
        
        D3DMeshManager manager = new D3DMeshManager(_modelsToConvert, FilePath.Text);

        SetImportantControlsEnabled( false);

        Task.Run(async () =>
        {
            Dispatcher.Invoke(() => AddMessageToBox("Converting..."));
            await Task.Delay(100);
            
            manager.LoadMeshes(this)?
                .GetAwaiter().OnCompleted(() => Dispatcher.Invoke(CompleteMeshLoad)); //lol

            Console.Out.WriteLine("Read mesh data from ttarchive!");

            //todo: some kind of a system to automatically hide the box if there's been no more messages for a while

        });
    }

    private void CompleteMeshLoad()
    {
        AddMessageToBox("Done!");

        SetImportantControlsEnabled(true);

        if (App.QuitAfterConvert)
        {
            Console.WriteLine("Automatically Exiting!");
            Dispatcher.Invoke(() => Environment.Exit(1));
        }
    }

    private void SetImportantControlsEnabled(bool enabled)
    {
        ConvertButton.IsEnabled = enabled;
        Header.IsEnabled = enabled;
        OutputFolderButton.IsEnabled = enabled;
        ConvertButton.IsEnabled = enabled;
        Back.IsEnabled = enabled;

    }

    public void AddMessageToBox(string message)
    {
        TextBlock messageBlock = new TextBlock();
        messageBlock.Text = message;
        messageBlock.FontSize = 16;
        messageBlock.HorizontalAlignment = HorizontalAlignment.Left;
        messageBlock.Margin = new Thickness(5, 5, 5, 5);

        ListBoxItem item = new();
        item.Content = messageBlock;

        MessageList.Items.Add(messageBlock);

        MessageList.SelectedIndex = MessageList.Items.Count - 1;
    }

    public static void AddMessageToBox(string message, Converting converting)
    {
        converting.AddMessageToBox(message);
    }

    private void Back_OnClick(object sender, RoutedEventArgs e)
    {
        ArchiveModelList list = new ArchiveModelList(false) { OverriddenOwner = this };

        CloseOnNewWindowOpened(list);

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