using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SeaRegion : MonoBehaviour
{
	public enum SeaTerrain
	{
		Shallowwater,
		Deepwater,
		Fjords_and_archipelagos,
	}

    public int _id;
	public SeaTerrain _terrain;

    private bool _isSelected = false;
    [HideInInspector] public SeaRegion _currentSeaRegion;

    [Space(10f)]
    [Header("RegionArmy")]
    public List<SeaMovePoint> _movePoints = new List<SeaMovePoint>();

    public List<Fleet> _fleets = new List<Fleet>();

    private Vector3 StartPos;
    private Vector3 PosAfter;


    private void OnMouseDown()
    {
        StartPos = ReferencesManager.Instance.mainCamera.GetComponent<Camera>().WorldToViewportPoint(Input.mousePosition);
    }

    private void OnMouseEnter()
    {
        //if (Application.platform == RuntimePlatform.WindowsPlayer)
        //{
        //    if (ReferencesManager.Instance.mapType != null)
        //    {
        //        if (!ReferencesManager.Instance.mapType.viewMap)
        //        {
        //            this.GetComponent<SpriteRenderer>().color = hoverColor;
        //        }
        //    }
        //}
    }

    private void OnMouseExit()
    {
        //if (Application.platform == RuntimePlatform.WindowsPlayer)
        //{
        //    if (ReferencesManager.Instance.mapType != null)
        //    {
        //        if (!ReferencesManager.Instance.mapType.viewMap)
        //        {
        //            if (currentRegionManager != null && currentRegionManager == this.GetComponent<RegionManager>())
        //            {
        //                this.GetComponent<SpriteRenderer>().color = currentRegionManager.selectedColor;
        //            }
        //            else if (currentRegionManager == null)
        //            {
        //                this.GetComponent<SpriteRenderer>().color = this.GetComponent<RegionManager>().currentCountry.country.countryColor;
        //            }
        //            else if (currentRegionManager != null && currentRegionManager != this.GetComponent<RegionManager>())
        //            {
        //                this.GetComponent<SpriteRenderer>().color = this.GetComponent<RegionManager>().currentCountry.country.countryColor;
        //            }
        //        }
        //    }
        //}
    }

    private void OnMouseUpAsButton()
    {
        PosAfter = ReferencesManager.Instance.mainCamera.GetComponent<Camera>().WorldToViewportPoint(Input.mousePosition);

        float difference_x = (PosAfter.x - StartPos.x);
        float difference_y = (PosAfter.y - StartPos.y);

        int cameraClickOffset = ReferencesManager.Instance.mainCamera.cameraClickOffset;

        // ���� ������� �� ���������� �� 32, �� ��������� ����
        if (difference_x > -cameraClickOffset && difference_x < cameraClickOffset && difference_y > -cameraClickOffset && difference_y < cameraClickOffset)
        {
            // *���������� ����
            if (!ReferencesManager.Instance.mainCamera.IsMouseOverUI)
            {
                if (ReferencesManager.Instance.mainCamera.Map_MoveTouch_IsAllowed)
                {
                    if (!ReferencesManager.Instance.gameSettings.regionSelectionMode)
                    {
                        SelectRegion();
                    }
                }
            }
        }
    }

    public void SelectRegion()
    {
        ReferencesManager.Instance.regionManager.DeselectRegions();

        ReferencesManager.Instance.regionUI.regionUIContainer.GetComponent<UI_Panel>().ClosePanel();

        Vector2 mainCamera = ReferencesManager.Instance.mainCamera.GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mainCamera, Input.mousePosition);

        if (hit.collider)
        {
            if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                if (Input.touchCount > 0)
                {
                    Touch touch = Input.GetTouch(0);

                    if (touch.phase == TouchPhase.Began)
                    {
                        var touchPostition = touch.position;

                        bool isOverUI = touchPostition.IsPointerOverGameObject();

                        if (isOverUI)
                        {
                            return;
                        }
                    }
                }
            }

            foreach (SeaRegion region in ReferencesManager.Instance.gameSettings._seaRegions)
            {
                region._isSelected = false;
                region._currentSeaRegion = hit.collider.GetComponent<SeaRegion>();
                region.GetComponent<SpriteRenderer>().color = ReferencesManager.Instance.gameSettings._seaDefaultColor;
            }

            ReferencesManager.Instance.regionUI.CloseTabs();

            if (UISoundEffect.Instance != null)
            {
                UISoundEffect.Instance.PlayAudio(ReferencesManager.Instance.regionUI.click_01);
            }

            _currentSeaRegion.GetComponent<SpriteRenderer>().color = ReferencesManager.Instance.gameSettings._seaSelectedColor;
            _currentSeaRegion._isSelected = true;

            //ReferencesManager.Instance.regionUI.regionBarContainer.SetActive(true);
            //ReferencesManager.Instance.regionUI.regionUIContainer.SetActive(true);
        }
    }

    public void DeselectRegions()
    {
        foreach (SeaRegion region in ReferencesManager.Instance.gameSettings._seaRegions)
        {
            region._isSelected = false;
            region.GetComponent<SpriteRenderer>().color = ReferencesManager.Instance.gameSettings._seaDefaultColor;
            region._currentSeaRegion = null;
        }
    }
}
