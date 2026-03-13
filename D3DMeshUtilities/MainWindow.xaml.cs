using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using D3DMeshUtilities.Code;
using Lua;
using TelltaleToolKit.Utility.Blowfish;
using Path = System.IO.Path;

namespace D3DMeshUtilities;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        foreach (T3BlowfishKey key in Enum.GetValues<T3BlowfishKey>())
        {
            GameDropdown.Items.Add(key.GetGameName());
        }

        GameDropdown.SelectedIndex = 79;
    }


    private void OpenFile_OnClick(object sender, RoutedEventArgs e)
    {
        Dispatcher.InvokeAsync(OpenFile);
    }

    private async void OpenFile()
    {
        var dialouge = new Microsoft.Win32.OpenFileDialog();

        dialouge.Filter = "TellTale Archive 2 Files (*.ttarch2) | *.ttarch2";

        bool? result = dialouge.ShowDialog();

        if (result == true)
        {
            string file = dialouge.FileName;

            filePath.Text = file;
        }
    }

    private void OpenFolder_OnClick(object sender, RoutedEventArgs e)
    {
        Dispatcher.InvokeAsync(OpenFolder);
    }

    private async void OpenFolder()
    {
        var dialouge = new Microsoft.Win32.OpenFolderDialog();

        bool? result = dialouge.ShowDialog();

        if (result == true)
        {
            string folder = dialouge.FolderName;

            GameDataPath.Text = folder;
        }
    }

    private void OpenArchive_OnClick(object sender, RoutedEventArgs e)
    {
        if (!File.Exists(filePath.Text))
            return;

        LoadedArchive.Instance.LoadArchive(Dispatcher, filePath.Text, GameDropdown.Text);

        ArchiveModelList list = new ArchiveModelList()
        {
            Owner = this
        };


        list.Show();

        this.Hide();

        Application.Current.MainWindow = list;

        list.Owner = null;

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

            ArchiveListGrid.Visibility = Visibility.Visible;

            //make sure the Loading... is shown before huge freeze
            await Task.Delay(100);

            var pathTask =
                LoadedArchive.Instance.LoadResourceContexts(Dispatcher, GameDataPath.Text, GameDropdown.Text);
        
            await pathTask;
        
            //remove "Loading..."
            ArchiveList.Items.Clear();

            foreach (string archivePath in pathTask.Result)
            {
                var li = new ListViewItem();
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

        if (selectedItems.Count == 0)
        {
            OpenArchiveGrid.Visibility = Visibility.Collapsed;
        }
        else
        {
            OpenArchiveGrid.Visibility = Visibility.Visible;
        }
    }

    private void LoadArchive(object sender, RoutedEventArgs e)
    {
        if (ArchiveList.SelectedItem is not ListViewItem selectedItem)
            return;

        var archiveName = ((TextBlock)selectedItem.Content).Text;

        LoadedArchive.Instance.LoadArchive(Dispatcher, Path.Combine(GameDataPath.Text, archiveName), GameDropdown.Text);

        ArchiveModelList list = new ArchiveModelList()
        {
            Owner = this
        };

        list.Show();

        this.Hide();

        Application.Current.MainWindow = list;

        list.Owner = null;

        this.Close();
    }
}