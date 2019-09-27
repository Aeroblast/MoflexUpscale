using MobiConverter;
using System;
using System.Collections.Generic;
using System.IO;
using LibMobiclip.Containers.Moflex;
using LibMobiclip.Codec;
using System.Drawing;
using LibMobiclip.Codec.Mobiclip;
using LibMobiclip.Containers.Mods;
using LibMobiclip.Codec.Sx;
using LibMobiclip.Utils;
using LibMobiclip.Codec.FastAudio;

class Moflex
{
    public static void Process(string srcfile, string outDir,out double fps)
    {


        MobiclipDecoder decoder = null;
        MemoryStream audio = null;
        FastAudioDecoder[] mFastAudioDecoders = null;
        int audiorate = -1;
        int audiochannels = 0;
        //VideoStream vs = null;
        FileStream stream = File.OpenRead(srcfile);
        var d = new MoLiveDemux(stream);
        int PlayingVideoStream = -1;

        int frameCount = 0; double finalFps = 0;
        d.OnCompleteFrameReceived += delegate (MoLiveChunk Chunk, byte[] Data)
        {
            if ((Chunk is MoLiveStreamVideo || Chunk is MoLiveStreamVideoWithLayout) && ((PlayingVideoStream == -1) || ((MoLiveStream)Chunk).StreamIndex == PlayingVideoStream))
            {

                if (decoder == null)
                {
                    decoder = new MobiclipDecoder(((MoLiveStreamVideo)Chunk).Width, ((MoLiveStreamVideo)Chunk).Height, MobiclipDecoder.MobiclipVersion.Moflex3DS);
                    PlayingVideoStream = ((MoLiveStream)Chunk).StreamIndex;
                }
                decoder.Data = Data;
                decoder.Offset = 0;
                Bitmap b = decoder.DecodeFrame();
                b.Save(outDir + "frame" + NumToNo(frameCount, 8) + ".png", System.Drawing.Imaging.ImageFormat.Png);
                frameCount++;
                finalFps = (double)((MoLiveStreamVideo)Chunk).FpsRate / ((double)((MoLiveStreamVideo)Chunk).FpsScale);
                Console.WriteLine(finalFps);
            }
            else if (Chunk is MoLiveStreamAudio)
            {
                if (audio == null)
                {
                    audio = new MemoryStream();
                    audiochannels = (int)((MoLiveStreamAudio)Chunk).Channel;
                    audiorate = (int)((MoLiveStreamAudio)Chunk).Frequency;
                }
                switch ((int)((MoLiveStreamAudio)Chunk).CodecId)
                {
                    case 0://fastaudio
                        {
                            if (mFastAudioDecoders == null)
                            {
                                mFastAudioDecoders = new FastAudioDecoder[(int)((MoLiveStreamAudio)Chunk).Channel];
                                for (int i = 0; i < (int)((MoLiveStreamAudio)Chunk).Channel; i++)
                                {
                                    mFastAudioDecoders[i] = new FastAudioDecoder();
                                }
                            }
                            List<short>[] channels = new List<short>[(int)((MoLiveStreamAudio)Chunk).Channel];
                            for (int i = 0; i < (int)((MoLiveStreamAudio)Chunk).Channel; i++)
                            {
                                channels[i] = new List<short>();
                            }

                            int offset = 0;
                            int size = 40;
                            while (offset + size < Data.Length)
                            {
                                for (int i = 0; i < (int)((MoLiveStreamAudio)Chunk).Channel; i++)
                                {
                                    mFastAudioDecoders[i].Data = Data;
                                    mFastAudioDecoders[i].Offset = offset;
                                    channels[i].AddRange(mFastAudioDecoders[i].Decode());
                                    offset = mFastAudioDecoders[i].Offset;
                                }
                            }
                            short[][] channelsresult = new short[(int)((MoLiveStreamAudio)Chunk).Channel][];
                            for (int i = 0; i < (int)((MoLiveStreamAudio)Chunk).Channel; i++)
                            {
                                channelsresult[i] = channels[i].ToArray();
                            }
                            byte[] result = InterleaveChannels(channelsresult);
                            audio.Write(result, 0, result.Length);
                        }
                        break;
                    case 1://IMA-ADPCM
                        {
                            IMAADPCMDecoder[] decoders = new IMAADPCMDecoder[(int)((MoLiveStreamAudio)Chunk).Channel];
                            List<short>[] channels = new List<short>[(int)((MoLiveStreamAudio)Chunk).Channel];
                            for (int i = 0; i < (int)((MoLiveStreamAudio)Chunk).Channel; i++)
                            {
                                decoders[i] = new IMAADPCMDecoder();
                                decoders[i].GetWaveData(Data, 4 * i, 4);
                                channels[i] = new List<short>();
                            }

                            int offset = 4 * (int)((MoLiveStreamAudio)Chunk).Channel;
                            int size = 128 * (int)((MoLiveStreamAudio)Chunk).Channel;
                            while (offset + size < Data.Length)
                            {
                                for (int i = 0; i < (int)((MoLiveStreamAudio)Chunk).Channel; i++)
                                {
                                    channels[i].AddRange(decoders[i].GetWaveData(Data, offset, 128));
                                    offset += 128;
                                }
                            }
                            short[][] channelsresult = new short[(int)((MoLiveStreamAudio)Chunk).Channel][];
                            for (int i = 0; i < (int)((MoLiveStreamAudio)Chunk).Channel; i++)
                            {
                                channelsresult[i] = channels[i].ToArray();
                            }
                            byte[] result = InterleaveChannels(channelsresult);
                            audio.Write(result, 0, result.Length);
                        }
                        break;
                    case 2://PCM16
                        {
                            audio.Write(Data, 0, Data.Length - (Data.Length % ((int)((MoLiveStreamAudio)Chunk).Channel * 2)));
                        }
                        break;
                }
            }
        };

        int counter = 0;
        while (true)
        {
            uint error = d.ReadPacket();
            if (error == 73)
                break;

            counter++;
            if (counter == 50) counter = 0;
        }
        if (audio != null)
        {
            //Write WAV
            byte[] adata = audio.ToArray();
            audio.Close();
            byte[] head = new byte[0x2C + adata.Length];

            new byte[] { (byte)'R', (byte)'I', (byte)'F', (byte)'F' }.CopyTo(head, 0x0);
            BitConverter.GetBytes((Int32)(0x2C - 8 + adata.Length)).CopyTo(head, 0x4);
            new byte[] { (byte)'W', (byte)'A', (byte)'V', (byte)'E' }.CopyTo(head, 0x8);
            new byte[] { (byte)'f', (byte)'m', (byte)'t', 0x20 }.CopyTo(head, 0xC);
            BitConverter.GetBytes((Int32)0x10).CopyTo(head, 0x10);
            BitConverter.GetBytes((Int16)0x1).CopyTo(head, 0x14);
            head[0x16] = (byte)audiochannels;
            BitConverter.GetBytes((Int32)audiorate).CopyTo(head, 0x18);
            BitConverter.GetBytes((Int32)(audiorate * audiochannels * 2)).CopyTo(head, 0x1C);
            BitConverter.GetBytes((Int16)(audiochannels * 2)).CopyTo(head, 0x20);
            head[0x22] = 16;
            new byte[] { (byte)'d', (byte)'a', (byte)'t', (byte)'a' }.CopyTo(head, 0x24);
            BitConverter.GetBytes((Int32)(adata.Length)).CopyTo(head, 0x28);
            adata.CopyTo(head, 0x2C);
            File.WriteAllBytes(outDir+"audio.wav",head);
        }
        stream.Close();
        fps=finalFps;

    }
    private static byte[] InterleaveChannels(params Int16[][] Channels)
    {
        if (Channels.Length == 0) return new byte[0];
        byte[] Result = new byte[Channels[0].Length * Channels.Length * 2];
        for (int i = 0; i < Channels[0].Length; i++)
        {
            for (int j = 0; j < Channels.Length; j++)
            {
                Result[i * 2 * Channels.Length + j * 2] = (byte)(Channels[j][i] & 0xFF);
                Result[i * 2 * Channels.Length + j * 2 + 1] = (byte)(Channels[j][i] >> 8);
            }
        }
        return Result;
    }
    static string NumToNo(int i, int n)
    {
        string r = i.ToString();
        for (int j = n - r.Length; j > 0; j--) r = "0" + r;
        return r;
    }

}