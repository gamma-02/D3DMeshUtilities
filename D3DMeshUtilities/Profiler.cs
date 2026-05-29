using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using JetBrains.Annotations;

namespace D3DMeshUtilities;

public class Profiler
{

    public static readonly Profiler Instance = new Profiler();
    // public static readonly ProfilerThreadLocal ThreadInstance = new ();
    
    public readonly List<ProfilerFrame> ParentFrames = [];

    // public ProfilerFrame? CurrentFrame { get; private set; }
    
    public void BeginFrame(string name, out ProfilerFrame frame) => BeginFrame(name, null, out frame);
    
    public void BeginFrame(string name, ProfilerFrame? parent, out ProfilerFrame frame)
    {
        frame = BeginFrame(name, parent);
    }

    [MustUseReturnValue]
    public ProfilerFrame BeginFrame(string name) => BeginFrame(name, null);

    [MustUseReturnValue]
    public ProfilerFrame BeginFrame(string name, ProfilerFrame? parent)
    {
        ProfilerFrame frame;

        if (parent != null)
        {
            frame = parent.CreateChild(name);
        }
        else
        {
            frame = new ProfilerFrame(name);
            ParentFrames.Add(frame);
        }

        return frame;
    }

    public void EndFrame(ProfilerFrame frame)
    {
        
        frame.EndFrame();

        // CurrentFrame = CurrentFrame.Parent;
    }

    public void EndFrame(ProfilerFrame frame, out TimeSpan length)
    {
        length = TimeSpan.Zero;

        // if (CurrentFrame == null)
        //     return;
        
        frame.EndFrame();

        length = frame.EndTime!.Value.Subtract(frame.StartTime);
        
    }

    public string GetResults()
    {

        StringWriter stringWriter = new StringWriter();
        IndentedTextWriter writer = new IndentedTextWriter(stringWriter, "|   ");
        writer.Indent = 0;

        foreach (var parent in ParentFrames)
        {
            parent.GetResults(writer, 0);
        }

        return stringWriter.ToString();
    }

    private void MergeCompleteOther(Profiler? other)
    {
        if (other is null)
            return;
        
        this.ParentFrames.AddRange(other.ParentFrames);
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

        public void GetResults(IndentedTextWriter writer, int level)
        {

            if (EndTime == null)
            {
                throw new InvalidOperationException($"Profiler frame {Name} not ended!");

            }
            
            writer.Indent = level;
            
            writer.WriteLine($"{Name} took {Length}");
            
            
            foreach (var child in Children)
            {
                child.GetResults(writer, level + 1);
            }
        }
    }

    // public class ProfilerThreadLocal() : ThreadLocal<Profiler>(() => new Profiler(), true)
    // {
    //     protected override void Dispose(bool disposing)
    //     {
    //         Instance.MergeCompleteOther(this.Value);
    //         base.Dispose(disposing);
    //     }
    // }
}