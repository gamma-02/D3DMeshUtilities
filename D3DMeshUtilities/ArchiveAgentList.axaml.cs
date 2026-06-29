using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using D3DMeshUtilities.Code.D3DMeshFormats;
using TelltaleToolKit.Meta.Reflection;
using TelltaleToolKit.T3Types;
using TelltaleToolKit.T3Types.Meshes;
using TelltaleToolKit.T3Types.Properties;

namespace D3DMeshUtilities;

public partial class ArchiveAgentList : BaseProjectWindow
{

    public static readonly Dictionary<string, List<AgentRepresentation>> ContextAgentDictionary = new ();

    public static Action? OnAgentDictionaryBuilt;

    private List<string>? _specificArchives;
    
    private bool _enableButton = true;
    
    public ArchiveAgentList()
    {
        InitializeComponent();

        // TestBox.Items.Add("HELLO");
        // TestBox.SelectedIndex = 0;

        TabsState.SelectedTab = 2;//set index to our window "id"
        
        if (Design.IsDesignMode)
        {
            List<AgentRepresentation> agents = 
            [
                new AgentRepresentation(["test 1", "test 2", "test 3"], ["test 1", "test 3"], "Test Agent Rep 1"),
                new AgentRepresentation(["test 4", "test 5", "test 6"], ["test 5", "test 4"], "Test Agent Rep 2")
            ];
            
            ListPanel.Children.Clear();
            
            ArchiveAgentListBox testBox = new ArchiveAgentListBox("Ex Archive Name", agents);
            ListPanel.Children.Add(testBox);

            return;
        }
        
        if(ContextAgentDictionary.Count == 0)
        {
            ListPanel.Children.Clear();
            
            TextBlock notLoadedMessage;
            
            if(AsyncSearchForSkeletonFiles.BuildDictionaryTask == null)
            {
                notLoadedMessage = new TextBlock
                {
                    Text = "Archive not yet loaded :(",
                    FontSize = 20,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(10)
                };
            }
            else
            {
                notLoadedMessage = new TextBlock
                {
                    Text = "Agent Dictionary building, please wait...",
                    FontSize = 20,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(10)
                };
            }

            ListPanel.Children.Add(notLoadedMessage);
            
            
            
            OnAgentDictionaryBuilt += FillAgentList;
        
            return;
        
        }
        
        FillAgentList();
        
    }
    
    public ArchiveAgentList(List<string> specificArchives)
    {
        InitializeComponent();

        _specificArchives = specificArchives;
        
        // Agents = agents;
        if(ContextAgentDictionary.Count == 0)
        {
            ListPanel.Children.Clear();
            
            var notLoadedMessage = new TextBlock
            {
                Text = "Agent Dictionary building, please wait...",
                FontSize = 20,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(10)
            };
        
            ListPanel.Children.Add(notLoadedMessage);
            
            OnAgentDictionaryBuilt += FillAgentList;
            return;
        
        }
        
        FillAgentList();
    }

    public void FillAgentList()
    {
        List<string> contextsToScanThrough = _specificArchives ?? ContextAgentDictionary.Keys.ToList();
        
        ListPanel.Children.Clear();
        
        foreach (string contextName in contextsToScanThrough)
        {
            if(!ContextAgentDictionary.TryGetValue(contextName, out List<AgentRepresentation>? agents)) continue;
            
            var archiveAgentListBox = new ArchiveAgentListBox(contextName, agents);
            archiveAgentListBox.SelectionChanged += ArchiveAgentListBox_OnSelectionChanged;
            ListPanel.Children.Add(archiveAgentListBox);
        }

        if (ListPanel.Children.Count == 0)
        {
            ListPanel.Children.Clear();
            
            var notLoadedMessage = new TextBlock
            {
                Text = "No agents found in archive!",
                FontSize = 20,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(10)
            };
        
            ListPanel.Children.Add(notLoadedMessage);
            
        }
        
    }

    private void ArchiveAgentListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (IsNothingSelected() || !_enableButton)
        {
            ConvertButtonGrid.IsVisible = false;
        }
        else
        {
            ConvertButtonGrid.IsVisible = true;
        }
    }

    public bool IsNothingSelected()
    {
        foreach (Control? child in ListPanel.Children)
        {
            if(child is not ArchiveAgentListBox agentListBox) continue;

            if (agentListBox.GetSelectedItems() is not { Count: 0 })
            {
                return false;
            }
        }

        return true;
    }
    
    public List<AgentRepresentation> GetSelectedAgents()
    {
        List<AgentRepresentation> meshes = [];

        if (IsNothingSelected()) return meshes;

        foreach (Control? child in ListPanel.Children)
        {
            if(child is not ArchiveAgentListBox agentListBox) continue;
            
            if(agentListBox.GetSelectedItems() is { Count: 0 }) continue; //slightly faster, checks for empty
            
            meshes.AddRange(agentListBox.GetSelectedAgents());
        }

        return meshes;
    }

    public override Window GetWindow()
    {
        return Window.ListAgents;
    }

    private void BeginConversion(object? sender, RoutedEventArgs e)
    {
        List<AgentRepresentation> agentRepresentations = GetSelectedAgents();
        
        if(agentRepresentations.Count == 0) return;
        
        var task = new AgentConversionTask(agentRepresentations);

        var converting = new Converting(task) { OverriddenOwner = this };
        CloseOnNewWindowOpened(converting);
    }

    public static async void BuildArchiveAgentDictionary()
    {
        try
        {
            //make sure the dictionaries are built
            while (!AsyncSearchForSkeletonFiles.BuiltDictionary)
            {
                await Task.Yield();
            }

            Profiler.Instance.BeginFrame("Building Agent Dictionary", out Profiler.ProfilerFrame frame);

            foreach (string contextName in AsyncSearchForSkeletonFiles.AgentPropertiesByContextName.Keys)
            {
                List<AgentRepresentation> representations =
                    AsyncSearchForSkeletonFiles.AgentPropertiesByContextName[contextName]!
                        .Select(set => new AgentRepresentation(set.set, set.name.ToString())).ToList();
                
                representations.Sort((a, b) => string.CompareOrdinal(a.PropertySetName, b.PropertySetName));

                ContextAgentDictionary[contextName] = representations;
            }
            
            Profiler.Instance.EndFrame(frame, out TimeSpan length);
            
            Avalonia.Threading.Dispatcher.UIThread.Invoke(() => OnAgentDictionaryBuilt?.Invoke());
            
            await Console.Out.WriteLineAsync($"\tFinsihed building AgentRepresentation dictionary in: {length}");

        }
        catch (Exception)
        {
            return;
        }
    }

    public class AgentConversionTask(List<AgentRepresentation> agentRepresentations) : Converting.ConversionTask
    {
        
        
        public override bool ValidateTask(Converting? converting)
        {
            if (agentRepresentations.Count != 0) return true;
            
            Console.Out.WriteLine("No agents provided!");
            converting?.AddMessageToBox("No agents provided!");

            return false;

        }

        public override void Convert(string filePath, Converting? converting, Action completeTaskAction)
        {
            converting?.Dispatcher.Invoke(() => converting.AddMessageToBox("Converting..."));

            Profiler.Instance.BeginFrame("Converting Agents", out Profiler.ProfilerFrame agentConversionFrame);
                
            foreach (AgentRepresentation agent in agentRepresentations)
            {
                ObservableCollection<MeshVisibility> meshes = agent.MeshAndVisibility;

                List<Handle<D3DMesh>> meshesToConvert = [];
                
                meshesToConvert.AddRange(from mesh in meshes where mesh.Visible select mesh.Mesh);

                agent.SaveMeshes(filePath, converting, agentConversionFrame, meshesToConvert);
            }
            
            Profiler.Instance.EndFrame(agentConversionFrame, out TimeSpan span);

            Task.Run(() => converting?.Dispatcher.Invoke(completeTaskAction));
            
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Out.WriteLine($"Finished converting agents! Took: {span}");
            Console.ResetColor();

            converting?.Dispatcher.Invoke(() => converting.AddMessageToBox("Finished converting agents!"));

        }
    }
}