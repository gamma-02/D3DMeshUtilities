using System.IO;

namespace D3DMeshUtilities.Code.ImageStuffAUGH;


public interface IImageCodec
{
    string Name { get; } // Codec name (e.g., "PNG Codec")
    string FormatName { get; } // Format name (e.g., "Portable Network Graphics")
    string[] SupportedExtensions { get; } // Supported file extensions
    static PixelFormatInfo[]? SupportedPixelFormats { get; } // Supported pixel formats

    // Core operations
    public Texture LoadFromMemory(byte[] input, CodecOptions options);

    public Texture LoadFromFile(string filePath, CodecOptions options)
    {
        var bytes = File.ReadAllBytes(filePath);
        return LoadFromMemory(bytes, options);
    }

    public byte[] SaveToMemory(Texture input, CodecOptions options);

    public void SaveToFile(string filePath, Texture input, CodecOptions options)
    {
        var bytes = SaveToMemory(input, options);
        File.WriteAllBytes(filePath, bytes);
    }

    bool IsSupportedPixelFormat(PixelFormatInfo format)
    {
        return SupportedPixelFormats.Contains(format);
    }
}


public class Codecs
{
    
}