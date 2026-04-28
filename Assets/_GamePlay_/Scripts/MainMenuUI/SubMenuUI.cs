using UnityEngine;
using UnityEngine.UI;

public class SubMenuUI : MonoBehaviour
{
    public Button backBtn;

    private void Start()
    {
        // Khi bấm Back ở bất kỳ màn hình phụ nào, đều quay về Dashboard
        if (backBtn != null)
        {
            backBtn.onClick.AddListener(() => MenuUIManager.Instance.ShowDashboard());
        }
    }
}