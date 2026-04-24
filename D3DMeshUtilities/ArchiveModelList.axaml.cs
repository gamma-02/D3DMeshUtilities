using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace D3DMeshUtilities;

public partial class ArchiveModelList : BaseProjectWindow
{
    public static readonly Dictionary<string, List<string>> ArchiveMeshListDictionary =
        new Dictionary<string, List<string>>();
    
    public ArchiveModelList()
    {
        InitializeComponent();

        // ModelList.SelectionMode = SelectionMode.Extended;

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
        catch (Exception e)
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
            // Dispatcher.
        }
        catch (Exception e)
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
                    .Where(entry => entry.Name.EndsWith("d3dmesh"))
                    .Select(fe => fe.Name)
                    .ToList();

                entries.Sort();

                ArchiveMeshListDictionary[ResourceLoader.Instance.ArchiveLocation] = entries;
            }

            foreach (string element in ArchiveMeshListDictionary[ResourceLoader.Instance.ArchiveLocation])
            {
                
                var li = new ListBoxItem();
                var text = new TextBlock();

                text.Text = element;
                li.Content = text;

                ModelList.Items.Add(li);
                
            }

            Color one = Color.FromRgb(0x43, 0x43, 0x43);
            Color two = Color.FromRgb(0x26, 0x28, 0x2C);
            Brush backgroundA = new SolidColorBrush(one);
            Brush backgroundB = new SolidColorBrush(two);
            
            for (int i = 0; i < ModelList.Items.Count; i++)
            {
                var li = ModelList.Items[i] as ListBoxItem;

                if (li == null)
                    continue;

                if (i % 2 == 0)
                    li.Background = backgroundB;
                else
                    li.Background = backgroundA;

            }
        }
        finally
        {
            ResourceLoader.ArchiveLocationLock.Exit();
        }
    }

    void ModelList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedItems = ModelList.SelectedItems;

        if (selectedItems.Count == 0)
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

        converting.Show();
        
        this.Hide();

        SetMainWindow(converting);

        converting.OverriddenOwner = null;
        
        this.Close();
    }

    void BeginConversion(object sender, RoutedEventArgs routedEventArgs)
    {

        List<string> models = [];

        foreach (var item in ModelList.SelectedItems)
        {
            if (item == null)
                continue;
            
            ListBoxItem? i = item as ListBoxItem;
            
            if(i == null)
                continue;
            
            TextBlock? b = i.Content as TextBlock;
            
            if(b == null)
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

        foreach (var item in ModelList.SelectedItems)
        {
            if (item == null)
                continue;
            
            ListBoxItem? i = item as ListBoxItem;
            
            if(i == null)
                continue;
            
            TextBlock? b = i.Content as TextBlock;
            
            if(b == null)
                continue;
            
            models.Add(b.Text);
        }

        return models;
    }
}