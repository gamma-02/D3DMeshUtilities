using System;
using System.Collections.Generic;
using System.Windows;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using SelectionChangedEventArgs = Avalonia.Controls.SelectionChangedEventArgs;
using Window = Avalonia.Controls.Window;

namespace D3DMeshUtilities;

public abstract class BaseProjectWindow : Window
{
    new public Action? Opened;
    
    public static Dictionary<Window, Func<BaseProjectWindow>> WindowConstructorMap =
        new Dictionary<Window, Func<BaseProjectWindow>>()
        {
            { Window.LoadResources, () => new MainWindow()/*{ OverriddenOwner = GetMainWindow() }*/ },
            { Window.ListModels, () => new ArchiveModelList(true)/*{ OverriddenOwner = GetMainWindow() }*/ },
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

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        _activate = true;
        
        Opened?.Invoke();

    }

    protected void CloseOnNewWindowOpened(BaseProjectWindow newWindow)
    {
        void _internalOnNewWindowOpened()
        {
            //screw parenting, manually set positon/size/state (should also account for monitors)
            newWindow.ClientSize = this.ClientSize;
            newWindow.Position = this.Position;
            newWindow.WindowState = this.WindowState;
            
            if(_activate)
                this.Hide();
            else
                this.Opened += Close;
            
            SetMainWindow(newWindow);

            newWindow.SetOwner(null);

            if (_activate)
                this.Close();
            
            newWindow.Opened -= _internalOnNewWindowOpened;
        }

        newWindow.Opened += _internalOnNewWindowOpened;
        
        newWindow.Show();

    }

    public void Header_OnSelectionChanged(object? sender, SelectionChangedEventArgs selectionChangedEventArgs)
    {
        if (!_activate)
        {
            return;
        }
        
        if (!(sender is TabControl control)) return;

        if (!(control.SelectedItem is TabItem item)) return;

        Enum.TryParse((item.Header as string)?.Replace(" ", ""), out BaseProjectWindow.Window window);

        control.IsEnabled = false;

        Func<BaseProjectWindow> newWindowConstructor = WindowConstructorMap[window];

        BaseProjectWindow newWindow = newWindowConstructor();
        newWindow.SetOwner(this);

        if (this is ArchiveModelList list && newWindow is Converting c)
        {
            c.SetModelsToConvert(list.GetModelsToConvert());
        }
        
        CloseOnNewWindowOpened(newWindow);

        // Console.WriteLine(item.Header);
        // Console.Out.WriteLine(window);
    }
}