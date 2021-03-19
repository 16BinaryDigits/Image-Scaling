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
        private static object _lock = new object();
        private static Stopwatch _stopwatch = new Stopwatch();

        struct Pixel
        {
            public int X { get; set; }
            public int Y { get; set; }
        }
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
            Console.WriteLine();

            // Image scaling
            Bitmap scaled = new Bitmap(1,1);
            switch (_switchCheck)
            {
                case "1":
                    // Print original Bitmap size
                    Console.WriteLine("***************************************************");
                    Console.WriteLine($"Original image details : {original.Width}-{original.Height}");

                    // Scale Bitmap using Async method
                    Console.WriteLine($"Scaling async call ...");

                    _stopwatch.Start();
                    scaled = await ScaleImageAsync(original);
                    _stopwatch.Stop();

                    Console.WriteLine($"Method processing time : {_stopwatch.ElapsedMilliseconds}");

                    // Print scaled Bitmap size
                    Console.WriteLine($"Scaled image details: {scaled.Width}-{scaled.Height}");
                    Console.WriteLine("***************************************************");
                    break;

                case "2":
                    // Print original Bitmap size
                    Console.WriteLine("***************************************************");
                    Console.WriteLine($"Original image details : {original.Width}-{original.Height}");

                    // Scale Bitmap using Sync method
                    Console.WriteLine($"Scaling sync call ...");

                    _stopwatch.Start();
                    scaled = ScaleImageSync(original);
                    _stopwatch.Stop();

                    Console.WriteLine($"Method processing time : {_stopwatch.ElapsedMilliseconds}");

                    // Print scaled Bitmap size
                    Console.WriteLine($"Scaled image details: {scaled.Width}-{scaled.Height}");
                    Console.WriteLine("***************************************************");
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
            _stopwatch.Restart();

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

            await Task.WhenAll(Task.Run(() => Q1(bitmap, scaledImage)), Task.Run(() => Q2(bitmap, scaledImage)), Task.Run(() => Q3(bitmap, scaledImage)), Task.Run(() => Q4(bitmap, scaledImage)));

            return scaledImage;
        }

        // Handles quarter 1 processing
        static void Q1(Bitmap bitmap, Bitmap scaledImage)
        {
            Color color;
            Bitmap cloneBitmap;

            lock (_lock)
            {
                cloneBitmap = (Bitmap)bitmap.Clone();
            }

            for (int y = 0; y < cloneBitmap.Height / 2; y++)
            {
                for (int x = 0; x < cloneBitmap.Width / 2; x++)
                {
                    color = cloneBitmap.GetPixel(x, y);
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

        // Handles quarter 2 processing
        static void Q2(Bitmap bitmap, Bitmap scaledImage)
        {
            Color color;
            Bitmap cloneBitmap;

            lock (_lock)
            {
                cloneBitmap = (Bitmap)bitmap.Clone();
            }

            for (int y = cloneBitmap.Height / 2; y < cloneBitmap.Height; y++)
            {
                for (int x = 0; x < cloneBitmap.Width / 2; x++)
                {
                    color = cloneBitmap.GetPixel(x, y);
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

        // Handles quarter 3 processing
        static void Q3(Bitmap bitmap, Bitmap scaledImage)
        {
            Color color;
            Bitmap cloneBitmap;

            lock (_lock)
            {
                cloneBitmap = (Bitmap)bitmap.Clone();
            }

            for (int y = 0; y < cloneBitmap.Height / 2; y++)
            {
                for (int x = cloneBitmap.Width / 2; x < cloneBitmap.Width; x++)
                {
                    color = cloneBitmap.GetPixel(x, y);
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

        // Handles quarter 4 processing
        static void Q4(Bitmap bitmap, Bitmap scaledImage)
        {
            Color color;
            Bitmap cloneBitmap;

            lock (_lock)
            {
                cloneBitmap = (Bitmap)bitmap.Clone();
            }

            for (int y = cloneBitmap.Height / 2; y < cloneBitmap.Height; y++)
            {
                for (int x = cloneBitmap.Width / 2; x < cloneBitmap.Width; x++)
                {
                    color = cloneBitmap.GetPixel(x, y);
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
        static Bitmap ScaleImageSync(Bitmap bitmap)
        {
            Color color;
            Bitmap scaledImage = new Bitmap(bitmap.Width * 2, bitmap.Height * 2);

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    color = bitmap.GetPixel(x, y);
                    scaledImage.SetPixel((x * 2), (y * 2), color); // start
                    scaledImage.SetPixel((x * 2), (y * 2) + 1, color); // down
                    scaledImage.SetPixel((x * 2) + 1, (y * 2), color); // right
                    scaledImage.SetPixel((x * 2) + 1, (y * 2) + 1, color); // end
                }
            }

            return scaledImage;
        }
        #endregion
    }
}
