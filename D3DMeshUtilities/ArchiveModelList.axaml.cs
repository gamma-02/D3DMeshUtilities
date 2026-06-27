using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using D3DMeshUtilities.Code;
using TelltaleToolKit.IO.Archives;

namespace D3DMeshUtilities;

public partial class ArchiveModelList : BaseProjectWindow
{
    public static readonly Dictionary<string, List<string>> ArchiveMeshListDictionary =
        new Dictionary<string, List<string>>();

    private bool _enableButton = true;

    // public bool fromTab = true;

    public ArchiveModelList()
    {
        InitializeComponent();
        
        if (ArchiveMeshListDictionary.Count == 0 && !Design.IsDesignMode)
        {
            ListPanel.Children.Clear();
            
            var notLoadedMessage = new TextBlock();
            notLoadedMessage.Text = "Archive not yet loaded";

            ListPanel.Children.Add(notLoadedMessage);
            
            _enableButton = false;
        }

        _enableButton = false;

        bool startupChooseModels = App.StartConversionInstantly;
        
        if (startupChooseModels && App.StartupModels.Length > 0)
        {
            Dispatcher.InvokeAsync(ConvertInstantly);
            return;
        }
        
        Dispatcher.InvokeAsync(FillModelList);

    }
    
    public ArchiveModelList(bool fromTab = true)
    {
        InitializeComponent();
        
        if (fromTab && ArchiveMeshListDictionary.Count == 0 && !Design.IsDesignMode)
        {
            ListPanel.Children.Clear();
            
            var notLoadedMessage = new TextBlock
            {
                Text = "Archive not yet loaded :(",
                FontSize = 20
            };

            ListPanel.Children.Add(notLoadedMessage);
            
            _enableButton = false;
        }

        bool startConversionInstantly = App.StartConversionInstantly;
        
        if (startConversionInstantly && App.StartupModels.Length > 0)
        {
            Dispatcher.InvokeAsync(ConvertInstantly);
            return;
        }
        
        Dispatcher.InvokeAsync(FillModelList);
        
    }
    

    private async void ConvertInstantly()
    {
        try
        {
            await Task.Delay(100);

            ResourceLoader.ArchiveLocationLock.Enter();
            
            BeginConversion(App.StartupModels);
            
        }
        catch (Exception)
        {
            //ignored
        }
        finally
        {
            if(ResourceLoader.ArchiveLocationLock.IsHeldByCurrentThread)
                ResourceLoader.ArchiveLocationLock.Exit();
        }
    }

    private void FillModelList()
    {
        ResourceLoader.ArchiveLocationLock.TryEnter();

        try
        {
            if(!Design.IsDesignMode)
            {
                ListPanel.Children.Clear();
            }

            HashSet<string> modelsToTrySelect = new (!App.StartConversionInstantly ? App.StartupModels.ToList() : []);
            
            if (!string.IsNullOrWhiteSpace(ResourceLoader.Instance.ArchiveLocation) &&
                !ArchiveMeshListDictionary.ContainsKey(ResourceLoader.Instance.ArchiveLocation))
            {
                (String rcName, IEnumerable<ResourceEntry> entryList) =
                    ResourceLoader.Instance.GetEntriesInCurrentArchive();
                List<string> entries = entryList
                    .Where(resource => resource.Name.EndsWith("d3dmesh"))
                    .Select(entry => entry.Name)
                    .ToList();

                entries.Sort();
                
                ArchiveMeshListDictionary[rcName] = entries;
                
                var listBox = new ArchiveListBox(rcName, entries);
                listBox.SelectionChanged += ArchiveListBox_OnSelectionChanged;
                listBox.MeshesToTrySelect = modelsToTrySelect;
                
                ListPanel.Children.Add(listBox);

            }
            else
            {
                Dictionary<string, IEnumerable<ResourceEntry>> entries = ResourceLoader.Instance.GetEntriesInCurrentContexts();
                
                foreach(string archive in entries.Keys)
                {
                    List<string> meshes = entries[archive]
                        .Where(resource => resource.Name.EndsWith("d3dmesh"))
                        .Select(entry => entry.Name)
                        .ToList();

                    meshes.Sort();

                    if (meshes.Count == 0)
                    {
                        continue; //todo: Add skipping archives without meshes as a config option
                    }

                    ArchiveMeshListDictionary[archive] = meshes;
                }
                
                
                foreach(string archive in ArchiveMeshListDictionary.Keys)
                {
                    var listBox = new ArchiveListBox(archive, ArchiveMeshListDictionary[archive]);
                    listBox.SelectionChanged += ArchiveListBox_OnSelectionChanged;
                    listBox.MeshesToTrySelect = modelsToTrySelect;
                    
                    ListPanel.Children.Add(listBox);
                }
                
            }

            if (ArchiveMeshListDictionary.Count != 0 || !_enableButton) return;
            
            var notLoadedMessage = new ArchiveListBox("No models found :(", new List<string>());
            notLoadedMessage.GetHeaderText().FontSize = 20;

            ListPanel.Children.Add(notLoadedMessage);
            
            _enableButton = false;

            // i wish I could use this :(
            // Color one = Color.FromRgb(0x43, 0x43, 0x43);
            // Color two = Color.FromRgb(0x26, 0x28, 0x2C);
            // Brush backgroundA = new SolidColorBrush(one);
            // Brush backgroundB = new SolidColorBrush(two);
            //
            // for (int i = 0; i < ModelList.Items.Count; i++)
            // {
            //     var li = ModelList.Items[i] as ListBoxItem;
            //
            //     if (li == null)
            //         continue;
            //
            //     li.Background = (i % 2 == 0) ? backgroundB : backgroundA;
            //
            // }
        }
        finally
        {
            ResourceLoader.ArchiveLocationLock.Exit();
        }
    }

    void ArchiveListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (IsNothingSelected() || !_enableButton)
        {
            ConvertButtonGrid.IsVisible = false;
        }
        else
        {
            ConvertButtonGrid.IsVisible = true;
        }
    }

    public bool IsNothingSelected()
    {
        foreach (Control? child in ListPanel.Children)
        {
            if(child is not ArchiveListBox archiveListBox) continue;

            if (archiveListBox.GetSelectedItems() is not { Count: 0 })
            {
                return false;
            }
        }

        return true;
    }

    public List<string> GetSelectedMeshes()
    {
        List<string> meshes = [];

        if (IsNothingSelected()) return meshes;

        foreach (Control? child in ListPanel.Children)
        {
            if(child is not ArchiveListBox archiveListBox) continue;
            
            if(archiveListBox.GetSelectedItems() is { Count: 0 }) continue; //slightly faster, checks for empty
            
            meshes.AddRange(archiveListBox.GetSelectedMeshes());
        }

        return meshes;
    }

    void BeginConversion(string[] modelArray)
    {
        List<string> models = modelArray.ToList();

        Converting converting = new Converting(new MeshConversionTask(models))
        {
            OverriddenOwner = this
        };

        CloseOnNewWindowOpened(converting);
    }

    void BeginConversion(object sender, RoutedEventArgs routedEventArgs)
    {
        List<string> models = GetSelectedMeshes();

        if(models.Count == 0) return;

        Converting converting = new Converting(new MeshConversionTask(models))
        {
            OverriddenOwner = this
        };
        
        CloseOnNewWindowOpened(converting);
        
    }
    
    
    public override Window GetWindow()
    {
        return Window.ListModels;
    }

    public List<string> GetModelsToConvert() => GetSelectedMeshes();

    public class MeshConversionTask(List<string> meshesToConvert) : Converting.ConversionTask
    {
        private List<string> _meshesToConvert = meshesToConvert;
        
        public override bool ValidateTask(Converting? converting)
        {
            if (_meshesToConvert.Count != 0) return true;
            
            Console.Out.WriteLine("No models provided!");
            converting?.AddMessageToBox("No models provided!");

            return false;

        }

        public override async void Convert(string filePath, Converting? converting, Action completeTaskAction)
        {
            D3DMeshManager manager = new D3DMeshManager(_meshesToConvert, filePath);
            
            converting?.Dispatcher.Invoke(() => converting.AddMessageToBox("Converting..."));
            await Task.Delay(100);
            
            manager.LoadMeshes(converting)?
                .GetAwaiter().OnCompleted(() => converting?.Dispatcher.Invoke(completeTaskAction)); //lol

            Console.Out.WriteLine("Read mesh data from ttarchive!");
            
        }
    }
}