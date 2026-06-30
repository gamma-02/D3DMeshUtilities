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

    private bool _hasExpandedDropdown = false;
    private bool _skipNext = false;
        
    private void Header_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_skipNext)
        {
            _skipNext = false;
            return;
        }
        
        if(sender is TabControl control)
        {
            bool isComboBox = (control.Items[control.SelectedIndex] as TabItem)?.Header is ComboBox;

            if (control.SelectedIndex == _state.SelectedTab && !isComboBox) return;

            if (isComboBox && !_hasExpandedDropdown)
            {
                _hasExpandedDropdown = true;
                control.SelectedIndex = _state.SelectedTab; //return to initial state since we don't want to chagne it
                // _skipNext = true; //skip the selection changed event from switching back
                return;
            }
        }
        else if (sender is ComboBox)
        {
            Console.Out.WriteLine("Got ComboBox!");
        }
        else
            return;
        
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
            IntermediateDropdown.Items.Add(listModelsTab);
        

        IntermediateDropdown.SelectedIndex = state.SelectedListTab;
        
        Header.SelectedIndex = state.SelectedTab;
        
        Header.SelectionChanged += Header_OnSelectionChanged;
        
    }

    private bool _selectionNotMade = false;
    
    private void IntermediateDropdown_OnSelectionChanged(object? senderTmp, SelectionChangedEventArgs e)
    {
        ComboBox sender = (senderTmp as ComboBox)!;
        _selectionNotMade = false;
        _state.SelectedListTab = sender.SelectedIndex;
        if(sender.IsInitialized)
        {
            _hasExpandedDropdown = true;
            Header_OnSelectionChanged(sender, e);
            _skipNext = true;
        }
    }
    
    private void IntermediateDropdown_OnDropDownClosed(object? senderTmp, EventArgs e)
    {
        ComboBox sender = (senderTmp as ComboBox)!;

        if (!sender.IsInitialized)
            return;

        if (!_selectionNotMade) return; //selection was made, return
        
        Header_OnSelectionChanged(sender, null!);
        _skipNext = true;
    }

    private void IntermediateDropdown_OnDropDownOpened(object? sender, EventArgs e)
    {
        _selectionNotMade = true;
    }
}