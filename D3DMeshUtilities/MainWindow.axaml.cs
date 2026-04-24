using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using TelltaleToolKit.Utility.Blowfish;
using HorizontalAlignment = Avalonia.Layout.HorizontalAlignment;
using SelectionChangedEventArgs = Avalonia.Controls.SelectionChangedEventArgs;
using TextBlock = Avalonia.Controls.TextBlock;
using TextBox = Avalonia.Controls.TextBox;
using RoutedEventArgs = Avalonia.Interactivity.RoutedEventArgs;

namespace D3DMeshUtilities;

public partial class MainWindow : BaseProjectWindow
{
    public static int? GameCache;
    public static string? SingleArchivePathCache;
    public static string? GameDataDirectoryCache;
    public static List<string>? LoadedArchivesCache;
    
    public MainWindow()
    {
// #if WINDOWS7_0_OR_GREATER
//         App.AllocConsole();
// #endif
        
        InitializeComponent();

        foreach (T3BlowfishKey key in Enum.GetValues<T3BlowfishKey>())
        {
            GameDropdown.Items.Add(key.GetGameName());
        }

        if (App.StartupGame != null)
            GameDropdown.SelectedIndex = (int)App.StartupGame;
        else if (GameCache.HasValue)
            GameDropdown.SelectedIndex = GameCache.Value;
        else
            GameDropdown.SelectedIndex = 79;

        if (SingleArchivePathCache != null)
        {
            filePath.Text = SingleArchivePathCache;
        }

        if (GameDataDirectoryCache != null)
        {
            GameDataPath.Text = GameDataDirectoryCache;
        }

        if (LoadedArchivesCache != null)
        {
            ArchiveList.Items.Clear();
            //assume the resource context is loaded
            foreach (string archivePath in LoadedArchivesCache)
            {
                var li = new Avalonia.Controls.ListBoxItem();
                var text = new TextBlock();

                int seperator = archivePath.LastIndexOfAny(['\\', '/']);

                string archiveName = archivePath.Substring(seperator + 1);

                text.Text = archiveName;
                text.HorizontalAlignment = HorizontalAlignment.Right;
                li.Content = text;

                ArchiveList.Items.Add(li);
            }
            
            ArchiveListGrid.IsVisible = true;
        }

        if (!startedUp)
        {
            startedUp = true;
        }

        if (string.IsNullOrWhiteSpace(App.StartupGameArchivesDirectory)) return;
        GameDataPath.Text = App.StartupGameArchivesDirectory;

        if (!App.StartupLoadGameDir || string.IsNullOrWhiteSpace(App.StartupGameArchivesDirectory)) return;
        Dispatcher.InvokeAsync(LoadResources);

        if (!App.StartupChooseArchive || string.IsNullOrWhiteSpace(App.StartupArchive)) return;
        LoadArchive(App.StartupArchive);
        
    }


    private void OpenFile_OnClick(object sender, RoutedEventArgs e)
    {
        Dispatcher.InvokeAsync(OpenFile);
    }

    private static FilePickerFileType TelltaleArchive2Files =
        new FilePickerFileType("TellTale Archive 2 Files (*.ttarch2) | *.ttarch2")
        {
            AppleUniformTypeIdentifiers =
                new[] { "public.item" }, //apple moment. choose whatever, will error if wrong lol
            Patterns = new[] { "*.ttarch2" }
        };

    private async void OpenFile()
    {
        
        IReadOnlyList<IStorageFile> dialouge = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = "Open Telltale Archive",
            FileTypeFilter = new []{ TelltaleArchive2Files },
            AllowMultiple = false
        });
        

        if (dialouge.Count <= 0) return;
        
        
        string? file = dialouge[0].TryGetLocalPath();
            
        if(file != null) 
            filePath.Text = file;
    }

    private void OpenFolder_OnClick(object sender, RoutedEventArgs e)
    {
        Dispatcher.InvokeAsync(OpenFolder);
    }

    private async void OpenFolder()
    {
        IReadOnlyList<IStorageFolder> dialouge = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            AllowMultiple = false,
            Title = "Select game directory"
        });

        if (dialouge.Count <= 0) return;
        
        string? folder = dialouge[0].TryGetLocalPath();
            
        if(!string.IsNullOrWhiteSpace(folder))
            GameDataPath.Text = folder;
    }

    private void OpenArchive_OnClick(object sender, RoutedEventArgs e)
    {
        if (!File.Exists(filePath.Text))
            return;

        ResourceLoader.Instance.LoadArchive(Dispatcher, filePath.Text, GameDropdown.Text!);

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


    private void LoadResources_OnClick(object sender, RoutedEventArgs e)
    {
        Dispatcher.InvokeAsync(LoadResources);
    }

    private async void LoadResources()
    {
        try
        {
            if (!Directory.Exists(GameDataPath.Text))
                return;

            ArchiveListGrid.IsVisible = true;

            //make sure the Loading... is shown before huge freeze
            await Task.Delay(100);

            var pathTask =
                ResourceLoader.Instance.LoadResourceContexts(Dispatcher, GameDataPath.Text, GameDropdown.Text!);
        
            await pathTask;

            pathTask.Result.Sort();
        
            //remove "Loading..."
            ArchiveList.Items.Clear();

            foreach (string archivePath in pathTask.Result)
            {
                var li = new Avalonia.Controls.ListBoxItem();
                var text = new TextBlock();

                int seperator = archivePath.LastIndexOfAny(['\\', '/']);

                string archiveName = archivePath.Substring(seperator + 1);

                text.Text = archiveName;
                text.HorizontalAlignment = HorizontalAlignment.Right;
                li.Content = text;

                ArchiveList.Items.Add(li);
            }
        }
        catch (Exception e)
        {
            Console.Out.WriteLine(e);
        }
    }

    private void ArchiveList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedItems = ArchiveList.SelectedItems;

        if (selectedItems == null || selectedItems.Count == 0)
        {
            OpenArchiveGrid.IsVisible = false;
        }
        else
        {
            OpenArchiveGrid.IsVisible = true;
        }
    }
    
    private void LoadArchive(string archive)
    {
        ResourceLoader.Instance.LoadArchive(Dispatcher, Path.Combine(GameDataPath.Text!, archive), GameDropdown.Text!);

        ArchiveModelList list = new ArchiveModelList();

        list.Show();

        this.Hide();

        SetMainWindow(list);

        this.Close();
    }

    private void LoadArchive(object sender, RoutedEventArgs e)
    {
        if (ArchiveList.SelectedItem is not ListBoxItem selectedItem)
            return;

        var archiveName = ((TextBlock)selectedItem.Content!)?.Text;

        ResourceLoader.Instance.LoadArchive(Dispatcher, Path.Combine(GameDataPath.Text!, archiveName), GameDropdown.Text!);

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
        return Window.LoadResources;
    }


    private void MainWindow_OnClosed(object? sender, EventArgs e)
    {
        if (GameDropdown.SelectedIndex != 79)
        {
            GameCache = GameDropdown.SelectedIndex;
        }

        if (!filePath.Text.Contains("Archive containing mesh to extract"))
        {
            SingleArchivePathCache = filePath.Text;
        }

        if (!GameDataPath.Text.Contains("Game Data Directory"))
        {
            GameDataDirectoryCache = GameDataPath.Text;
        }

        if (ArchiveList.Items.Count == 0)
            return;

        if (!(ArchiveList.Items[0] is ListBoxItem lvi && lvi.Content is TextBox box &&
              box.Text.Contains("Loading...")))
        {
            List<string> archives = new List<string>();
            foreach (object? item in ArchiveList.Items)
            {
                if(!(item is ListBoxItem listItem))
                    continue;
                
                string? archiveName = ((TextBlock)listItem.Content!).Text;

                archives.Add(Path.Combine(GameDataPath.Text, archiveName!));
            }

            LoadedArchivesCache = archives;
        }
    }
}