using System.Windows;
using D3DMeshUtilities.Code;

namespace D3DMeshUtilities;

public partial class Converting : BaseProjectWindow
{

    public List<string> ModelsToConvert
    {
        get => _modelsToConvert;

        set => _modelsToConvert = value;
    }

    private List<string> _modelsToConvert;
    
    public Converting(List<string> modelsToConvert)
    {
        _modelsToConvert = modelsToConvert;
        InitializeComponent();


        if (string.IsNullOrWhiteSpace(App.StartupOutputDir)) return;
        FilePath.Text = App.StartupOutputDir;

        if (!App.StartupConvertModels) return;
        
        D3DMeshManager manager = new D3DMeshManager(modelsToConvert, App.StartupOutputDir);

        Dispatcher.InvokeAsync(manager.LoadMeshes);

        // Console.Out.WriteLine("Read mesh data from ttarchive!");


    }

    public Converting()
    {
        _modelsToConvert = new List<string>();
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

        if (_modelsToConvert.Count == 0)
        {
            Console.Out.WriteLine("No models provided!");
        }
        
        D3DMeshManager manager = new D3DMeshManager(_modelsToConvert, FilePath.Text);

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
    
    public override Window GetWindow()
    {
        return Window.ConvertModels;
    }
}