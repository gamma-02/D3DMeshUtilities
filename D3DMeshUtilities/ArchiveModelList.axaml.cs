using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
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
        
        if (ArchiveMeshListDictionary.Count == 0)
        {
            if(!Design.IsDesignMode)
            {
                ListPanel.Children.Clear();
            }
            
            var notLoadedMessage = new ArchiveListBox("Archive not yet loaded", new List<string>());
            notLoadedMessage.GetHeaderText().FontSize = 20;

            ListPanel.Children.Add(notLoadedMessage);
            
            _enableButton = false;
        }

        _enableButton = false;

        bool startupChooseModels = App.StartupChooseModels;
        
        if (startupChooseModels && App.StartupModels.Length > 0)
        {
            Dispatcher.InvokeAsync(ConvertInstantly);
            return;
        }
        
        Dispatcher.InvokeAsync(FillModelList);
        
        if (!startupChooseModels && App.StartupModels.Length > 0)
        {
            Dispatcher.InvokeAsync(SelectModels);
        }


    }
    
    public ArchiveModelList(bool fromTab = true)
    {
        InitializeComponent();

        // ModelList.SelectionMode = SelectionMode.Extended;

        if (fromTab && ArchiveMeshListDictionary.Count == 0)
        {
            if(!Design.IsDesignMode)
            {
                ListPanel.Children.Clear();
            }
            
            var notLoadedMessage = new ArchiveListBox("Archive not yet loaded", new List<string>());
            notLoadedMessage.GetHeaderText().FontSize = 20;

            ListPanel.Children.Add(notLoadedMessage);
            
            _enableButton = false;
        }

        bool startupChooseModels = App.StartupChooseModels;
        
        if (startupChooseModels && App.StartupModels.Length > 0)
        {
            Dispatcher.InvokeAsync(ConvertInstantly);
            return;
        }
        
        Dispatcher.InvokeAsync(FillModelList);
        
        if (!startupChooseModels && App.StartupModels.Length > 0)
        {
            Dispatcher.InvokeAsync(SelectModels);
        }
        
    }

    private async void SelectModels()
    {
        try
        {
            await Task.Delay(100);

            ResourceLoader.ArchiveLocationLock.TryEnter();

            foreach (string model in App.StartupModels)
            {
                // if (ModelList.SelectedItems == null) 
                //     continue;
                //
                // foreach (object? item in ModelList.SelectedItems)
                // {
                //     var i = item as ListBoxItem;
                //
                //     if (i?.Content is not TextBlock b)
                //         continue;
                //
                //     if (b.Text != null && b.Text.Equals(model))
                //         i.IsSelected = true;
                // }
            }
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
                    
                    ListPanel.Children.Add(listBox);
                }
                
            }
            
            if (ArchiveMeshListDictionary.Count == 0 && _enableButton)
            {
                
                var notLoadedMessage = new ArchiveListBox("No models found :(", new List<string>());
                notLoadedMessage.GetHeaderText().FontSize = 20;

                ListPanel.Children.Add(notLoadedMessage);
            
                _enableButton = false;
            }

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

    void ModelList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // IList? selectedItems = ModelList.SelectedItems;
        //
        // if (selectedItems?.Count == 0 || !_enableButton)
        // {
        //     ConvertButtonGrid.IsVisible = false;
        // }
        // else
        // {
        //     ConvertButtonGrid.IsVisible = true;
        // }
    }
    
    void BeginConversion(string[] modelArray)
    {

        List<string> models = modelArray.ToList();

        Converting converting = new Converting(models)
        {
            OverriddenOwner = this
        };

        CloseOnNewWindowOpened(converting);
    }

    void BeginConversion(object sender, RoutedEventArgs routedEventArgs)
    {

        List<string> models = [];

        // if (ModelList.SelectedItems == null) return;
        //
        // foreach (var item in ModelList.SelectedItems)
        // {
        //     if (item == null)
        //         continue;
        //     
        //     ListBoxItem? i = item as ListBoxItem;
        //     
        //     if(i == null)
        //         continue;
        //     
        //     TextBlock? b = i.Content as TextBlock;
        //     
        //     if(b?.Text == null)
        //         continue;
        //     
        //     models.Add(b.Text);
        // }

        Converting converting = new Converting(models)
        {
            OverriddenOwner = this
        };

        converting.Show();
        
        this.Hide();

        SetMainWindow(converting);

        converting.OverriddenOwner = null;
        
        this.Close();
    }
    
    
    public override Window GetWindow()
    {
        return Window.ListModels;
    }

    public List<string> GetModelsToConvert()
    {
        List<string> models = [];

        // if (ModelList.SelectedItems == null) return models;
        //
        // foreach (var item in ModelList.SelectedItems)
        // {
        //     if (item == null)
        //         continue;
        //
        //     ListBoxItem? i = item as ListBoxItem;
        //
        //     if (i == null)
        //         continue;
        //
        //     TextBlock? b = i.Content as TextBlock;
        //
        //     if (b?.Text == null)
        //         continue;
        //
        //     models.Add(b.Text);
        // }

        return models;
    }
}