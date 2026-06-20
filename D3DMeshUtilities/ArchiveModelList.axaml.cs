using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;

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
            ModelList.Items.Clear();
            
            
            var li = new ListBoxItem();
            var text = new TextBlock
            {
                Text = "Archive not yet loaded",
                FontSize = 20
            };

            li.Content = text;
            li.HorizontalAlignment = HorizontalAlignment.Center;

            ModelList.Items.Add(li);

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
            ModelList.Items.Clear();
            
            
            var li = new ListBoxItem();
            var text = new TextBlock
            {
                Text = "Archive not yet loaded",
                FontSize = 20
            };

            li.Content = text;
            li.HorizontalAlignment = HorizontalAlignment.Center;

            ModelList.Items.Add(li);

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
                if (ModelList.SelectedItems == null) 
                    continue;
                
                foreach (object? item in ModelList.SelectedItems)
                {
                    var i = item as ListBoxItem;

                    if (i?.Content is not TextBlock b)
                        continue;

                    if (b.Text != null && b.Text.Equals(model))
                        i.IsSelected = true;
                }
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

            if (string.IsNullOrWhiteSpace(ResourceLoader.Instance.ArchiveLocation))
            {
                return;
            }

            ModelList.Items.Clear();
            
            if(!ArchiveMeshListDictionary.ContainsKey(ResourceLoader.Instance.ArchiveLocation))
            {
                List<string> entries = ResourceLoader.Instance.GetEntriesInCurrentArchive()
                    .Where(resource => resource.Name.EndsWith("d3dmesh"))
                    .Select(entry => entry.Name)
                    .ToList();

                entries.Sort();

                ArchiveMeshListDictionary[ResourceLoader.Instance.ArchiveLocation] = entries;
            }

            foreach (string element in ArchiveMeshListDictionary[ResourceLoader.Instance.ArchiveLocation])
            {
                
                var li = new ListBoxItem();
                var text = new TextBlock
                {
                    Text = element
                };

                li.Content = text;

                ModelList.Items.Add(li);
                
            }

            if (ArchiveMeshListDictionary.Count == 0 && _enableButton)
            {
                var li = new ListBoxItem();
                var text = new TextBlock
                {
                    Text = "No models found!",
                    FontSize = 20
                };

                li.Content = text;
                li.HorizontalAlignment = HorizontalAlignment.Center;

                ModelList.Items.Add(li);
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
        IList? selectedItems = ModelList.SelectedItems;

        if (selectedItems?.Count == 0 || !_enableButton)
        {
            ConvertButtonGrid.IsVisible = false;
        }
        else
        {
            ConvertButtonGrid.IsVisible = true;
        }
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

        if (ModelList.SelectedItems == null) return;

        foreach (var item in ModelList.SelectedItems)
        {
            if (item == null)
                continue;
            
            ListBoxItem? i = item as ListBoxItem;
            
            if(i == null)
                continue;
            
            TextBlock? b = i.Content as TextBlock;
            
            if(b?.Text == null)
                continue;
            
            models.Add(b.Text);
        }

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

        if (ModelList.SelectedItems == null) return models;
        
        foreach (var item in ModelList.SelectedItems)
        {
            if (item == null)
                continue;

            ListBoxItem? i = item as ListBoxItem;

            if (i == null)
                continue;

            TextBlock? b = i.Content as TextBlock;

            if (b?.Text == null)
                continue;

            models.Add(b.Text);
        }

        return models;
    }
}