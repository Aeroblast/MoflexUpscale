using System;

namespace MoflexUpscale
{
    class Program
    {
        static void Main(string[] args)
        {
            double fps;
            //Moflex.Process(@"E:\MoflexUpscale\新･光神話 パルテナの鏡 3Dアニメ おいかけて (シャフト).moflex", @"E:\MoflexUpscale\output\", out fps);
            Proc3DFrame.MergeAll(@"E:\MoflexUpscale\output(UpRGB)(noise_scale)(Level2)(height 1080)\", @"E:\MoflexUpscale\output3D\");
        
        //ffmpeg -i input_file.mkv -c copy -metadata:s:v:0 stereo_mode=1 output_file.mkv
        }


    }
}
