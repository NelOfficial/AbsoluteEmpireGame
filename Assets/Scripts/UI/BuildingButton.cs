using UnityEngine;

public class BuildingButton : MonoBehaviour
{
    public BuildingScriptableObject currentBuilding;

    private UI_VerifyBuildingPanel uI_VerifyBuildingPanel;

    private void Awake()
    {
        uI_VerifyBuildingPanel = FindObjectOfType<UI_VerifyBuildingPanel>();
    }

    public void DestroyBuilding()
    {
        uI_VerifyBuildingPanel.currentBuilding = currentBuilding;
        uI_VerifyBuildingPanel.Show();
    }
}
