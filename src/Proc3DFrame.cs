using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Collections.Generic;

class Proc3DFrame
{
    public static void Merge(string src_L, string src_R, string dst)
    {
        const int w = 1920, h = 1080;
        Image l = Image.FromFile(src_L);
        Image r = Image.FromFile(src_R);
        Bitmap d = new Bitmap(w, h, l.PixelFormat);
        using (Graphics g = Graphics.FromImage(d))
        {
            g.FillRectangle(Brushes.Black, 0, 0, w, h);
            g.DrawImage(l, (w - l.Width) / 4, 0, l.Width / 2, l.Height);
            g.DrawImage(r, w / 2 + (w - l.Width) / 4, 0, l.Width / 2, l.Height);
        }
        d.Save(dst);
        l.Dispose();
        r.Dispose();
        d.Dispose();
    }
    public static void MergeAll(string src,string dst) 
    {
        List<string> imgs = new List<string>(Directory.GetFiles(src,"*.png"));
        imgs.Sort();
        if (imgs.Count % 2 != 0) throw new System.Exception("Image count not ");
        int i = 0;
        for (; i*2 < imgs.Count;i++) 
        {
            string o = Path.Combine(dst,"3dframe"+Util.NumToNo(i,8)+".png");
            Merge(imgs[2*i],imgs[2*i+1],o);
        }
    
    }
}

