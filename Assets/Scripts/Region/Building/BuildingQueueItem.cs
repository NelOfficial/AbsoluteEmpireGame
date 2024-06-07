using UnityEngine;
using UnityEngine.UI;

public class BuildingQueueItem : MonoBehaviour
{
    public Image progressbar;
    public RegionManager.BuildingQueueItem buildingQueueItem;

    /*
    ⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
    ⣿⣿⣿⣿⣿⣿⣿⣿⡿⠿⠛⠛⠉⠉⠉⠀⠀⠀⠉⠉⠉⠛⠛⠛⠿⣿⣿⣿⣿⣿
    ⣿⣿⣿⣿⠿⠋⠉⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠛⢿⣿⣿
    ⣿⣿⡆⢀⣤⡄⠀⠀⠀⠀⠀⠀⠀⠀⣠⡄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣠⡄⣿⣿
    ⣿⣿⣇⡀⠉⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡼⠟⢫⣿⣿
    ⣿⣿⣿⣿⣿⣶⣄⣀⣀⣀⠀⣀⣠⣤⡤⠤⠀⠀⠀⠀⠀⠀⠀⡴⠊⠁⠀⣼⣿⣿
    ⣿⣿⣿⡇⠀⠉⠙⠻⠿⠿⠿⣿⣿⡀⠀⠀⠀⠀⠀⠀⠀⢀⡴⠁⠀⠀⢠⣿⣿⣿
    ⣿⣿⣿⠁⠀⠀⠀⠀⠀⢀⣾⣿⣿⣿⣶⣤⣀⡀⠀⠀⠶⣿⣷⣶⣶⣿⣿⣿⣿⣿
    ⣿⣿⣿⣄⣀⣀⣀⣤⣶⣿⣿⣿⣿⣿⣿⣿⣿⣷⣶⣶⣾⣿⣿⣿⣿⣿⣿⣿⣿⣿
    ⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
    ⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
    */

    public void UpdateUI()
    {
        float value = (float)buildingQueueItem.movesLasts / (float)buildingQueueItem.building.moves;
        progressbar.fillAmount = value;
    }

    public void Evdokim()
    {
        UpdateUI();

        if (buildingQueueItem.movesLasts <= 0)
        {
            CancelBuilding();

            buildingQueueItem.region.BuildBuilding(buildingQueueItem.building, buildingQueueItem.region, true);
        }
    }

    public void CancelBuilding()
    {
        buildingQueueItem.region.buildingsQueue.Remove(buildingQueueItem);
        Destroy(this.gameObject);
    }
}
