using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HartLib
{
    public class ImageProcessing
    {

        public struct ColorRBGA
        {
            byte r { set; get; }
            byte g { set; get; }
            byte b { set; get; }
            byte a { set; get; }

            public override string ToString() => $"({r}, {g}, {b}, {a})";

            public ColorRBGA(float _r, float _g, float _b, float _a)
            {
                r = (byte)(_r * 255);
                g = (byte)(_g * 255);
                b = (byte)(_b * 255);
                a = (byte)(_a * 255);
            }
            public ColorRBGA(Color col)
            {
                r = (byte)(col[0] * 255);
                g = (byte)(col[1] * 255);
                b = (byte)(col[2] * 255);
                a = (byte)(col[3] * 255);
            }

        }

        public static void PixelsFromImage(string path)
        {

            Image img = new Image(); // = (Image)GD.Load("res://Imported/TestColors.png");
            var texture = (Texture)GD.Load("res://Imported/TestColors.png");
            img = texture.GetData();

            img.Lock();

            var pixelArray = new ColorRBGA[img.GetWidth(), img.GetHeight()];

            for (int y = 0; y < img.GetHeight(); y++)
            {
                for (int x = 0; x < img.GetWidth(); x++)
                {
                    pixelArray[x, y] = new ColorRBGA(img.GetPixel(x, y));
                    GD.Print(pixelArray[x, y].ToString());
                } //TODO This 
            }

            GD.Print(img.GetHeight());
            //var size = new Vector2i(img.GetSize());
            //GD.Print(size);
        }


    }
}