1.Merge a video for local player.

``` ./ffmpeg -framerate 24 -i "output3D\3dframe%08d.png" -i "output\audio.wav" -c:v libx264 -c:a aac -b:a 256k 0.mkv ```



2.For youtube. The aspect ratio will be weird in local player. Only for upload.

``` ./ffmpeg -i 0.mkv -c copy -metadata:s:v:0 stereo_mode=1 1.mkv ```
