using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using TelltaleToolKit.T3Types.Animations;

namespace D3DMeshUtilities;

public partial class WindowTabs : UserControl
{
    private readonly List<EventHandler<SelectionChangedEventArgs>?> _listeners = [];
    public event EventHandler<SelectionChangedEventArgs>? SelectionChanged
    {
        add => _listeners.Add(value);
        remove => _listeners.Remove(value);
    }

    private TabsState _state;

    public WindowTabs() : this(new TabsState {ListModelsDropDown = ["List Models", "List Agents"], SelectedListTab = 0})
    {
        
    }

    private int _count = 0;
    private void Header_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        Console.Out.WriteLine($"Changing tabs... {_count += 1}");

        foreach (EventHandler<SelectionChangedEventArgs>? handler in _listeners)
        {
            handler?.Invoke(sender, e);
        }
    }

    public WindowTabs(TabsState state)
    {
        InitializeComponent();
        _state = state;
        
        foreach (string listModelsTab in state.ListModelsDropDown)
        {

            IntermediateDropdown.Items.Add(listModelsTab);
            Console.Out.WriteLine(listModelsTab);

        }

        IntermediateDropdown.SelectedIndex = state.SelectedListTab;
        
        Header.SelectedIndex = state.SelectedTab;
        
        Header.SelectionChanged += Header_OnSelectionChanged;


    }
    
    private void IntermediateDropdown_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _state.SelectedListTab = (sender as ComboBox)!.SelectedIndex;
    }
}