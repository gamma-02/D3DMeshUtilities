using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ICSharpCode.SharpZipLib.Core;
using TelltaleToolKit;
using TelltaleToolKit.IO.Archives;
using Tmds.DBus.Protocol;
using HorizontalAlignment = Avalonia.Layout.HorizontalAlignment;
using SelectionChangedEventArgs = Avalonia.Controls.SelectionChangedEventArgs;
using TextBlock = Avalonia.Controls.TextBlock;
using RoutedEventArgs = Avalonia.Interactivity.RoutedEventArgs;

namespace D3DMeshUtilities;

public class MainWindowTabState
{
    public static int? GameCache;
    public static string? SingleArchivePathCache;
    public static string? GameDataDirectoryCache;
    public static List<string>? LoadedArchivesCache;

    public static List<TextBlock>? MessageBoxCache;

}

public partial class MainWindow : BaseProjectWindow
{
    private static readonly Dictionary<string, int> EXTRA_GAME_YEARS = new Dictionary<string, int>()
    {
        { "The Walking Dead", 2012 }
    };
    
    public static int? GameCache;
    public static string? SingleArchivePathCache;
    public static string? GameDataDirectoryCache;
    public static List<string>? LoadedArchivesCache;
    
    public MainWindow()
    {
#if BUILT_FOR_WINDOWS
        App.AllocConsole();
#endif
        
        InitializeComponent();
        
        if (Design.IsDesignMode)
        {
            startedUp = true;
            return;
        }

        List<string> gameProfileKeys = Toolkit.Instance.GameProfiles.Keys.ToList();
        
        int _getYear(string gameName)
        {
            if (EXTRA_GAME_YEARS.ContainsKey(gameName))
            {
                return EXTRA_GAME_YEARS[gameName];
            }
            
            return int.Parse(Regex.Match(Toolkit.Instance.GameProfiles[gameName].Id, "20[0-9]{2}").Value);
        }
        
        gameProfileKeys.Sort((s, s1) => _getYear(s).CompareTo(_getYear(s1)));
        
        foreach (string gameProfileName in gameProfileKeys)
        {
            GameDropdown.Items.Add(gameProfileName);
        }
        
        if (!string.IsNullOrWhiteSpace(App.StartupGameName))
        {
            if (int.TryParse(App.StartupGameName, out int index))
            {
                GameDropdown.SelectedIndex = index;
            }
            else if(GameDropdown.Items.IndexOf(App.StartupGameName) != -1)
            {
                GameDropdown.SelectedIndex = GameDropdown.Items.IndexOf(App.StartupGameName);
            }
        }
        else if (GameCache.HasValue)
            GameDropdown.SelectedIndex = GameCache.Value;
        else
            GameDropdown.SelectedIndex = GameDropdown.Items.IndexOf("Poker Night at the Inventory Remastered");
        
        
        if (GameDataDirectoryCache != null)
        {
            GameDataPath.Text = GameDataDirectoryCache;
        }
        
        // if (LoadedArchivesCache != null && LoadedArchivesCache.Count > 0)
        // {
        //     ArchiveList.Items.Clear();
        //     //assume the resource context is loaded
        //     foreach (string archivePath in LoadedArchivesCache)
        //     {
        //         var li = new ListBoxItem();
        //         var text = new TextBlock();
        //
        //         int seperator = archivePath.LastIndexOfAny(['\\', '/']);
        //
        //         string archiveName = archivePath.Substring(seperator + 1);
        //
        //         text.Text = archiveName;
        //         text.HorizontalAlignment = HorizontalAlignment.Left;
        //         li.Content = text;
        //
        //         ArchiveList.Items.Add(li);
        //     }
        //     
        //     ArchiveListGrid.IsVisible = true;
        // }

        if (!startedUp)
        {
            startedUp = true;
        }
        
        if (string.IsNullOrWhiteSpace(App.StartupGameArchivesDirectory)) return;
        GameDataPath.Text = App.StartupGameArchivesDirectory;

        bool chooseArchive = App.StartupChooseArchive;
        bool loadGameDir = App.StartupLoadGameDir;
        //todo: Change this to be compatable with the new system that has a couple extra options
        if(loadGameDir && !chooseArchive && !string.IsNullOrWhiteSpace(App.StartupGameArchivesDirectory))
        {
            Dispatcher.InvokeAsync(LoadResources);
            return;
        }

        //for right now: StartupChooseArchive implies App.StartupLoadGameDir
        if (chooseArchive && !string.IsNullOrWhiteSpace(App.StartupArchive) && !string.IsNullOrWhiteSpace(App.StartupGameArchivesDirectory))
        {
            if (!loadGameDir)
            {
                Console.Out.WriteLine("Automatically loading game directory (possibly against your wishes). This is based on legacy behaviour and you should update your usage methodology.");
            }
            Dispatcher.InvokeAsync(() => LoadArchiveAfterResources(App.StartupArchive));
        }
        
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

        // ListBoxItem item = new()
        // {
        //     Content = messageBlock
        // };

        MessageBox.Items.Add(messageBlock);

        MessageBox.SelectedIndex = MessageBox.Items.Count - 1;
    }

    public void AddMessageToBox(string message, Color textColor)
    {
        textColor = Color.FromArgb(0xFF, textColor.R, textColor.G, textColor.B);
        var messageBlock = new TextBlock
        {
            Text = message,
            FontSize = 16,
            HorizontalAlignment = HorizontalAlignment.Left,
            Foreground = new SolidColorBrush(textColor),
            Margin = new Thickness(5, 5, 5, 5)
        };

        // ListBoxItem item = new()
        // {
        //     Content = messageBlock
        // };

        MessageBox.Items.Add(messageBlock);

        MessageBox.SelectedIndex = MessageBox.Items.Count - 1;

    }


    private static FilePickerFileType TelltaleArchive2Files =
        new ("TellTale Archive 2 Files (*.ttarch2) | *.ttarch2")
        {
            AppleUniformTypeIdentifiers =
                new[] { "public.item" }, //apple moment. choose whatever, will error if wrong lol
            Patterns = new[] { "*.ttarch2" }
        };


    private void OpenFolder_OnClick(object sender, RoutedEventArgs e)
    {
        Dispatcher.InvokeAsync(OpenFolder);
    }

    private async void OpenFolder()
    {
        try
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
        catch (Exception)
        {
            await Console.Out.WriteLineAsync("File picking cancelled");
        }
    }
    

    private void LoadResources_OnClick(object sender, RoutedEventArgs e)
    {
        Dispatcher.InvokeAsync(LoadResources);
    }
    

    private static async Task<List<string>> LoadGameDir(MainWindow window, string? gameDataPath, string chosenGame)
    {
        try
        {
            if (string.IsNullOrEmpty(gameDataPath))
                return [];

            string fullPath = Path.GetFullPath(gameDataPath);

            if (!Directory.Exists(fullPath))
                return [];

            //make sure the Loading... is shown before huge freeze
            Task.Delay(100).GetAwaiter().GetResult();

            var cts = new CancellationTokenSource();

            
            Task<List<string>> pathTask = Task.Run(() =>
                ResourceLoader.Instance.LoadResourceContexts(cts, window, fullPath, chosenGame), cts.Token);

            while (!pathTask.IsCompleted)
            {
                await Task.Yield();
            }
            
            if (cts.IsCancellationRequested) //i don't even know. like what.
            {
                await Console.Out.WriteLineAsync("Resdesc lua parsing failed! Watch out!!");

                window.Dispatcher.Invoke(() => window.AddMessageToBox("Error parsing resource descriptions", Color.FromUInt32(0xf64a42)));

                return [];

            }

            List<string> archives = pathTask.Result;
            archives.Sort();

            return archives;
        }
        catch (Exception e)
        {
            Console.Out.WriteLine(e);
            return [];
        }
    }

    private void LoadResources()
    {
        string? gameDataPath = GameDataPath.Text;
        string gameDropdown = GameDropdown.Text!;
        Task.Run(() => LoadGameDir(this, gameDataPath, gameDropdown))
            .GetAwaiter()
            .OnCompleted(
                () => AddMessageToBox("Loaded game resources", Color.FromUInt32(0x62a36d))
                );

        
    }

    private void LoadArchiveAfterResources(string archive)
    {
        string? gameDataPath = GameDataPath.Text;
        string gameDropdown = GameDropdown.Text!;
        Task.Run(() => LoadGameDir(this, gameDataPath, gameDropdown))
            .GetAwaiter()
            .OnCompleted(() => LoadArchive(archive));
    }
    
    private void LoadArchive(string archive)
    {
        ResourceLoader.Instance.LoadArchive(this, Path.Combine(GameDataPath.Text!, archive), GameDropdown.Text!);

        ArchiveModelList list = new ArchiveModelList(false) { OverriddenOwner = this };
        
        CloseOnNewWindowOpened(list);
        
    }

    // private void LoadArchive(object sender, RoutedEventArgs e)
    // {
    //     if (ArchiveList.SelectedItem is not ListBoxItem selectedItem)
    //         return;
    //
    //     string archiveName = ((TextBlock)selectedItem.Content!).Text ?? "";
    //
    //     ResourceLoader.Instance.LoadArchive(this, Path.Combine(GameDataPath.Text!, archiveName), GameDropdown.Text!);
    //
    //     ArchiveModelList list = new ArchiveModelList(false) {OverriddenOwner = this };
    //     CloseOnNewWindowOpened(list);
    // }

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

        if (!(GameDataPath.Text?.Contains("Game Data Directory") ?? false))
        {
            GameDataDirectoryCache = GameDataPath.Text;
        }

        // LoadedArchivesCache = null;
        //
        // if (ArchiveList.Items.Count == 0)
        //     return;
        //
        // if (ArchiveList.Items[0] is not ListBoxItem { Content: TextBlock b })
        //     return;
        //
        // if (b.Text?.Contains("Loading...") ?? false)
        //     return;
        //
        // var archives = new List<string>();
        // foreach (object? item in ArchiveList.Items)
        // {
        //     if(item is not ListBoxItem listItem)
        //         continue;
        //         
        //     string? archiveName = ((TextBlock)listItem.Content!).Text;
        //
        //     archives.Add(Path.Combine(GameDataPath.Text!, archiveName!));
        // }
        //
        // LoadedArchivesCache = archives;
    }

    private void ChooseSingleArchive_OnClick(object? sender, RoutedEventArgs args)
    {
        string? gameDataPath = GameDataPath.Text;
        string gameDropdown = GameDropdown.Text!;
        Task.Run(() => LoadSingleArchive(this, gameDataPath, gameDropdown));

    }

    private async void LoadSingleArchive(MainWindow window, string? gameDataPath, string chosenGame)
    {
        try
        {
            List<string> archives = await LoadGameDir(window, gameDataPath, chosenGame);


            await window.Dispatcher.Invoke(async () =>
            {
                var dialouge = new SelectSingleArchive(archives);

                string? archive = await dialouge.ShowDialog<string?>(this);

                if (archive == null)
                    return;

                ResourceLoader.Instance.LoadArchive(this, Path.Combine(gameDataPath!, archive), chosenGame);

                AddMessageToBox($"Archive {archive} selected!", Color.FromUInt32(0x62a36d));
            });

        }
        catch (Exception e)
        {
            Console.Out.WriteLine(e);
        }
    }

    private void GameDataPath_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (!(sender is TextBox box))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(box.Text))
        {
            OpenGameDir.IsEnabled = false;
            ChooseSingleArchive.IsEnabled = false;
        }
        else
        {
            OpenGameDir.IsEnabled = true;
            ChooseSingleArchive.IsEnabled = true;
        }
    }

    private void MoveToListMeshes(object? sender, RoutedEventArgs e)
    {
        ArchiveModelList list = new ArchiveModelList(false) { OverriddenOwner = this };
        CloseOnNewWindowOpened(list);
    }

    private void MoveToListAgents(object? sender, RoutedEventArgs e)
    {
        ArchiveAgentList list;

        (string name, IEnumerable<ResourceEntry> thing) = ResourceLoader.Instance.GetEntriesInCurrentArchive();
        
        list = !string.IsNullOrWhiteSpace(ResourceLoader.Instance.ArchiveLocation) 
            ? new ArchiveAgentList([name]) { OverriddenOwner = this } 
            : new ArchiveAgentList { OverriddenOwner = this };
        
        CloseOnNewWindowOpened(list);
    }

    private void MoveToImportMeshes(object? sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void MoveToImportAgents(object? sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
}