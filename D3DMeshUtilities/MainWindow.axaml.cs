using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using TelltaleToolKit;
using HorizontalAlignment = Avalonia.Layout.HorizontalAlignment;
using SelectionChangedEventArgs = Avalonia.Controls.SelectionChangedEventArgs;
using TextBlock = Avalonia.Controls.TextBlock;
using RoutedEventArgs = Avalonia.Interactivity.RoutedEventArgs;

namespace D3DMeshUtilities;

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

        TttkInit.Init();

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
            GameDropdown.SelectedIndex = GameDropdown.Items.IndexOf(App.StartupGameName);
        else if (GameCache.HasValue)
            GameDropdown.SelectedIndex = GameCache.Value;
        else
            GameDropdown.SelectedIndex = GameDropdown.Items.IndexOf("Poker Night at the Inventory Remastered");

        if (SingleArchivePathCache != null)
        {
            filePath.Text = SingleArchivePathCache;
        }

        if (GameDataDirectoryCache != null)
        {
            GameDataPath.Text = GameDataDirectoryCache;
        }

        if (LoadedArchivesCache != null && LoadedArchivesCache.Count > 0)
        {
            ArchiveList.Items.Clear();
            //assume the resource context is loaded
            foreach (string archivePath in LoadedArchivesCache)
            {
                var li = new ListBoxItem();
                var text = new TextBlock();

                int seperator = archivePath.LastIndexOfAny(['\\', '/']);

                string archiveName = archivePath.Substring(seperator + 1);

                text.Text = archiveName;
                text.HorizontalAlignment = HorizontalAlignment.Left;
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
        DispatcherOperation op = Dispatcher.InvokeAsync(LoadResources);

        if (!App.StartupChooseArchive || string.IsNullOrWhiteSpace(App.StartupArchive)) return;
        Dispatcher.InvokeAsync(() => LoadArchiveAfterResources(op, App.StartupArchive));
        
    }
    
     private string CreateMultilevelIndentString()
        {
            // Creates a TextWriter to use as the base output writer.
            var baseTextWriter = new StringWriter();

            // Create an IndentedTextWriter and set the tab string to use
            // as the indentation string for each indentation level.
            var indentWriter = new IndentedTextWriter(baseTextWriter, "    ");

            // Sets the indentation level.
            indentWriter.Indent = 0;

            // Output test strings at stepped indentations through a recursive loop method.
            WriteLevel(indentWriter, 0, 5);

            // Return the resulting string from the base StringWriter.
            return baseTextWriter.ToString();
        }

        private void WriteLevel(IndentedTextWriter indentWriter, int level, int totalLevels)
        {
            // Output a test string with a new-line character at the end.
            indentWriter.WriteLine($"This is a test phrase. Current indentation level: {level}");

            // If not yet at the highest recursion level, call this output method for the next level of indentation.
            if( level < totalLevels )
            {
                // Increase the indentation count for the next level of indented output.
                indentWriter.Indent++;

                // Call the WriteLevel method to write test output for the next level of indentation.
                WriteLevel(indentWriter, level+1, totalLevels);

                // Restores the indentation count for this level after the recursive branch method has returned.
                indentWriter.Indent--;
            }
            else
            {
                // Outputs a string using the WriteLineNoTabs method.
                indentWriter.WriteLineNoTabs("This is a test phrase written with the IndentTextWriter.WriteLineNoTabs method.");
            }

            // Outputs a test string with a new-line character at the end.
            indentWriter.WriteLine("This is a test phrase. Current indentation level: " + level.ToString());
        }


    private void OpenFile_OnClick(object sender, RoutedEventArgs e)
    {
        Dispatcher.InvokeAsync(OpenFile);
    }

    private static FilePickerFileType TelltaleArchive2Files =
        new ("TellTale Archive 2 Files (*.ttarch2) | *.ttarch2")
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

        ArchiveModelList list = new ArchiveModelList(false) { OverriddenOwner = this };
        CloseOnNewWindowOpened(list);

        // list.Show();
        //
        // this.Hide();
        //
        // SetMainWindow(list);
        //
        // list.OverriddenOwner = null;
        //
        // this.Close();
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
            Task.Delay(100).GetAwaiter().GetResult();

            var cts = new CancellationTokenSource();

            Task<List<string>> pathTask = ResourceLoader.Instance.LoadResourceContexts(cts, GameDataPath.Text, GameDropdown.Text!);

            await pathTask;

            if (cts.IsCancellationRequested)//i don't even know. like what.
            {
                ArchiveList.Items.Clear();
                Console.Out.WriteLine("Resdesc lua parsing failed! Watch out!!");
                
                var li = new ListBoxItem();
                var text = new TextBlock();

                text.Text = "Error parsing resource descriptions!";
                text.HorizontalAlignment = HorizontalAlignment.Center;
                li.Content = text;

                ArchiveList.Items.Add(li);

                return;

            }
            
            List<string> archives = pathTask.Result;
            archives.Sort();
        
            //remove "Loading..."
            ArchiveList.Items.Clear();

            foreach (string archivePath in archives)
            {
                var li = new ListBoxItem();
                var text = new TextBlock();

                int seperator = archivePath.LastIndexOfAny(['\\', '/']);

                string archiveName = archivePath.Substring(seperator + 1);

                text.Text = archiveName;
                text.HorizontalAlignment = HorizontalAlignment.Left;
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

    private async void LoadArchiveAfterResources(DispatcherOperation resourceLoad, string archive)
    {
        try
        {
            await resourceLoad;

            LoadArchive(archive);

        }
        catch (Exception)
        {
            // ignored
        }
    }
    
    private void LoadArchive(string archive)
    {
        ResourceLoader.Instance.LoadArchive(Dispatcher, Path.Combine(GameDataPath.Text!, archive), GameDropdown.Text!);

        ArchiveModelList list = new ArchiveModelList(false) { OverriddenOwner = this };
        
        CloseOnNewWindowOpened(list);
        
    }

    private void LoadArchive(object sender, RoutedEventArgs e)
    {
        if (ArchiveList.SelectedItem is not ListBoxItem selectedItem)
            return;

        string archiveName = ((TextBlock)selectedItem.Content!).Text ?? "";

        ResourceLoader.Instance.LoadArchive(Dispatcher, Path.Combine(GameDataPath.Text!, archiveName), GameDropdown.Text!);

        ArchiveModelList list = new ArchiveModelList(false) {OverriddenOwner = this };
        CloseOnNewWindowOpened(list);

        // list.Show();
        //
        // this.Hide();
        //
        // SetMainWindow(list);
        //
        // list.OverriddenOwner = null;
        //
        // this.Close();
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

        if (!(filePath.Text?.Contains("Archive containing mesh to extract") ?? false))
        {
            SingleArchivePathCache = filePath.Text;
        }

        if (!(GameDataPath.Text?.Contains("Game Data Directory") ?? false))
        {
            GameDataDirectoryCache = GameDataPath.Text;
        }

        LoadedArchivesCache = null;

        if (ArchiveList.Items.Count == 0)
            return;

        if (ArchiveList.Items[0] is not ListBoxItem { Content: TextBlock b })
            return;

        if (b.Text?.Contains("Loading...") ?? false)
            return;
        
        var archives = new List<string>();
        foreach (object? item in ArchiveList.Items)
        {
            if(item is not ListBoxItem listItem)
                continue;
                
            string? archiveName = ((TextBlock)listItem.Content!).Text;

            archives.Add(Path.Combine(GameDataPath.Text!, archiveName!));
        }

        LoadedArchivesCache = archives;
    }
}