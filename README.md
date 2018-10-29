[![Build status](https://ci.appveyor.com/api/projects/status/9m0f8x11gcg3gy8f?svg=true)](https://ci.appveyor.com/project/Leayal/webp-sharp-wrapper)

# WebP-Sharp-wrapper
`libwebp` wrapper for .NET WPF environment in C#. ~~The most complete wrapper in pure managed C#~~.

Exposes Simple Decoding API and Encoding API, Advanced  Decoding and Encoding API (with stadistis of compresion), Get version library and WebPGetFeatures (info of any WebP file). Exposed get PSNR, SSIM or LSIM distortion metrics.

All the APIs, except the simple ones, will enable multi-threading by default.

The wrapper is in safe managed code. No need external dll except libwebp_x86.dll and libwebp_x64.dll. The wrapper work in 32, 64 bit or ANY (auto swith to the apropiate library).


The code is ~~full~~ comented and include simple example for using the wrapper.

## Decompress Functions:
Load WebP image for WebP file
```C#
using (WebP webp = new WebP())
  BitmapSource bmp = webp.DecodeFile("test.webp");
```

Decode WebP filename to BitmapSource and display to Image control
```C#
byte[] rawWebP = File.ReadAllBytes("test.webp");
using (WebP webp = new WebP())
  this.myImage.Source = webp.Decode(rawWebP);
```

Advanced decode WebP filename to BitmapSource and display to Image control
```C#
byte[] rawWebP = File.ReadAllBytes("test.webp");
DecoderOptions decoderOptions = new DecoderOptions();
decoderOptions.FlipVertically = 1; //Flip the image
using (WebP webp = new WebP())
  this.myImage.Source = webp.Decode(rawWebP, decoderOptions);
```

Advanced decode from unmamaged memory or from pinned managed memeory to BitmapSource
```C#
IntPtr rawWebP; // The pointer to zero index of the memory
DecoderOptions decoderOptions = new DecoderOptions();
decoderOptions.FlipVertically = true; //Flip the image
using (WebP webp = new WebP())
{
  var bitmapsource = webp.Decode(rawWebP, decoderOptions);
}
```

Get thumbnail with 200x150 pixels in fast/low quality mode
```C#
using (WebP webp = new WebP())
	this.myImage.Source = webp.GetThumbnailFast(rawWebP, 200, 150);
```

Get thumbnail with 200x150 pixels in slow/high quality mode
```C#
using (WebP webp = new WebP())
	this.myImage.Source = webp.GetThumbnailQuality(rawWebP, 200, 150);
```


## Compress Functions:
Save BitmapSource to WebP file
```C#
BitmapSource bmp = new BitmapImage(new Uri("test.png", UriKind.Relative));
using (WebP webp = new WebP())
  webp.EncodeToFile(bmp, "test.webp", 80);
```

Encode to memory buffer in lossly mode with quality 75 and save to file
```C#
BitmapSource bmp;
byte[] rawWebP = File.ReadAllBytes("test.jpg");
using (WebP webp = new WebP())
using (var webpfile = webp.EncodeLossy(bmp, 75))
using (FileStream fs = File.Create("test.webp"))
{
  webpfile.Content.CopyTo(fs);
}
```

Encode to memory buffer in lossly mode with quality 75 and speed 9. Save to file
```C#
BitmapSource bmp;
byte[] rawWebP = File.ReadAllBytes("test.jpg");
using (WebP webp = new WebP())
using (var webpfile = webp.EncodeLossy(bmp, 75, 9))
using (FileStream fs = File.Create("test.webp"))
{
  webpfile.Content.CopyTo(fs);
}
```

Encode to memory buffer in lossless mode and save to file
```C#
BitmapSource bmp;
byte[] rawWebP = File.ReadAllBytes("test.jpg");
using (WebP webp = new WebP())
using (var webpfile = webp.EncodeLossless(bmp))
using (FileStream fs = File.Create("test.webp"))
{
  webpfile.Content.CopyTo(fs);
}
```

Encode to memory buffer in lossless mode with speed 9 and save to file
```C#
BitmapSource bmp;
byte[] rawWebP = File.ReadAllBytes("test.jpg");
using (WebP webp = new WebP())
using (var webpfile = webp.EncodeLossless(bmp, 9))
using (FileStream fs = File.Create("test.webp"))
{
  webpfile.Content.CopyTo(fs);
}
```

Encode to memory buffer in near lossless mode with quality 40 and speed 9 and save to file
```C#
BitmapSource bmp;
byte[] rawWebP = File.ReadAllBytes("test.jpg");
using (WebP webp = new WebP())
using (var webpfile = webp.EncodeNearLossless(bmp, 40, 9))
using (FileStream fs = File.Create("test.webp"))
{
  webpfile.Content.CopyTo(fs);
}
```

## Another Functions:	
Get version of libwebp.dll
```C#
using (WebP webp = new WebP())
  string version = "libwebp.dll v" + webp.GetVersion();
```

Get info from WebP file
```C#
byte[] rawWebp = File.ReadAllBytes(pathFileName);
WebPHeader header;
using (WebP webp = new WebP())
  header = webp.GetInfo(rawWebp);
MessageBox.Show("Width: " + header.PixelWidth + "\n" +
                "Height: " + header.PixelHeight + "\n" +
                "Has alpha: " + header.HasAlpha + "\n" +
                "Is animation: " + header.HasAnimation + "\n" +
                "Format: " + header.CompressionType.ToString());
```

Check the content of a file whether is is really a WebP file
```C#
string path = System.IO.Path.Combine("Path", "to", "file");
bool result = WebP.IsWebp(path);
```

Get PSNR, SSIM or LSIM distortion metric between two pictures
```C#
int metric = 0;  //0 = PSNR, 1= SSIM, 2=LSIM
BitmapSource bmp1 = BitmapSource.FromFile("image1.png");
BitmapSource bmp2 = BitmapSource.FromFile("image2.png");
using (WebP webp = new WebP())
	result = webp.GetPictureDistortion(source, reference, metric);
	                    MessageBox.Show("Red: " + result[0] + "dB.\nGreen: " + result[1] + "dB.\nBlue: " + result[2] + "dB.\nAlpha: " + result[3] + "dB.\nAll: " + result[4] + "dB.", "PSNR");

MessageBox.Show("Red: " + result[0] + dB\n" +
                "Green: " + result[1] + "dB\n" +
                "Blue: " + result[2] + "dB\n" +
                "Alpha: " + result[3] + "dB\n" +
                "All: " + result[4] + "dB\n");
```


## Thanks to jzern@google.com and [JosePineiro's repo](https://github.com/JosePineiro/WebP-wrapper)
Without their help this wapper would not have been possible.
