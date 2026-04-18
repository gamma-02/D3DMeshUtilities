namespace D3DMeshUtilities.Code;

/// <summary>
/// This class is designed to contain implementations for writing any GFXPlatformFormat tuple
/// to a byte array.
///
/// <see cref="TelltaleToolKit.T3Types.Meshes.T3Types.GFXPlatformFormat"/>
/// </summary>
public class ConvertToGFXPlatformFormat
{
    
    /*
    For my reference, all of the GFX formats:
`None,
  F32,
  F32x2,
  F32x3,
  F32x4,
  F16x2,
  F16x4,
  S32,
  U32,
  S32x2,
  U32x2,
  S32x3,
  U32x3,
  S32x4,
  U32x4,
  S16,
  U16,
  S16x2,
  U16x2,
  S16x4,
  U16x4,
  SN16,
  UN16,
  SN16x2,
  UN16x2,
  SN16x4,
  UN16x4,
  S8,
  U8,
  S8x2,
  U8x2,
  S8x4,
  U8x4,
  SN8,
  UN8,
  SN8x2,
  UN8x2,
  SN8x4,
  UN8x4,
  SN10_SN11_SN11,
  SN10x3_SN2,
  UN10x3_UN2,
  D3DCOLOR,
     */

    public bool WriteF32(Span<byte> span, float value)
    {
        byte[] val = BitConverter.GetBytes(value);

        try
        {
            val.CopyTo(span);
        }
        catch (ArgumentException)
        {
            return false;
        }

        return true;
    }
}