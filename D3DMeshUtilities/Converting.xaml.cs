using System.Windows;
using D3DMeshUtilities.Code;

namespace D3DMeshUtilities;

public partial class Converting : Window
{

    private List<string> modelsToConvert;
    
    public Converting(List<string> modelsToConvert)
    {
        this.modelsToConvert = modelsToConvert;
        InitializeComponent();
    }


    private void OutFile_OnClick(object sender, EventArgs e)
    {
        Dispatcher.InvokeAsync(OpenFile);

    }
    
    private void OpenFile()
    {
        var dialouge = new Microsoft.Win32.OpenFolderDialog();
        
        bool? result = dialouge.ShowDialog();

        if (result == true)
        {
            string file = dialouge.FolderName;

            FilePath.Text = file;

        }
    }
    

    private void Convert_OnClick(object sender, RoutedEventArgs e)
    {
        D3DMeshManager manager = new D3DMeshManager(modelsToConvert, FilePath.Text);

        manager.LoadMeshes();

        Console.Out.WriteLine("Read mesh data from ttarchive!");
    }

    private void Back_OnClick(object sender, RoutedEventArgs e)
    {
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