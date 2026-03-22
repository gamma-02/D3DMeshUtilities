using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TelltaleToolKit.TelltaleArchives;

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
                foreach (var item in ModelList.SelectedItems)
                {
                    if (item == null)
                        continue;

                    ListViewItem? i = item as ListViewItem;

                    if (i == null)
                        continue;

                    TextBlock? b = i.Content as TextBlock;

                    if (b == null)
                        continue;

                    if (b.Text.Equals(model))
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
                
                var li = new ListViewItem();
                var text = new TextBlock();

                text.Text = element;
                li.Content = text;

                ModelList.Items.Add(li);
                
            }

            Color lg = Brushes.LightGray.Color;
            Brush backgroundB =
                new SolidColorBrush(Color.FromRgb((byte)(lg.R - 25), (byte)(lg.G - 25), (byte)(lg.B - 25)));
            for (int i = 0; i < ModelList.Items.Count; i++)
            {
                ListViewItem? li = ModelList.Items[i] as ListViewItem;

                if (li == null)
                    continue;

                if (i % 2 == 0)
                    li.Background = Brushes.LightGray;
                else
                    li.Background = backgroundB;

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
            ConvertButtonGrid.Visibility = Visibility.Collapsed;
        }
        else
        {
            ConvertButtonGrid.Visibility = Visibility.Visible;
        }
    }
    
    void BeginConversion(string[] modelArray)
    {

        List<string> models = modelArray.ToList();

        Converting converting = new Converting(models)
        {
            Owner = this
        };

        converting.Show();
        
        this.Hide();

        Application.Current.MainWindow = converting;

        converting.Owner = null;
        
        this.Close();
    }

    void BeginConversion(object sender, RoutedEventArgs routedEventArgs)
    {

        List<string> models = [];

        foreach (var item in ModelList.SelectedItems)
        {
            if (item == null)
                continue;
            
            ListViewItem? i = item as ListViewItem;
            
            if(i == null)
                continue;
            
            TextBlock? b = i.Content as TextBlock;
            
            if(b == null)
                continue;
            
            models.Add(b.Text);
        }

        Converting converting = new Converting(models)
        {
            Owner = this
        };

        converting.Show();
        
        this.Hide();

        Application.Current.MainWindow = converting;

        converting.Owner = null;
        
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
            
            ListViewItem? i = item as ListViewItem;
            
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