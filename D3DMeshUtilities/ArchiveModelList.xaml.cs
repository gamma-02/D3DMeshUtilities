using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace D3DMeshUtilities;

public partial class ArchiveModelList : BaseProjectWindow
{
    public ArchiveModelList()
    {
        InitializeComponent();

        // ModelList.SelectionMode = SelectionMode.Extended;

        bool startupChooseModels = App.StartupChooseModels;
        
        if (startupChooseModels && App.StartupModels.Length <= 0)
        {
            Dispatcher.InvokeAsync(FillModelList);
        }
        else if (!startupChooseModels && App.StartupModels.Length > 0)
        {
            Dispatcher.InvokeAsync(FillModelList);
            Dispatcher.InvokeAsync(SelectModels);
        }
        else if (startupChooseModels && App.StartupModels.Length > 0)
        {
            Dispatcher.InvokeAsync(ConvertInstantly);
        }



    }

    private async void SelectModels()
    {


        try
        {
            await Task.Delay(100);

            LoadedArchive.ArchiveLocationLock.TryEnter();

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
            if(LoadedArchive.ArchiveLocationLock.IsHeldByCurrentThread)
                LoadedArchive.ArchiveLocationLock.Exit();
        }
    }

    private async void ConvertInstantly()
    {
        try
        {
            await Task.Delay(100);

            LoadedArchive.ArchiveLocationLock.Enter();
            
            BeginConversion(App.StartupModels);
        }
        catch (Exception e)
        {
            //ignored
        }
        finally
        {
            if(LoadedArchive.ArchiveLocationLock.IsHeldByCurrentThread)
                LoadedArchive.ArchiveLocationLock.Exit();
        }
    }

    private void FillModelList()
    {

        LoadedArchive.ArchiveLocationLock.TryEnter();

        try
        {

            ModelList.Items.Clear();

            foreach (var element in LoadedArchive.Instance.CurrentArchive?.FileEntries!)
            {
                if (element.Name.EndsWith("d3dmesh"))
                {
                    var li = new ListViewItem();
                    var text = new TextBlock();

                    text.Text = element.Name;
                    li.Content = text;

                    ModelList.Items.Add(li);
                }
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
            LoadedArchive.ArchiveLocationLock.Exit();
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
    
}