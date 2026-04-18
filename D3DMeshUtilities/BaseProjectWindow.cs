using System.Windows;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using SelectionChangedEventArgs = Avalonia.Controls.SelectionChangedEventArgs;
using Window = Avalonia.Controls.Window;

namespace D3DMeshUtilities;

public abstract class BaseProjectWindow : Window
{
    public Action onOpened;
    
    public static Dictionary<Window, Func<BaseProjectWindow>> WindowConstructorMap =
        new Dictionary<Window, Func<BaseProjectWindow>>()
        {
            { Window.LoadResources, () => new MainWindow()/*{ OverriddenOwner = GetMainWindow() }*/ },
            { Window.ListModels, () => new ArchiveModelList()/*{ OverriddenOwner = GetMainWindow() }*/ },
            { Window.ConvertModels, () => new Converting()/*{ OverriddenOwner = GetMainWindow() }*/ }
        };
    
    
    public WindowBase? OverriddenOwner
    {
        get => Owner;
        set => Owner = value;
    }

    public static bool startedUp = false;
    private bool _activate = false;
    

    protected static Avalonia.Controls.Window? GetMainWindow()
    {
        return (Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
    }
    
    protected static void SetMainWindow(Avalonia.Controls.Window window)
    {
        (Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow = window;
    }

    public enum Window
    {
        LoadResources,
        ListModels,
        ConvertModels
    }

    public abstract Window GetWindow();

    public void SetOwner(WindowBase? newOwner)
    {
        Owner = newOwner;
    }

    // protected override void OnInitialized()
    // {
    //     Console.WriteLine("wahoooooo from OnInitialized");
    //     _activate = true;
    //
    // }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        _activate = true;
        
        onOpened?.Invoke();

    }

    public void Header_OnSelectionChanged(object? sender, SelectionChangedEventArgs selectionChangedEventArgs)
    {
        
        
        // Console.WriteLine($"wahoooooo from SelectionChange, startedUp: {startedUp}");

        if (!_activate)
        {
            // _activate = true;
            return;
        }
        
        if (!(sender is TabControl control)) return;

        if (!(control.SelectedItem is TabItem item)) return;

        

        Enum.TryParse((item.Header as string)?.Replace(" ", ""), out BaseProjectWindow.Window window);

        Func<BaseProjectWindow> newWindowConstructor = WindowConstructorMap[window];

        BaseProjectWindow newWindow = newWindowConstructor();
        newWindow.SetOwner(this);

        if (this is ArchiveModelList list && newWindow is Converting c)
        {
            c.SetModelsToConvert(list.GetModelsToConvert());
        }
        
        newWindow.onOpened += OnNewWindowOpened;

        void OnNewWindowOpened()
        {
            this.Hide();

            SetMainWindow(newWindow);

            newWindow.SetOwner(null);
        
            this.Close();

            newWindow.onOpened -= OnNewWindowOpened;

        }

        newWindow.Show();
        

        // Console.WriteLine(item.Header);
        // Console.Out.WriteLine(window);
    }
}