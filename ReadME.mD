
Moflex video is used by Nintendo 3DS. Decode work: https://github.com/Gericom/MobiclipDecoder.



See Program.cs and comment out/in.
- Split moflex video to wav and frames. (Then you can upscale with  waifu2x)
- If this  is a 3D video, you can rename them to frameXXXXXL and frameXXXXXR
- Merge left and right frame to side by side frame.

ffmpeg examples:

Merge a 3D video for local player.

``` ./ffmpeg -framerate 24 -i "output3D\3dframe%08d.png" -i "output\audio.wav" -c:v libx264 -c:a aac -b:a 256k 0.mkv ```


Add 3D metadata for youtube. The aspect ratio will be weird in local player. Only for upload.

``` ./ffmpeg -i 0.mkv -c copy -metadata:s:v:0 stereo_mode=1 1.mkv ```


Merge a normal video use left frame data

``` ./ffmpeg -framerate 24 -i "output(UpRGB)(noise_scale)(Level2)(height 1080)\frame%08dL.png" -i "output\audio.wav" -c:v libx264 -c:a aac -b:a 256k 0.mkv ```

