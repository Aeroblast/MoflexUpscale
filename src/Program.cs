using System;
using System.Collections.Generic;
using System.IO;
namespace MoflexUpscale
{
    class Program
    {
        static void Main(string[] args)
        {
            double fps;
            //Moflex.Process(@"E:\MoflexUpscale\新･光神話 パルテナの鏡 3Dアニメ おいかけて (シャフト).moflex", @"E:\MoflexUpscale\output\", out fps);

            //Rename
            RenameLR(@"E:\MoflexUpscale\output(UpRGB)(noise_scale)(Level2)(height 1080)\");

            //Merge Left and  right.
            //Proc3DFrame.MergeAll(@"E:\MoflexUpscale\output(UpRGB)(noise_scale)(Level2)(height 1080)\", @"E:\MoflexUpscale\output3D\");
        
        //ffmpeg -i input_file.mkv -c copy -metadata:s:v:0 stereo_mode=1 output_file.mkv
        }
        static void RenameLR(string src) 
        {
            List<string> imgs = new List<string>(Directory.GetFiles(src, "*.png"));
            imgs.Sort();
            if (imgs.Count % 2 != 0) throw new System.Exception("Image count not ");
            int i = 0;
            for (; i * 2 < imgs.Count; i++)
            {
                File.Move(imgs[i*2],Path.Combine(src, "frame" + Util.NumToNo(i, 8) + "L.png"));
                File.Move(imgs[i * 2+1], Path.Combine(src, "frame" + Util.NumToNo(i, 8) + "R.png"));
            }

        }


    }
}
public class Util 
{

    public static string NumToNo(int i, int n)
    {
        string r = i.ToString();
        for (int j = n - r.Length; j > 0; j--) r = "0" + r;
        return r;
    }
}