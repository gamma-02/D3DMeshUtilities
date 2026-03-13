using TelltaleToolKit.T3Types.Textures.T3Types;

namespace D3DMeshUtilities.Code.ImageStuffAUGH;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class PixelFormatUtility
{
    public static uint GetBitsPerPixel(PixelFormat pixelFormat)
    {
        return pixelFormat switch
        {
            PixelFormat.R32G32B32A32 => 128,
            PixelFormat.R32G32B32 => 96,

            PixelFormat.R32G32 or PixelFormat.R16G16B16A16 => 64,

            PixelFormat.R16G16B16 => 48,

            PixelFormat.R32
            or PixelFormat.R16G16
            or PixelFormat.R8G8B8A8
            or PixelFormat.B8G8R8A8
            or PixelFormat.B8G8R8X8
            or PixelFormat.R9G9B9E5
            or PixelFormat.R10G10B10A2
            or PixelFormat.R11G11B10 => 32,

            PixelFormat.R8G8B8 or PixelFormat.B8G8R8 => 24,

            PixelFormat.R16
            or PixelFormat.R8G8
            or PixelFormat.B5G5R5A1
            or PixelFormat.B5G5R5X1
            or PixelFormat.B5G6R5
            or PixelFormat.B4G4R4A4 => 16,

            PixelFormat.R8
            or PixelFormat.A8
            or PixelFormat.BC2
            or PixelFormat.BC3
            or PixelFormat.BC5
            or PixelFormat.BC6H
            or PixelFormat.BC7 => 8,

            PixelFormat.BC1 or PixelFormat.BC4 => 4,

            PixelFormat.R1 => 1,

            PixelFormat.Unknown => throw new System.NotImplementedException(),
            PixelFormat.A4B4G4R4 => 16,
            PixelFormat.L16A16 => throw new System.NotImplementedException(),
            PixelFormat.D16 => throw new System.NotImplementedException(),
            PixelFormat.CTX1 => throw new System.NotImplementedException(),
            PixelFormat.PVRTC1_2BPP_RGB => 2,
            PixelFormat.PVRTC1_4BPP_RGB => 4,
            PixelFormat.PVRTC1_2BPP_RGBA => 2,
            PixelFormat.PVRTC1_4BPP_RGBA => 4,
            PixelFormat.ATC_RGB => throw new System.NotImplementedException(),
            PixelFormat.ATC_RGBA_EXPLICIT_ALPHA => throw new System.NotImplementedException(),
            PixelFormat.ATC_RGBA_INTERPOLATED_ALPHA => throw new System.NotImplementedException(),
            PixelFormat.ETC1 => throw new System.NotImplementedException(),
            PixelFormat.ETC2_RGB => throw new System.NotImplementedException(),
            PixelFormat.ETC2_RGBA => throw new System.NotImplementedException(),
            PixelFormat.ETC2_RGB_A1 => throw new System.NotImplementedException(),
            PixelFormat.ETC2_R11 => throw new System.NotImplementedException(),
            PixelFormat.ETC2_RG11 => throw new System.NotImplementedException(),
            PixelFormat.EAC_R11 => throw new System.NotImplementedException(),
            PixelFormat.EAC_RG11 => throw new System.NotImplementedException(),
            PixelFormat.ASTC_4x4 => throw new System.NotImplementedException(),
            _ => throw new NotImplementedException(),
        };
    }

    public static uint GetBytesPerBlock(PixelFormat pixelFormat)
    {
        return pixelFormat switch
        {
            PixelFormat.BC1
            or PixelFormat.BC4
            or PixelFormat.ETC1
            or PixelFormat.ETC2_RGB
            or PixelFormat.ETC2_R11
            or PixelFormat.EAC_R11
            or PixelFormat.ASTC_4x4 => 8,
            PixelFormat.BC2
            or PixelFormat.BC3
            or PixelFormat.BC5
            or PixelFormat.BC6H
            or PixelFormat.BC7
            or PixelFormat.ETC2_RGBA
            or PixelFormat.ETC2_RGB_A1
            or PixelFormat.ETC2_RG11
            or PixelFormat.EAC_RG11 => 16,
            PixelFormat.CTX1 => 8,
            PixelFormat.PVRTC1_2BPP_RGB or PixelFormat.PVRTC1_2BPP_RGBA => 8,
            PixelFormat.PVRTC1_4BPP_RGB or PixelFormat.PVRTC1_4BPP_RGBA => 16,
            PixelFormat.ATC_RGB => 8,
            PixelFormat.ATC_RGBA_EXPLICIT_ALPHA or PixelFormat.ATC_RGBA_INTERPOLATED_ALPHA => 16,
            _ => GetBitsPerPixel(pixelFormat) / 8,
        };
    }

    public static uint GetBlockWidth(PixelFormat pixelFormat)
    {
        return pixelFormat switch
        {
            PixelFormat.BC1
            or PixelFormat.BC4
            or PixelFormat.ETC1
            or PixelFormat.ETC2_RGB
            or PixelFormat.ETC2_R11
            or PixelFormat.EAC_R11
            or PixelFormat.ASTC_4x4 => 4,
            PixelFormat.BC2
            or PixelFormat.BC3
            or PixelFormat.BC5
            or PixelFormat.BC6H
            or PixelFormat.BC7
            or PixelFormat.ETC2_RGBA
            or PixelFormat.ETC2_RGB_A1
            or PixelFormat.ETC2_RG11
            or PixelFormat.EAC_RG11 => 4,
            PixelFormat.CTX1 => 4,
            PixelFormat.PVRTC1_2BPP_RGB or PixelFormat.PVRTC1_2BPP_RGBA => 8,
            PixelFormat.PVRTC1_4BPP_RGB or PixelFormat.PVRTC1_4BPP_RGBA => 4,
            PixelFormat.ATC_RGB => 4,
            PixelFormat.ATC_RGBA_EXPLICIT_ALPHA or PixelFormat.ATC_RGBA_INTERPOLATED_ALPHA => 4,
            _ => 1,
        };
    }

    public static bool IsFormatCompressed(PixelFormat pixelFormat)
    {
        return pixelFormat switch
        {
            PixelFormat.BC1
            or PixelFormat.BC2
            or PixelFormat.BC3
            or PixelFormat.BC4
            or PixelFormat.BC5
            or PixelFormat.BC6H
            or PixelFormat.BC7
            or PixelFormat.CTX1
            or PixelFormat.PVRTC1_2BPP_RGB
            or PixelFormat.PVRTC1_4BPP_RGB
            or PixelFormat.PVRTC1_2BPP_RGBA
            or PixelFormat.PVRTC1_4BPP_RGBA
            or PixelFormat.ATC_RGB
            or PixelFormat.ATC_RGBA_EXPLICIT_ALPHA
            or PixelFormat.ATC_RGBA_INTERPOLATED_ALPHA
            or PixelFormat.ETC1
            or PixelFormat.ETC2_RGB
            or PixelFormat.ETC2_RGBA
            or PixelFormat.ETC2_RGB_A1
            or PixelFormat.ETC2_R11
            or PixelFormat.ETC2_RG11
            or PixelFormat.EAC_R11
            or PixelFormat.EAC_RG11
            or PixelFormat.ASTC_4x4 => true,
            _ => false,
        };
    }

    public static (uint rowPitch, uint slicePitch) ComputePitch(
        PixelFormat pixelFormat,
        uint width,
        uint height
    )
    {
        long slice;
        long pitch;
        if (IsFormatCompressed(pixelFormat))
        {
            // TODO: I need to add PVRTC
            uint blockWidth = Math.Max(1, (width + 3) / 4);
            uint blockHeight = Math.Max(1, (height + 3) / 4);
            uint blockBytes = GetBytesPerBlock(pixelFormat);

            pitch = blockWidth * blockBytes;
            slice = pitch * blockHeight;
        }
        else
        {
            pitch = (width * GetBitsPerPixel(pixelFormat) + 7) / 8;
            slice = pitch * height;
        }

        return ((uint)pitch, (uint)slice);
    }

    public static bool IsSRGB(PixelFormatInfo pixelFormatInfo)
    {
        return pixelFormatInfo.ColorSpace == ColorSpace.sRGB;
    }
    
    /// <summary>
    /// Get the DXGI format from a Telltale surface format. Gamma and platform type are optional.
    /// </summary>
    /// <param name="format"></param>
    /// <param name="gamma"></param>
    /// <returns>The corresponding DXGI_Format.</returns>
    public static PixelFormatInfo GetPixelFormatInfo(
        T3SurfaceFormat format,
        T3SurfaceGamma gamma = T3SurfaceGamma.Linear
    )
    {
        PixelFormat pixelFormat = format switch
        {
            T3SurfaceFormat.ARGB8 => PixelFormat.R8G8B8A8, // channel?
            T3SurfaceFormat.ARGB16 => PixelFormat.R16G16B16A16,
            T3SurfaceFormat.RGB565 => PixelFormat.B5G6R5, // channel?
            T3SurfaceFormat.ARGB1555 => PixelFormat.B5G5R5A1,
            T3SurfaceFormat.ARGB4 => PixelFormat.B4G4R4A4,
            T3SurfaceFormat.ARGB2101010 => PixelFormat.R10G10B10A2,
            T3SurfaceFormat.R16 => PixelFormat.R16, // L16
            T3SurfaceFormat.RG16 => PixelFormat.R16G16, // DXGI IS FLOAT, D3DFORMAT is UNORM
            T3SurfaceFormat.RGBA16 => PixelFormat.R16G16B16A16, // UNORM
            T3SurfaceFormat.RG8 => PixelFormat.R8G8,
            T3SurfaceFormat.RGBA8 => PixelFormat.R8G8B8A8, // channel?
            T3SurfaceFormat.R32 => PixelFormat.R32, // D3DFormat 42???? UINT??
            T3SurfaceFormat.RG32 => PixelFormat.R32G32, // UINT Yoooo
            T3SurfaceFormat.RGBA32 => PixelFormat.R32G32B32A32, // UINT LOL
            T3SurfaceFormat.R8 => PixelFormat.R8,
            T3SurfaceFormat.RGBA8S => PixelFormat.R8G8B8A8,
            T3SurfaceFormat.A8 => PixelFormat.A8,
            T3SurfaceFormat.L8 => PixelFormat.R8,
            T3SurfaceFormat.AL8 => PixelFormat.R8G8,
            T3SurfaceFormat.L16 => PixelFormat.R16,
            T3SurfaceFormat.RG16S => PixelFormat.R16G16,
            T3SurfaceFormat.RGBA16S => PixelFormat.R16G16B16A16,
            T3SurfaceFormat.R16UI => PixelFormat.R16,
            T3SurfaceFormat.RG16UI => PixelFormat.R16G16,
            T3SurfaceFormat.R16F => PixelFormat.R16, // FLOAT
            T3SurfaceFormat.RG16F => PixelFormat.R16G16,
            T3SurfaceFormat.RGBA16F => PixelFormat.R16G16B16A16, // FLOAT
            T3SurfaceFormat.R32F => PixelFormat.R32, // FLOAT
            T3SurfaceFormat.RG32F => PixelFormat.R32G32, // FLOAT
            T3SurfaceFormat.RGBA32F => PixelFormat.R32G32B32A32, // FLOAT
            T3SurfaceFormat.RGBA1010102F => PixelFormat.R10G10B10A2,
            T3SurfaceFormat.RGB111110F => PixelFormat.R11G11B10,
            T3SurfaceFormat.RGB9E5F => PixelFormat.R9G9B9E5,
            T3SurfaceFormat.DepthPCF16 => PixelFormat.D16,
            T3SurfaceFormat.DepthPCF24 => PixelFormat.D24X8,
            T3SurfaceFormat.Depth16 => PixelFormat.D16,
            T3SurfaceFormat.Depth24 => PixelFormat.D24X8,
            T3SurfaceFormat.DepthStencil32 => PixelFormat.D24S8,
            T3SurfaceFormat.Depth32F => PixelFormat.D32, // UNknown
            T3SurfaceFormat.Depth32F_Stencil8 => PixelFormat.D32S8, // UNknown
            T3SurfaceFormat.Depth24F_Stencil8 => PixelFormat.D24S8, // UNknown
            T3SurfaceFormat.BC1 => PixelFormat.BC1,
            T3SurfaceFormat.BC2 => PixelFormat.BC2,
            T3SurfaceFormat.BC3 => PixelFormat.BC3,
            T3SurfaceFormat.BC4 => PixelFormat.BC4,
            T3SurfaceFormat.BC5 => PixelFormat.BC5,
            T3SurfaceFormat.BC6 => PixelFormat.BC6H,
            T3SurfaceFormat.BC7 => PixelFormat.BC7,
            T3SurfaceFormat.ATC_RGB => PixelFormat.ATC_RGB,
            T3SurfaceFormat.ATC_RGB1A => PixelFormat.ATC_RGBA_EXPLICIT_ALPHA,
            T3SurfaceFormat.ATC_RGBA => PixelFormat.ATC_RGBA_INTERPOLATED_ALPHA,

            T3SurfaceFormat.PVRTC2 => PixelFormat.PVRTC1_2BPP_RGB,
            T3SurfaceFormat.PVRTC2a => PixelFormat.PVRTC1_2BPP_RGB,
            T3SurfaceFormat.PVRTC4 => PixelFormat.PVRTC1_4BPP_RGB,
            T3SurfaceFormat.PVRTC4a => PixelFormat.PVRTC1_4BPP_RGB,
            T3SurfaceFormat.CTX1 => PixelFormat.CTX1,
            T3SurfaceFormat.FrontBuffer => PixelFormat.B8G8R8X8,
            T3SurfaceFormat.Unknown => PixelFormat.Unknown,

            T3SurfaceFormat.ETC1_RGB => PixelFormat.ETC1,
            T3SurfaceFormat.ETC2_RGB => PixelFormat.ETC2_RGB,
            T3SurfaceFormat.ETC2_RGB1A => PixelFormat.ETC2_RGB_A1,
            T3SurfaceFormat.ETC2_RGBA => PixelFormat.ETC2_RGBA,
            T3SurfaceFormat.ETC2_R => PixelFormat.ETC2_R11,
            T3SurfaceFormat.ETC2_RG => PixelFormat.ETC2_RG11,
            T3SurfaceFormat.ASTC_RGBA_4x4 => PixelFormat.ASTC_4x4,
            T3SurfaceFormat.Count => throw new System.NotImplementedException(),
            _ => throw new System.NotImplementedException(),

            //  _ => DXGIFormat.R8G8B8A8_UNORM, // Choose R8G8B8A8 if the format is not specified. (Raw data)
        };

        var colorSpace = gamma == T3SurfaceGamma.sRGB ? ColorSpace.sRGB : ColorSpace.Linear;

        var dataType = format switch
        {
            T3SurfaceFormat.RGBA8S => DataType.Snorm,
            T3SurfaceFormat.RG16S => DataType.Snorm,
            T3SurfaceFormat.RGBA16S => DataType.Snorm,

            T3SurfaceFormat.R16UI => DataType.Uint,
            T3SurfaceFormat.RG16UI => DataType.Uint,

            T3SurfaceFormat.R32 => DataType.Uint,
            T3SurfaceFormat.RG32 => DataType.Uint,
            T3SurfaceFormat.RGBA32 => DataType.Uint,

            T3SurfaceFormat.R16F => DataType.Float,
            T3SurfaceFormat.RG16F => DataType.Float,
            T3SurfaceFormat.RGBA16F => DataType.Float,
            T3SurfaceFormat.R32F => DataType.Float,
            T3SurfaceFormat.RG32F => DataType.Float,
            T3SurfaceFormat.RGBA32F => DataType.Float,
            T3SurfaceFormat.RGBA1010102F => DataType.Float,
            T3SurfaceFormat.RGB111110F => DataType.Float,
            T3SurfaceFormat.RGB9E5F => DataType.Float,
            _ => DataType.Unorm,
        };

        return new PixelFormatInfo(pixelFormat, dataType, colorSpace);
    }
}