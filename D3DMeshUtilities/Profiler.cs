using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;

namespace D3DMeshUtilities;

public class Profiler
{

    public static readonly Profiler Instance = new Profiler();
    
    public readonly List<ProfilerFrame> ParentFrames = [];

    public ProfilerFrame? CurrentFrame { get; private set; }

    public void BeginFrame(string name)
    {
        ProfilerFrame frame;

        if (CurrentFrame != null)
        {
            frame = CurrentFrame.CreateChild(name);
        }
        else
        {
            frame = new ProfilerFrame(name);
            ParentFrames.Add(frame);
        }

        CurrentFrame = frame;
    }

    public void EndFrame()
    {

        if (CurrentFrame == null)
            return;
        
        CurrentFrame.EndFrame();

        CurrentFrame = CurrentFrame.Parent;
    }

    public void EndFrame(out TimeSpan length)
    {
        length = TimeSpan.Zero;

        if (CurrentFrame == null)
            return;
        
        CurrentFrame.EndFrame();

        length = CurrentFrame.EndTime!.Value.Subtract(CurrentFrame.StartTime);

        CurrentFrame = CurrentFrame.Parent;

    }

    public string GetResults()
    {
        if (CurrentFrame != null)
        {
            throw new InvalidOperationException($"Profiler frame {CurrentFrame.Name} not ended!");
        }

        StringWriter stringWriter = new StringWriter();
        IndentedTextWriter writer = new IndentedTextWriter(stringWriter);

        foreach (var parent in ParentFrames)
        {
            parent.GetResults(writer);
        }

        return stringWriter.ToString();
    }
    
    public class ProfilerFrame(string name)
    {
        public string Name = name;
        public ProfilerFrame? Parent;
        public readonly List<ProfilerFrame> Children = [];
        public DateTime StartTime { get; private set; } = DateTime.Now;
        public DateTime? EndTime;

        public TimeSpan? Length
        {
            get
            {
                if (!EndTime.HasValue)
                    return null;

                return EndTime.Value.Subtract(StartTime);
            }
        }

        public ProfilerFrame(string name, ProfilerFrame parent) : this(name)
        {
            Parent = parent;
            parent.AddChild(this);
        }

        public void AddChild(ProfilerFrame frame)
        {
            Children.Add(frame);
        }

        public ProfilerFrame CreateChild(string name)
        {
            return new ProfilerFrame(name, this); 
        }

        public void EndFrame()
        {
            EndTime = DateTime.Now;
        }

        public void GetResults(IndentedTextWriter writer)
        {
            writer.WriteLine($"{Name} took {Length}");
            writer.Indent += 1;
            
            foreach (var child in Children)
            {
                child.GetResults(writer);
            }

            writer.Indent -= 1;
        }
    }
}