using System.Runtime.InteropServices;
using SkiaSharp;
using ZXing;
using ZXing.QrCode;
using ZXing.QrCode.Internal;

namespace Api.Extensions;

/// <summary>
/// Interface for QR code generation functionality
/// </summary>
public interface IQrCodeGenerator
{
    /// <summary>
    /// Creates a QR code image from the provided text and returns it as a stream
    /// </summary>
    /// <param name="text">The text to encode in the QR code</param>
    /// <returns>MemoryStream containing the QR code image</returns>
    Stream GenerateQrCodeStream(string text);

    /// <summary>
    /// Creates a QR code image with customized parameters and returns it as a stream
    /// </summary>
    /// <param name="text">The text to encode in the QR code</param>
    /// <param name="size">Size of the QR code image in pixels</param>
    /// <param name="errorCorrectionLevel">Error correction level</param>
    /// <returns>MemoryStream containing the QR code image</returns>
    Stream GenerateQrCodeStream(string text, int size, ErrorCorrectionLevel errorCorrectionLevel);

    /// <summary>
    /// Creates a QR code image from the provided text and saves it to the specified file path
    /// </summary>
    /// <param name="text">The text to encode in the QR code</param>
    /// <param name="filePath">File path to save the QR code image</param>
    void GenerateQrCodeFile(string text, string filePath);
}

/// <summary>
/// Implementation of the QR code generator interface
/// </summary>
public class QrCodeGenerator : IQrCodeGenerator
{
    /// <summary>
    /// Creates a QR code image from the provided text and returns it as a stream
    /// </summary>
    /// <param name="text">The text to encode in the QR code</param>
    /// <returns>MemoryStream containing the QR code image</returns>
    public Stream GenerateQrCodeStream(string text)
    {
        // Use default size and error correction level
        return GenerateQrCodeStream(text, 300, ErrorCorrectionLevel.M);
    }

    /// <summary>
    /// Creates a QR code image with customized parameters and returns it as a stream
    /// </summary>
    /// <param name="text">The text to encode in the QR code</param>
    /// <param name="size">Size of the QR code image in pixels</param>
    /// <param name="errorCorrectionLevel">Error correction level</param>
    /// <returns>MemoryStream containing the QR code image</returns>
    public Stream GenerateQrCodeStream(string text, int size, ErrorCorrectionLevel errorCorrectionLevel)
    {
        if (string.IsNullOrEmpty(text))
            throw new ArgumentException("Text cannot be null or empty", nameof(text));

        // Create a pixel data writer
        var writer = new BarcodeWriterPixelData
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                DisableECI = true,
                CharacterSet = "UTF-8",
                Width = size,
                Height = size,
                ErrorCorrection = errorCorrectionLevel,
                Margin = 2
            }
        };

        // Generate QR code as pixel data
        var pixelData = writer.Write(text);

        using (var bitmap =
               new SKBitmap(pixelData.Width, pixelData.Height, SKColorType.Rgba8888, SKAlphaType.Premul))
        {
            var handle = GCHandle.Alloc(pixelData.Pixels, GCHandleType.Pinned);
            try
            {
                bitmap.SetPixels(handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }

            using (var image = SKImage.FromBitmap(bitmap))
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            {
                var memoryStream = new MemoryStream();
                data.SaveTo(memoryStream);
                memoryStream.Position = 0;
                return memoryStream;
            }
        }
    }

    /// <summary>
    /// Creates a QR code image from the provided text and saves it to the specified file path
    /// </summary>
    /// <param name="text">The text to encode in the QR code</param>
    /// <param name="filePath">File path to save the QR code image</param>
    public void GenerateQrCodeFile(string text, string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        using var stream = GenerateQrCodeStream(text);
        using var fileStream = new FileStream(filePath, FileMode.Create);
        stream.CopyTo(fileStream);
    }
}