using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Xunit.Abstractions;
using SelectionChangedEventArgs = Avalonia.Controls.SelectionChangedEventArgs;
using Window = Avalonia.Controls.Window;

namespace D3DMeshUtilities;

public class TabsState
{
    public Dictionary<BaseProjectWindow.Window, ITabState> TabStates = [];

    public List<string> ListModelsDropDown = ["List Models", "List Agents"];
    public int SelectedListTab = 0;
    public int SelectedTab = 0;
}

public interface ITabState;

public abstract class BaseProjectWindow : Window
{
    
    public static TabsState TabsState = new();
    
    
    public new Action? Opened;
    
    
    public static Dictionary<Window, Func<BaseProjectWindow>> WindowConstructorMap =
        new ()
        {
            { Window.LoadResources, () => new MainWindow()/*{ OverriddenOwner = GetMainWindow() }*/ },
            { Window.ListModels, () => new ArchiveModelList(true)/*{ OverriddenOwner = GetMainWindow() }*/ },
            { Window.ListAgents, () => new ArchiveAgentList() },
            { Window.ConvertModels, () => new Converting()/*{ OverriddenOwner = GetMainWindow() }*/ }
        };
    
    
    public WindowBase? OverriddenOwner
    {
        get => Owner;
        set => Owner = value;
    }

    public static bool startedUp = false;
    private bool _activate;

    protected static void SetMainWindow(Avalonia.Controls.Window window)
    {
        (Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow = window;
    }

    public enum Window
    {
        LoadResources,
        ListModels,
        ListAgents,
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
        
        // return;
        
        if (!(sender is TabControl control)) return;

        if (!(control.SelectedItem is TabItem item)) return;

        if(Enum.TryParse((item.Header as string)?.Replace(" ", ""), out Window window))
        {
            control.IsEnabled = false;

            TabsState.SelectedListTab = control.SelectedIndex;

            Func<BaseProjectWindow> newWindowConstructor = WindowConstructorMap[window];

            BaseProjectWindow newWindow = newWindowConstructor();
            newWindow.SetOwner(this);

            if (this is ArchiveModelList list && newWindow is Converting c)
            {
                c.SetTask(new ArchiveModelList.MeshConversionTask(list.GetModelsToConvert()));
            }

            CloseOnNewWindowOpened(newWindow);
            
        } else if (item.Header is ComboBox box && Enum.TryParse(box.Text?.Replace(" ", ""), out Window window1))
        {
            control.IsEnabled = false;
            
            TabsState.SelectedListTab = control.SelectedIndex;

            Func<BaseProjectWindow> newWindowConstructor = WindowConstructorMap[window];

            BaseProjectWindow newWindow = newWindowConstructor();
            newWindow.SetOwner(this);

            if (this is ArchiveModelList list && newWindow is Converting c)
            {
                c.SetTask(new ArchiveModelList.MeshConversionTask(list.GetModelsToConvert()));
            }

            CloseOnNewWindowOpened(newWindow);
        }

        // Console.WriteLine(item.Header);
        // Console.Out.WriteLine(window);
    }
}