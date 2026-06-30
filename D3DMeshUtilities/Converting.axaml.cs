using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using D3DMeshUtilities.Code;

namespace D3DMeshUtilities;



public partial class Converting : BaseProjectWindow
{
    public class ConvertingState : ITabState
    {
        public ConversionTask? Task = null; //only non-null before the task is complete, in this case only non-null before the task is started
        
        public List<TextBlock> MessageBoxCache = [];
    }
    
    public ConversionTask? ModelsToConvert
    {
        get => _task;

        set => _task = value;
    }

    private ConversionTask? _task;
    private ConvertingState _state;
    
    public Converting(ConversionTask task) : this(task, null) {}
    
    public Converting(ConversionTask task, string? outputPath)
    {
        _task = task;
        
        TabsState.SelectedTab = 2;
        
        InitializeComponent();

        ConvertButton.IsEnabled = _task.ValidateTask(this);
        
        _state = TabsState.GetOrSetStateForWindow(Window.ConvertModels, new ConvertingState());

        if (_state.MessageBoxCache.Count != 0)
        {
            MessageList.Items.Clear();
            foreach (TextBlock child in _state.MessageBoxCache)
                MessageList.Items.Add(child);
        }
        
        if(_task.WaitingMessage != null)
        {
            AddMessageToBox(_task.WaitingString, _task.WaitingMessage);
        }
        else
        {
            AddMessageToBox(_task.WaitingString, Color.FromUInt32(0x62a36d));
        }

        if(!string.IsNullOrWhiteSpace(outputPath))
        {
            FilePath.Text = Path.GetFullPath(outputPath);
            // ConvertButton.IsEnabled = ConvertButton.IsEnabled && Directory.Exists(outputPath);
        }
        
        if (string.IsNullOrWhiteSpace(App.StartupOutputDir)) return;
        FilePath.Text = App.StartupOutputDir;

        if (!App.StartupConvertModels) return;
        
        Convert_OnClick(null, new RoutedEventArgs());
    }

    public Converting()
    {
        TabsState.SelectedTab = 2;

        InitializeComponent();
        
        ConvertButton.IsEnabled = false;

        _state = TabsState.GetOrSetStateForWindow(Window.ConvertModels, new ConvertingState());

        if (_state.Task != null)
        {
            _task = _state.Task;
        }

        if (_state.MessageBoxCache.Count != 0)
        {
            MessageList.Items.Clear();
            foreach (TextBlock child in _state.MessageBoxCache)
            {
                MessageList.Items.Add(child);
            }
        }

        if (_task is null or NoneConversionTask)
        {
            var messageBlock = new TextBlock
            {
                Text = "No task given...",
                FontSize = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(5, 5, 5, 5)
            };

            ListBoxItem i = new ListBoxItem() { Content = messageBlock };
            MessageList.Items.Add(i);
        }
    }
    
    private void Converting_OnClosed(object? sender, EventArgs e)
    {
        if (_task is not null and not NoneConversionTask)
        {
            _state.Task = _task;
        }

        foreach (object? child in MessageList.Items)
        {
            if (child is not TextBlock block) continue;

            var messageBlock = new TextBlock
            {
                Text = block.Text,
                FontSize = block.FontSize,
                HorizontalAlignment = block.HorizontalAlignment,
                Foreground = block.Foreground,
                Margin = block.Margin
            };
            
            _state.MessageBoxCache.Add(messageBlock);
        }
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

        ConversionTask task = _task ?? new NoneConversionTask();
        if (!task.ValidateTask(this))
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

        Task.Run(() => task.Convert(fullPath, this, CompleteConversionTask));
    }

    private void CompleteConversionTask()
    {
        ConversionTask task = _task ?? new NoneConversionTask();
        if(task.CompletedMessage != null)
        {
            AddMessageToBox(task.CompletedString, task.CompletedMessage);
        }
        else
        {
            AddMessageToBox(task.CompletedString, Color.FromUInt32(0x62a36d));
        }
        

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

        // _task = null;
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
        var messageBlock = new TextBlock
        {
            Text = message,
            FontSize = 16,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(5, 5, 5, 5)
        };

        MessageList.Items.Add(messageBlock);

        MessageList.SelectedIndex = MessageList.Items.Count - 1;
    }
    
    public void AddMessageToBox(string message, Color textColor)
    {
        textColor = new Color(255, textColor.R, textColor.G, textColor.B);
        var messageBlock = new TextBlock
        {
            Text = message,
            FontSize = 16,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(5, 5, 5, 5),
            Foreground = new SolidColorBrush(textColor)
        };

        MessageList.Items.Add(messageBlock);

        MessageList.SelectedIndex = MessageList.Items.Count - 1;
    }
    
    public void AddMessageToBox(string message, TextBlock messageBlock)
    {
        messageBlock = new TextBlock
        {
            Text = message,
            FontSize = 16,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(5, 5, 5, 5),
            Foreground = messageBlock.Foreground
        };

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
        ///  Validate this instance of the ConversionTask. If this returns true, then the task is valid and can begin.
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
        
        public abstract string WaitingString { get; }
        public virtual TextBlock? WaitingMessage { get; } = null; //if this returns something, expect Text to be set to the associated string
        public abstract string CompletedString { get; }
        public virtual TextBlock? CompletedMessage { get; } = null;
    }

    public class NoneConversionTask : ConversionTask
    {
        public override bool ValidateTask(Converting? converting) => false;

        public override void Convert(string filePath, Converting? converting, Action completeTaskAction) { }
        
        public override string WaitingString { get; } = "No task given...";
        public override string CompletedString { get; } = "...how...";

        public override TextBlock? CompletedMessage { get; } = new() { Foreground = new SolidColorBrush(Color.Parse("#bb5555")) };
    }

    
}