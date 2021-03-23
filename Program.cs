using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;

namespace ImageScaling
{
    class Program
    {
        #region Local_Params
        // Local variables & types
        private static readonly object _lock = new object();
        #endregion

        #region Main_Method
        static async Task Main(string[] args)
        {
        Start:
            // To infinity  
            await InfinitLoop();
            goto Start;
        }
        #endregion

        #region InfinitLoop
        static async Task<bool> InfinitLoop()
        {
            // Generate Bitmap
            Bitmap original;

            // Generate random Bitmap
            //original = GenerateBitmap(10, 10);

            // Get Bitmap from IO
            original = new Bitmap(@$"{Environment.CurrentDirectory}\1080.bmp");

            // Save original Bitmap to IO
            original.Save(@$"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\original.bmp");

            // Switch Handling
            Console.WriteLine("Press (1) async, (2) sync image scaling ...");
            string _switchCheck = Console.ReadLine();

            // Image scaling
            Bitmap scaled = new Bitmap(1,1);

            switch (_switchCheck)
            {
                case "1":
                    // Scale Bitmap using Async method
                    scaled = await ScaleImageAsync(original);
                    break;

                case "2":
                    // Scale Bitmap using Sync method
                    scaled = ScaleImageSync(original);
                    break;

                default:
                    goto End;
                    break;
            }

            // Save Scaled Bitmap to IO
            scaled.Save(@$"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\scaled.bmp");

            // Clean Up
            original.Dispose();
            scaled.Dispose();

        End:
            return true;
        }
        #endregion

        #region GenerateRandomBitmap
        // Generate a random bitmap based on a pre-set color list
        static Bitmap GenerateRandomBitmap(int width, int height)
        {
            List<Color> colors = new List<Color> { Color.White, Color.Black, Color.Gray };
            Bitmap bitmap = new Bitmap(width, height);
            Random random = new Random();

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    bitmap.SetPixel(x, y, colors[random.Next(0, colors.Count)]);
                }
            }

            return bitmap;
        }
        #endregion

        #region Async_Image_Scaling
        // Divide the image scaling to 4 thread_pool tasks (each handling one quarter of the image)
        static async Task<Bitmap> ScaleImageAsync(Bitmap bitmap)
        {
            Bitmap scaledImage = new Bitmap(bitmap.Width * 2, bitmap.Height * 2);

            await Task.WhenAll(
                Task.Run(() => BuildQuarter(1, bitmap, scaledImage)),
                Task.Run(() => BuildQuarter(2, bitmap, scaledImage )),
                Task.Run(() => BuildQuarter(3, bitmap, scaledImage)), 
                Task.Run(() => BuildQuarter(4, bitmap, scaledImage))
                );

            return scaledImage;
        }

        // Handles quarter processing
        static void BuildQuarter(int quarter, Bitmap bitmap, Bitmap scaledImage, bool greyScale = false)
        {
            int xStart = 0, xEnd = 0, yStart = 0, yEnd = 0;
            Color color;
            Bitmap cloneBitmap = bitmap;

            lock (_lock) cloneBitmap = (Bitmap)bitmap.Clone();

            switch (quarter)
            {
                case 1:
                    xStart = 0;
                    xEnd = cloneBitmap.Width / 2;
                    yStart = 0;
                    yEnd = cloneBitmap.Height / 2;
                    break;
                case 2:
                    xStart = 0;
                    xEnd = cloneBitmap.Width / 2;
                    yStart = cloneBitmap.Height / 2;
                    yEnd = cloneBitmap.Height;
                    break;
                case 3:
                    xStart = cloneBitmap.Width / 2;
                    xEnd = cloneBitmap.Width;
                    yStart = 0;
                    yEnd = cloneBitmap.Height / 2;
                    break;
                case 4:
                    xStart = cloneBitmap.Width / 2;
                    xEnd = cloneBitmap.Width;
                    yStart = cloneBitmap.Height / 2;
                    yEnd = cloneBitmap.Height;
                    break;
                default:
                    break;
            }

            for (int y = yStart; y < yEnd; y++)
            {
                for (int x = xStart; x < xEnd; x++)
                {
                    color = cloneBitmap.GetPixel(x, y);

                    if (greyScale) color = GetColorGreyScale(color);

                    lock (_lock)
                    {
                        scaledImage.SetPixel((x * 2), (y * 2), color); // start
                        scaledImage.SetPixel((x * 2), (y * 2) + 1, color); // down
                        scaledImage.SetPixel((x * 2) + 1, (y * 2), color); // right
                        scaledImage.SetPixel((x * 2) + 1, (y * 2) + 1, color); // end
                    }
                }
            }

            cloneBitmap.Dispose();
        }
        #endregion

        #region Sync_Image_Scaling
        // asynchronous scaling of the image on the main thread
        static Bitmap ScaleImageSync(Bitmap bitmap, bool greyScale = false)
        {
            Color color;
            Bitmap scaledImage = new Bitmap(bitmap.Width * 2, bitmap.Height * 2);

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    color = bitmap.GetPixel(x, y);

                    if (greyScale) color = GetColorGreyScale(color);

                    scaledImage.SetPixel((x * 2), (y * 2), color); // start
                    scaledImage.SetPixel((x * 2), (y * 2) + 1, color); // down
                    scaledImage.SetPixel((x * 2) + 1, (y * 2), color); // right
                    scaledImage.SetPixel((x * 2) + 1, (y * 2) + 1, color); // end
                }
            }

            return scaledImage;
        }

        static Color GetColorGreyScale(Color color)
        {
            int greyScale = (int)((color.R * 0.299) + (color.G * 0.587) + (color.B * 0.114));
            return Color.FromArgb(greyScale, greyScale, greyScale);
        }
        #endregion
    }
}
