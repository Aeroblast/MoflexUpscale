

namespace MoflexUpscale
{
    class Program
    {
        static void Main(string[] args)
        {
            double fps;
            Moflex.Process("新･光神話 パルテナの鏡 3Dアニメ おいかけて (シャフト).moflex","output/",out fps);
        
        
        //ffmpeg -i input_file.mkv -c copy -metadata:s:v:0 stereo_mode=1 output_file.mkv
        }


    }
}
