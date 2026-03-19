using System.Windows;
using System.Windows.Controls;

namespace D3DMeshUtilities;

public abstract class BaseProjectWindow : Window
{
    
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

        Enum.TryParse<BaseProjectWindow.Window>((item.Header as string)?.Replace(" ", ""), out BaseProjectWindow.Window window);
        
        Console.WriteLine(item.Header);
        Console.Out.WriteLine(window);
    }
}