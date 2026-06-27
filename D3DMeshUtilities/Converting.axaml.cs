using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Platform.Storage;
using D3DMeshUtilities.Code;

namespace D3DMeshUtilities;

public partial class Converting : BaseProjectWindow
{
    
    public ConversionTask? ModelsToConvert
    {
        get => _task;

        set => _task = value;
    }

    private ConversionTask? _task;
    
    public Converting(ConversionTask task)
    {
        _task = task;
        InitializeComponent();

        ConvertButton.IsEnabled = _task.ValidateTask(this);
        
        if (string.IsNullOrWhiteSpace(App.StartupOutputDir)) return;
        FilePath.Text = App.StartupOutputDir;

        if (!App.StartupConvertModels) return;
        
        Convert_OnClick(null, new RoutedEventArgs());
    }

    public Converting()
    {
        // _task = new List<string>();
        InitializeComponent();

        ConvertButton.IsEnabled = false;
    }

    public void SetTask(ConversionTask task)
    {
        _task = task;

        ConvertButton.IsEnabled = task.ValidateTask(this);
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

        if (!(_task ?? new NoneConversionTask()).ValidateTask(this))
            return;
        
        if (string.IsNullOrWhiteSpace(FilePath.Text))
            return;

        string fullPath = Path.GetFullPath(FilePath.Text);

        if (!Directory.Exists(fullPath))
        {
            Console.Out.WriteLine($"Directory {FilePath.Text} not found!");
            
            AddMessageToBox("Directory not found!");

            return;
        }

        SetImportantControlsEnabled( false);

        Task.Run(() => _task.Convert(fullPath, this, CompleteMeshLoad));
    }

    private void CompleteMeshLoad()
    {
        
        AddMessageToBox("Done!");

        SetImportantControlsEnabled(true);

        if (App.DumpOnConversionComplete)
        {
            App.DumpProfiler();
            
        }

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

    public abstract class ConversionTask
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="converting"></param>
        /// <returns></returns>
        public abstract bool ValidateTask(Converting? converting);

        /// <summary>
        /// This should be an async function that executes the conversion task given an input file path and the instance of the task
        /// </summary>
        /// <param name="filePath"> Folder the results of this task should be placed into </param>
        /// <param name="converting"> The instance of the conversion window that the task is being ran in </param>
        /// <param name="completeTaskAction"> Action to be ran after the conversion task finishes </param>
        public abstract void Convert(string filePath, Converting? converting, Action completeTaskAction);
    }

    public class NoneConversionTask : ConversionTask
    {
        public override bool ValidateTask(Converting? converting) => false;

        public override void Convert(string filePath, Converting? converting, Action completeTaskAction) { }
    }
}