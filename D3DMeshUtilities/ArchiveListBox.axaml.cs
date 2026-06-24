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
    
    public ObservableCollection<MeshEntry> MeshEntries { get; set; }
    
    public string ArchiveName { get; set; }

    public bool IsChecked
    {
        get => this.Get<Expander>("Expander").IsExpanded;
        set => this.Get<Expander>("Expander").IsExpanded = value;
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

    public TextBlock GetHeaderText()
    {
        return (this.Get<Expander>("Expander").Header as TextBlock)!;
    }

    
    
}