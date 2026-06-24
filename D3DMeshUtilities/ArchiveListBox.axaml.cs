using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;

namespace D3DMeshUtilities;

public class MeshEntry(string MeshName)
{
    public string MeshName { get; set; } = MeshName;
}

public class ArchiveListBox : TemplatedControl
{
    public ListBox? ArchiveList = null;
    public Expander? Expander = null;
    public HashSet<string> MeshesToTrySelect = [];
    
    public ObservableCollection<MeshEntry> MeshEntries { get; set; }
    
    public string ArchiveName { get; set; }

    public bool IsChecked
    {
        get => Expander is { IsExpanded: true };
        set => Expander?.IsExpanded = value;
    }

    public bool ShowExpand { get; set; } = true;

    public ArchiveListBox(string name) : this(name, "Item 1", "Item 2", "Item 3") { } //use some test values lol
    
    public ArchiveListBox(string name, params string[] meshes) : this(name, meshes.Select(mesh => new MeshEntry(mesh)).ToList()) {}

    public ArchiveListBox(string name, List<string> meshes)
    {
        ArchiveName = name;
        MeshEntries = new ObservableCollection<MeshEntry>(meshes.Select(mesh => new MeshEntry(mesh)).ToList());
        
        DataContext = this;
        
    }

    public ArchiveListBox(string name, List<MeshEntry> meshes) : this(name, new ObservableCollection<MeshEntry>(meshes)) { }

    public ArchiveListBox(string name, ObservableCollection<MeshEntry> meshes)
    {
        MeshEntries = meshes;
        ArchiveName = name;

        DataContext = this;
    }

    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _templateApplied = true;

        var box = e.NameScope.Get<ListBox>("ArchiveList");
        
        foreach (EventHandler<SelectionChangedEventArgs>? eventHandler in _eventHandlers)
        {
            box.SelectionChanged += eventHandler;
        }

        ArchiveList = box;
        Expander = e.NameScope.Get<Expander>("Expander");

        List<object?> selected = [];
        foreach (object? o in box.Items)
        {
            if(o is not MeshEntry entry ) continue;
            if (!MeshesToTrySelect.Contains(entry.MeshName)) continue; // so if it does contains, do below
            
            selected.Add(o);
        }

        if (selected.Count != 0)
        {
            box.SelectedItems = selected;
        }
    }

    public TextBlock GetHeaderText()
    {
        return (this.Get<Expander>("Expander").Header as TextBlock)!;
    }

    public List<string> GetSelectedMeshes()
    {
        
        List<string> meshes = [];

        if (ArchiveList?.SelectedItems == null)
        {
            return meshes;
        }

        foreach (object? item in ArchiveList.SelectedItems)
        {
            switch (item)
            {
                case MeshEntry entry:
                    meshes.Add(entry.MeshName);
                    break;
                case TextBox { Text: null }:
                    continue;
                case TextBox box:
                    meshes.Add(box.Text);
                    break;
            }
        }

        return meshes;
    }

    public IList? GetSelectedItems()
    {
        return ArchiveList?.SelectedItems;
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
                ArchiveList?.SelectionChanged += value;
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
                ArchiveList?.SelectionChanged -= value;
            }
        } 
    }
    
}