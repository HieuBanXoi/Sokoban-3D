using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))] // Ép buộc script này chỉ được gắn vào object có Button
public class ButtonSound : MonoBehaviour
{
    public Button button;
    private void Reset()
    {
        button = GetComponent<Button>();
    }
    void Start()
    {
        button.onClick.AddListener(PlaySound);
    }

    void PlaySound()
    {
        Ply_SoundManager.Ins.PlayFx(FxType.Click);
    }
}