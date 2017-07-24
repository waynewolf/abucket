## rgb565 texturing

A command line tool to analyze CTS case fail (testVectorDrawableInImageView). The tool runs on android N that bypasses RGB565 EGLImage generation with pre-defined texels, to verify only the texturing part.

Put the source in a ready-to-make android N environment, run mm to build. A bit of words about what I do:

1. Use the BitmapFactory.decodeResource (in the java apk, same algorithm used as in CTS) to generate the Bitmap from golden png, the Bitmap is ARGB8888, save it, remove header to raw data (golden_bmp_argb.cpp), to be compared later.
2. Prepare an RGB565 EGLImage. This is tricky how to generate this data, worth debating, I used gimp to convert golden png to RGB565 bmp, cut header, save the raw data in rgb565.data. 
3. Run the command line test program, texuture the RGB565 EGLImage, draw on surface, read pixel back, compare it with golden ARGB8888 which is generated in step 1. The command line program will print if the test pass or not.


