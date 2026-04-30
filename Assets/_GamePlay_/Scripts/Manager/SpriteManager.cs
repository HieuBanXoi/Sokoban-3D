using UnityEngine;

public class SpriteManager : Ply_Singleton<SpriteManager>
{
    [Header("Character Skins (Sprites)")]
    // Kéo toàn bộ ảnh skin nhân vật vào đây
    public Sprite[] playerSkinSprites;

    [Header("Map Skins (Sprites)")]
    // Kéo toàn bộ ảnh map vào đây (Nature, Ice, Rock...)
    public Sprite[] mapSkinSprites;
}