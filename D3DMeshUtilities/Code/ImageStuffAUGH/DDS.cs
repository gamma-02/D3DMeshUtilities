using Hexa.NET.DirectXTex;

namespace D3DMeshUtilities.Code.ImageStuffAUGH;

public class DdsCodec : IImageCodec
{
    public static DdsCodec Codec { get; private set; } = new DdsCodec();
    
    public string Name => "DDS Codec";
    public string FormatName => "DirectDraw Surface";
    public string[] SupportedExtensions => [".dds"];

    public static PixelFormatInfo[] SupportedPixelFormats => []; // Too many to list. We'll use the GetDXGIFormat to find all supported formats.

    public unsafe byte[] SaveToMemory(Texture input, CodecOptions options)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (DirectXTexUtility.GetDXGIFormat(input.Metadata.PixelFormatInfo) == DXGIFormat.UNKNOWN)
        {
            input.ConvertToRGBA8(); // Attempt to convert to a supported format.
        }

        ScratchImage newImage = DirectXTexUtility.CreateScratchImageFromTexture(input);
        Blob blob = DirectXTex.CreateBlob();

        DDSFlags flags = DDSFlags.None;

        if (options.ForceDx9Legacy)
        {
            flags = DDSFlags.ForceDx9Legacy;
        }

        try
        {
            var metadata = newImage.GetMetadata();

            DirectXTex
                .SaveToDDSMemory2(
                    newImage.GetImages(),
                    newImage.GetImageCount(),
                    ref metadata,
                    flags,
                    ref blob
                )
                .ThrowIf();

            return DirectXTexUtility.GetBytesFromBlob(blob);
        }
        finally
        {
            blob.Release();
            newImage.Release();
        }
    }

    public Texture LoadFromMemory(byte[] data, CodecOptions options)
    {
        ScratchImage scratchImage = DirectXTex.CreateScratchImage();
        Hexa.NET.DirectXTex.TexMetadata texMetadata = new();

        Texture texture;

        unsafe
        {
            fixed (byte* pData = data)
            {
                var res = DirectXTex.LoadFromDDSMemory(
                    pData,
                    (nuint)data.Length,
                    DDSFlags.None,
                    ref texMetadata,
                    ref scratchImage
                );

                if (res.IsFailure)
                {
                    scratchImage.Release();
                    res.Throw();
                }
            }
        }

        texture = DirectXTexUtility.LoadFromScratchImage(scratchImage);

        scratchImage.Release();

        return texture;
    }
}