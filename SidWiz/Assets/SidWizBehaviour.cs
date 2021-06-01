/*
Copy the "System.Drawing.dll" file in the "*\Unity\Editor\Data\Mono\lib\mono\2.0" folder into your "Assets" folder. 
2) Attach this script to any object in your scene. 
3) Change the "loadingGifPath" field of the script, (in the Inspector view), to the path of your Gif file. (this can be relative, from the root project folder (i.e. the parent of the "Assets" folder), or absolute
*/
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using SidWizPlus;
using UnityEngine;

public class SidWizBehaviour : MonoBehaviour
{
    public string loadingGifPath;
    public float speed = 1f;
    public Vector2 drawPosition;
    Program pr;
    Bitmap frame;
    Texture2D frameTexture;

    //List<Texture2D> gifFrames = new List<Texture2D>();
    void Awake()
    {
        pr = new Program();
        pr.Main();
    }


    void OnGUI()
    {
        if (frame == null)
        {
            frame = new Bitmap(pr.Renderer.Width, pr.Renderer.Height, PixelFormat.Format32bppPArgb);
        }

        pr.Renderer.RenderFrame(Time.frameCount / 100f, frame);
        if (frameTexture == null)
        {
            frameTexture = new Texture2D(frame.Width, frame.Height);
        }

        for (int x = 0; x < frame.Width; x++)
        for (int y = 0; y < frame.Height; y++)
        {
            System.Drawing.Color sourceColor = frame.GetPixel(x, y);
            frameTexture.SetPixel(x, frame.Height - y, new Color32(sourceColor.R, sourceColor.G, sourceColor.B, sourceColor.A)); // for some reason, x is flipped
        }
        frameTexture.Apply();

        GUI.DrawTexture(new Rect(drawPosition.x, drawPosition.y, frameTexture.width, frameTexture.height), frameTexture);
    }
}