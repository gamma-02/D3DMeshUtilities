using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;

namespace D3DMeshUtilities;

public partial class SelectSingleArchive : Window
{

    public SelectSingleArchive()
    {
        InitializeComponent();
    }
    
    public SelectSingleArchive(List<string> archives)
    {
        InitializeComponent();
        
        ArchiveList.Items.Clear();
        
        foreach (string archivePath in archives)
        {
            var li = new ListBoxItem();
            var text = new TextBlock();

            int seperator = archivePath.LastIndexOfAny(['\\', '/']);

            string archiveName = archivePath.Substring(seperator + 1);

            text.Text = archiveName;
            text.HorizontalAlignment = HorizontalAlignment.Left;
            li.Content = text;

            ArchiveList.Items.Add(li);
        }
        
        
    }

    private void ArchiveList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedItems = ArchiveList.SelectedItems;

        if (selectedItems == null || selectedItems.Count == 0)
        {
            OpenArchiveGrid.IsVisible = false;
        }
        else
        {
            OpenArchiveGrid.IsVisible = true;
        }
    }

    //todo: archiveName -> archiveNames (list,str)
    private void FinishDlog(object? sender, RoutedEventArgs e)
    {
        if (ArchiveList.SelectedItem is not ListBoxItem selectedItem)
            return;

        string archiveName = ((TextBlock)selectedItem.Content!).Text ?? "";

        Close(archiveName);
    }
}