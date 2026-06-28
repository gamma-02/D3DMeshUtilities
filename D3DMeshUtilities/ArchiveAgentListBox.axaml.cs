using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using D3DMeshUtilities.Code.D3DMeshFormats;

namespace D3DMeshUtilities;

public class ArchiveAgentListBox : TemplatedControl
{
    public ObservableCollection<AgentRepresentation> Agents { get; set; }
    public string ArchiveName { get; set; }
    
    public ArchiveAgentListBox()
    {
        ArchiveName = "Test Archive - Empty Constructor";
        Agents =
        [
            new AgentRepresentation(["test 1", "test 2", "test 3"], ["test 1", "test 3"], "Test Agent Rep 1"),
            new AgentRepresentation(["test 4", "test 5", "test 6"], ["test 5", "test 4"], "Test Agent Rep 2")
        ];
    }
    
    public ListBox? AgentList = null;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        AgentList = e.NameScope.Get<ListBox>("AgentList");
        
        foreach (EventHandler<SelectionChangedEventArgs>? eventHandler in _eventHandlers)
        {
            AgentList.SelectionChanged += eventHandler;
        }
        
        // foreach (Visual visual in AgentList.GetVisualDescendants())
        // {
        //     Console.Out.Write(visual);
        // }

        AgentList.Initialized += OnAgentListInit;

        AgentList.Loaded += OnAgentListInit;

    }

    private void OnAgentListInit(object? sender, EventArgs e)
    {
        if(sender is not ListBox box) return;
        
        foreach (Visual visual in box.GetVisualDescendants())
        {
            if (visual is not Expander ex) continue;
            
            ex.Expanded += OnExpanderExpanded;
        }
    }

    private void OnExpanderExpanded(object? sender, RoutedEventArgs e)
    {
        Console.Out.WriteLine($"{sender} expanded!");
    }

    public ArchiveAgentListBox(string archiveName, List<AgentRepresentation> agents)
    {
        ArchiveName = archiveName;
        Agents = new ObservableCollection<AgentRepresentation>(agents);
    }

    public List<AgentRepresentation> GetSelectedAgents()
    {
        
        List<AgentRepresentation> agents = [];

        if (AgentList?.SelectedItems == null)
        {
            return agents;
        }

        foreach (object? item in AgentList.SelectedItems)
            if (item is AgentRepresentation mesh) agents.Add(mesh);

        return agents;
    }

    public IList? GetSelectedItems()
    {
        return AgentList?.SelectedItems;
    }

    private bool _templateApplied = false;
        
    private readonly List<EventHandler<SelectionChangedEventArgs>?> _eventHandlers = [];
    public event EventHandler<SelectionChangedEventArgs>? SelectionChanged
    {
        add {
            if(!_templateApplied)
            {
                _eventHandlers.Add(value);
            }
            else
            {
                AgentList?.SelectionChanged += value;
            }
        }
        remove
        {
            if(!_templateApplied)
            {
                _eventHandlers.Remove(value);
            }
            else
            {
                AgentList?.SelectionChanged -= value;
            }
        } 
    }
}