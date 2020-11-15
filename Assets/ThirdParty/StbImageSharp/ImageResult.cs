﻿using System;
using System.IO;
using System.Runtime.InteropServices;

namespace StbImageSharp
{
#if !STBSHARP_INTERNAL
	public
#else
	internal
#endif
	class ImageResult
	{
		public int Width
		{
			get; set;
		}
		public int Height
		{
			get; set;
		}
		public ColorComponents SourceComp
		{
			get; set;
		}
		public ColorComponents Comp
		{
			get; set;
		}
		public byte[] Data
		{
			get; set;
		}

		internal static unsafe ImageResult FromResult(byte* result, int width, int height, ColorComponents comp,
			ColorComponents req_comp)
		{
			if (result == null)
				throw new InvalidOperationException(StbImage.LastError);

			var image = new ImageResult
			{
				Width = width,
				Height = height,
				SourceComp = comp,
				Comp = req_comp == ColorComponents.Default ? comp : req_comp
			};

			// Convert to array
			image.Data = new byte[width * height * (int)image.Comp];
			Marshal.Copy(new IntPtr(result), image.Data, 0, image.Data.Length);

			return image;
		}

		public static unsafe ImageResult FromStream(Stream stream,
			ColorComponents requiredComponents = ColorComponents.Default,
			bool verticalFlip = false)
		{
			byte* result = null;

			try
			{
				int x, y, comp;

				var context = new StbImage.stbi__context(stream);

				StbImage.stbi_set_flip_vertically_on_load(verticalFlip ? 1 : 0);
				result = StbImage.stbi__load_and_postprocess_8bit(context, &x, &y, &comp, (int)requiredComponents);

				return FromResult(result, x, y, (ColorComponents)comp, requiredComponents);
			}
			finally
			{
				if (result != null)
					CRuntime.free(result);
			}
		}

		public static ImageResult FromMemory(byte[] data, 
			ColorComponents requiredComponents = ColorComponents.Default,
			bool verticalFlip = false)
		{
			using (var stream = new MemoryStream(data))
			{
				return FromStream(stream, requiredComponents, verticalFlip);
			}
		}
	}
}
