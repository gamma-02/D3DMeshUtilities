using System.Windows;
using System.Windows.Controls;

namespace D3DMeshUtilities;

public abstract class BaseProjectWindow : Window
{
    private bool _activate = false;
    public static Dictionary<Window, Func<BaseProjectWindow>> WindowConstructorMap =
        new Dictionary<Window, Func<BaseProjectWindow>>()
        {
            { Window.LoadResources, () => new MainWindow(){ Owner = Application.Current.MainWindow } },
            { Window.ListModels, () => new ArchiveModelList(){ Owner = Application.Current.MainWindow } },
            { Window.ConvertModels, () => new Converting(){ Owner = Application.Current.MainWindow } }
        };
    
    public enum Window
    {
        LoadResources,
        ListModels,
        ConvertModels
    }

    public abstract Window GetWindow();
    
    
    public void Header_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        
        if (!(sender is TabControl control)) return;

        if (!(control.SelectedItem is TabItem item)) return;

        if (!_activate)
        {
            _activate = true;
            return;
        }

        Enum.TryParse((item.Header as string)?.Replace(" ", ""), out BaseProjectWindow.Window window);

        Func<BaseProjectWindow> newWindowConstructor = WindowConstructorMap[window];

        BaseProjectWindow newWindow = newWindowConstructor();

        if (this is ArchiveModelList list && newWindow is Converting c)
        {
            c.SetModelsToConvert(list.GetModelsToConvert());
        }
        
        newWindow.Show();
        
        this.Hide();

        Application.Current.MainWindow = newWindow;

        newWindow.Owner = null;
        
        this.Close();

        // Console.WriteLine(item.Header);
        // Console.Out.WriteLine(window);
    }
}