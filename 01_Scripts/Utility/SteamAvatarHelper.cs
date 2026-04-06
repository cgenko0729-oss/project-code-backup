using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager
using Cysharp.Threading.Tasks;
using Steamworks;


public static  class SteamAvatarHelper 
{
    // Gets the avatar as a Texture2D. Returns null if loading fails.
    public static Texture2D GetSteamImageAsTexture2D(int iImage)
    {
        Texture2D texture = null;

        bool isValid = SteamUtils.GetImageSize(iImage, out uint width, out uint height);
        if (isValid)
        {
            // Allocate a byte array to hold the image data
            byte[] image = new byte[width * height * 4];

            // Get the raw data from Steam
            bool success = SteamUtils.GetImageRGBA(iImage, image, (int)(width * height * 4));

            if (success)
            {
                texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
                texture.LoadRawTextureData(image);
                
                // Steam images are upside down in Unity. We must flip them.
                texture = FlipTexture(texture);
                
                texture.Apply();
            }
        }

        return texture;
    }

    private static Texture2D FlipTexture(Texture2D original)
    {
        int width = original.width;
        int height = original.height;
        
        Color32[] pixels = original.GetPixels32();
        Color32[] flippedPixels = new Color32[pixels.Length];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                flippedPixels[x + y * width] = pixels[x + (height - y - 1) * width];
            }
        }

        Texture2D flipped = new Texture2D(width, height);
        flipped.SetPixels32(flippedPixels);
        flipped.Apply();

        return flipped;
    }
}

