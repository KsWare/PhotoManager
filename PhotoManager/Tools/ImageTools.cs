using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace KsWare.PhotoManager.Tools
{
	public static class ImageTools
	{
		public static readonly Dictionary<string, ImageFormat> SupportedExtensions = new Dictionary<string, ImageFormat>
		{
			{".jpg", ImageFormat.Jpeg},
			{".jpeg", ImageFormat.Jpeg},
			{".png", ImageFormat.Png},
			{".bmp", ImageFormat.Bmp},
			{".emf", ImageFormat.Emf},
			{".exif", ImageFormat.Exif},
			{".gif", ImageFormat.Gif},
			{".ico", ImageFormat.Icon},
			{".tif", ImageFormat.Tiff},
			{".tiff", ImageFormat.Tiff},
			{".wmf", ImageFormat.Wmf}
		};


		public static Bitmap CreatePreview(string filePath, int maxLength)
		{
			var image = Image.FromFile(filePath);
			var length = Math.Max(image.Height, image.Width);
			var f = (double)maxLength / (double)length;
			var previewBitmap = ResizeImage(image, (int)((double)image.Width * f), (int)((double)image.Height * f));
			return previewBitmap;
		}

		public static Bitmap ResizeImage(Image image, int width, int height)
		{
			var destRect = new Rectangle(0, 0, width, height);
			var destImage = new Bitmap(width, height);

			destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

			using (var graphics = Graphics.FromImage(destImage))
			{
				graphics.CompositingMode = CompositingMode.SourceCopy;
				graphics.CompositingQuality = CompositingQuality.HighQuality;
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics.SmoothingMode = SmoothingMode.HighQuality;
				graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

				using (var wrapMode = new ImageAttributes())
				{
					wrapMode.SetWrapMode(WrapMode.TileFlipXY);
					graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
				}
			}

			return destImage;
		}

		public static BitmapSource CreateBitmapSource(Bitmap bitmap)
		{
			var hBitmap = bitmap.GetHbitmap();
			try
			{
				var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
					hBitmap,
					IntPtr.Zero,
					Int32Rect.Empty,
					BitmapSizeOptions.FromEmptyOptions()
				);
				return bitmapSource;

			}
			finally
			{
				DeleteObject(hBitmap);
			}
		}

		public static BitmapSource CreateBitmapSource2(Bitmap bitmap)
		{
			if (bitmap == null) throw new ArgumentNullException(nameof(bitmap));
			if (ApplicationWrapper.Dispatcher == null) return null; // Is it possible?

			try
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					// You need to specify the image format to fill the stream. 
					// I'm assuming it is PNG
					bitmap.Save(memoryStream, ImageFormat.Png);
					memoryStream.Seek(0, SeekOrigin.Begin);

					// Make sure to create the bitmap in the UI thread
					if (InvokeRequired)
						return (BitmapSource)ApplicationWrapper.Dispatcher.Invoke(
							new Func<Stream, BitmapSource>(CreateBitmapSourceFromBitmap),
							DispatcherPriority.Normal,
							memoryStream);
					return CreateBitmapSourceFromBitmap(memoryStream);
				}
			}
			catch (Exception)
			{
				return null;
			}
		}

		private static bool InvokeRequired => Dispatcher.CurrentDispatcher != ApplicationWrapper.Dispatcher;

		private static BitmapSource CreateBitmapSourceFromBitmap(Stream stream)
		{
			var bitmapDecoder = BitmapDecoder.Create(
				stream,
				BitmapCreateOptions.PreservePixelFormat,
				BitmapCacheOption.OnLoad);

			// This will disconnect the stream from the image completely...
			var writable = new WriteableBitmap(bitmapDecoder.Frames.Single());
			writable.Freeze();

			return writable;
		}

		[System.Runtime.InteropServices.DllImport("gdi32.dll")]
		private static extern bool DeleteObject(IntPtr hObject);

		public static Bitmap GetBitmap(BitmapSource source)
		{
			Bitmap bmp = new Bitmap
			(
				source.PixelWidth,
				source.PixelHeight,
				System.Drawing.Imaging.PixelFormat.Format32bppPArgb
			);

			BitmapData data = bmp.LockBits
			(
				new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size),
				ImageLockMode.WriteOnly,
				System.Drawing.Imaging.PixelFormat.Format32bppPArgb
			);

			source.CopyPixels
			(
				Int32Rect.Empty,
				data.Scan0,
				data.Height * data.Stride,
				data.Stride
			);

			bmp.UnlockBits(data);

			return bmp;
		}

		public static ImageSource CreateBitmapSource(string cacheFilePath)
		{
			var uri=new Uri(cacheFilePath);
			if (InvokeRequired)
				return (BitmapSource)ApplicationWrapper.Dispatcher.Invoke(
					new Func<Uri, BitmapSource>(CreateBitmapSourceFromUri),
					DispatcherPriority.Normal,uri);
			return CreateBitmapSourceFromUri(uri);
		}

		private static BitmapSource CreateBitmapSourceFromUri(Uri uri)
		{
			var bitmapDecoder = BitmapDecoder.Create(
				uri,
				BitmapCreateOptions.PreservePixelFormat,
				BitmapCacheOption.OnLoad);

			// This will disconnect the stream from the image completely...
			var writable = new WriteableBitmap(bitmapDecoder.Frames.Single());
			writable.Freeze();

			return writable;
		}

		public static void Save(Bitmap bitmap, string path)
		{
			var ext = Path.GetExtension(path)?.ToLower();
			switch (ext)
			{
				case ".jpg": case ".jpeg": SaveAsJpeg(bitmap, path, 75); break;
				default: bitmap.Save(path, GetImageFormat(ext)); break;
			}
		}

		public static void SaveAsJpeg(this Bitmap bitmap, string path, int quality)
		{
			var codecInfo = GetEncoderInfo("image/jpeg");
			var encoderParameters = new EncoderParameters(1);
			encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);
			bitmap.Save(path, codecInfo, encoderParameters);
		}

		public static ImageCodecInfo GetEncoderInfo(string mimeType)
		{
			int j;
			var encoders = ImageCodecInfo.GetImageEncoders();
			for (j = 0; j < encoders.Length; ++j)
			{
				if (encoders[j].MimeType == mimeType)
					return encoders[j];
			}
			return null;
		}

		public static ImageFormat GetImageFormat(string extension)
		{
			return SupportedExtensions.TryGetValue(extension, out var imageFormat) ? imageFormat : null;
		}
	}
}
