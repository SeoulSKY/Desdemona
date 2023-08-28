using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Gentleland.StemapunkUI.DemoAndExample
{
    public class CursorSelector : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        Vector2 hotspot;
        Texture2D cursorTexture;
        void Awake()
        {
            Image image = GetComponent<Image>();
            if (image != null && image.sprite != null)
            {
                cursorTexture = new Texture2D((int)image.sprite.textureRect.width, (int)image.sprite.textureRect.height, TextureFormat.RGBA32, false);
                for (int y = 0; y < cursorTexture.height; y++)
                {
                    for (int x = 0; x < cursorTexture.width; x++)
                    {
                        cursorTexture.SetPixel(x, y, image.sprite.texture.GetPixel((int)image.sprite.textureRect.x + x, (int)image.sprite.textureRect.y + y));
                    }
                }
                cursorTexture.Apply();
            }
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            if (cursorTexture != null)
                Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
        }
    }
}