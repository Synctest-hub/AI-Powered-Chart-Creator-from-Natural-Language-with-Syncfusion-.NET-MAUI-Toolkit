using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartGenerator
{
    public class ImagePickerHelper
    {
        internal async Task<byte[]> PickImageAsync()
        {
            try
            {
                var result = await MediaPicker.PickPhotoAsync();

                if (result != null)
                {
                    // Open the file stream
                    using (Stream stream = await result.OpenReadAsync())
                    {
                        // Copy the stream into a memory stream to avoid object disposed exception.
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            await stream.CopyToAsync(memoryStream);
                            return memoryStream.ToArray();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error picking image: {ex.Message}");
            }

            return null;
        }

        internal async Task SaveImageAsync(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting image: {ex.Message}");
            }

            try
            {
                // Get the image from Gallery.
                byte[] imageBytes = await PickImageAsync();

                if (imageBytes != null)
                {
                    // Save the image bytes to a file, database, etc.
                    File.WriteAllBytes(filePath, imageBytes);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving image: {ex.Message}");
            }
        }


    }
}
