using Avalonia.Controls;
using Avalonia.Input;

namespace D3DMeshUtilities;

public class NonSelectableListBox : ListBox
{
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        //overridden
    }
}