using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEditor;
using System;

public class CameraMovement : MonoBehaviour
{
    /* есть локальная переменная IsMouseOverUI
      она говорит о том нажимает ли игрок на элемиент UI
      если IsMouseOverUI == true, то передвижение/зум не разрешаются
     */

    [Header("Map settings")]
    public Camera Cam; // основная камера, назначается в инспекторе
    public float Map_CloseUpSpeed; // сила зума, у меня в инспекторе 0.1
    private Vector3 MapMoving_StartTouch; // позиция куда игрок нажал
    private Vector3 MapMoving_TargetTouch; // позиция куда игрок двигает, за ней следует камера
    public float inerciaSpeed; // скорость инерции не понмю чего, но в инспекторе стоит значение 8
    public float CameraSpeed = 20f; // скорость инерции не понмю чего, но в инспекторе стоит значение 8
    [Header("   ")]
    public bool Map_MoveTouch_IsAllowed; // показывает разрешены ли клики по карте, и передвижение карты
    public bool IsMouseOverUI; // показывает то, нажимает ли игрок на UI, если да, то 2 переменных выше становятся false

    public int cameraClickOffset;

    private float targetPositionX;
    private float targetPositionY;

    [SerializeField] float cameraOffset;

    public List<RegionInfoCanvas> regionInfos = new List<RegionInfoCanvas>();


    // Update is called once per frame
    void Update()
    {      
        // метод определяет нажимает/наводит ли игрок на UI (ДЛЯ ТЕЛЕФОНОВ)
        foreach (Touch touch in Input.touches)
        {
            int id = touch.fingerId;
            if (IsMouseOverUI == false &&
                EventSystem.current.IsPointerOverGameObject(id))
            {
                IsMouseOverUI = true;
            }
            else
            {
                IsMouseOverUI = false;
            }
        }

        // метод определяет нажимает/наводит ли игрок на UI (ДЛЯ ЭДИТОРА)
        #if UNITY_EDITOR
            if (EventSystem.current.IsPointerOverGameObject())
            {
                IsMouseOverUI = true;
            }
            else
            {
                IsMouseOverUI = false;
            }
        #endif


        // ПЕРЕМЕЩЕНИЕ КАМЕРЫ
        #region MAP MOVING
        // разрешить перемещение и клики по карте, если косаний нет
        if (!Map_MoveTouch_IsAllowed && Input.touchCount == 0 ||
            !Map_MoveTouch_IsAllowed && !Input.GetMouseButton(0)
            )
        {
            Map_MoveTouch_IsAllowed = true;
        }
        // заблокировать перемещение и клики по карте, если косаний больше 2, или если игрок нажимает на UI
        if (Map_MoveTouch_IsAllowed && Input.touchCount >= 2 ||
            Map_MoveTouch_IsAllowed && IsMouseOverUI
            )
        {
            Map_MoveTouch_IsAllowed = false;
        }

        // получение стартовой позиции
        if (Input.GetMouseButtonDown(0) && Map_MoveTouch_IsAllowed)
        {
            MapMoving_StartTouch = Cam.ScreenToWorldPoint(Input.mousePosition);
        }
        // само передвижение, камера движется за нажанием
        //else if (Input.GetMouseButton(0) && Map_MoveTouch_IsAllowed)
        //{
        //    CameraSpeed = 20f;
        //    MapMoving_TargetTouch = MapMoving_StartTouch - Cam.ScreenToWorldPoint(Input.mousePosition);
        //    transform.position += MapMoving_TargetTouch * CameraSpeed * Time.deltaTime;
        //}
        if (Input.GetMouseButtonDown(0))
        {
            if (Map_MoveTouch_IsAllowed)
            {
                MapMoving_StartTouch = Cam.ScreenToWorldPoint(Input.mousePosition);
            }
        }
        else if (Input.GetMouseButton(0))
        {
            if (Map_MoveTouch_IsAllowed)
            {
                float posX = Cam.ScreenToWorldPoint(Input.mousePosition).x - MapMoving_StartTouch.x;
                float posY = Cam.ScreenToWorldPoint(Input.mousePosition).y - MapMoving_StartTouch.y;

                targetPositionX = Mathf.Clamp(transform.position.x - posX, -cameraOffset, cameraOffset);
                targetPositionY = Mathf.Clamp(transform.position.y - posY, -cameraOffset, cameraOffset);
            }
        }

        transform.position = new Vector3(Mathf.Lerp(
            transform.position.x,
            targetPositionX,
            CameraSpeed * Time.deltaTime),

            Mathf.Lerp(transform.position.y,
            targetPositionY,
            CameraSpeed * Time.deltaTime),
            transform.position.z);

        #endregion
        // ПРИБЛИЖЕНИЕ И ОТДАЛЕНИЕ КАМЕРЫ
        #region MAP CLOSE UP
        // рабоает если: передвижение не разрещено, игрок не ведет по UI
        if (!Map_MoveTouch_IsAllowed
            && !IsMouseOverUI)
        {
            if (Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.IPhonePlayer)
            {
                // если касаний больше 2
                if (Input.touchCount >= 2)
                {
                    Touch TouchZero = Input.GetTouch(0);
                    Touch TouchOne = Input.GetTouch(1);

                    Vector3 TouchZero_Pos = TouchZero.position - TouchZero.deltaPosition;
                    Vector3 TouchOne_Pos = TouchOne.position - TouchOne.deltaPosition;

                    float PrevMagnitude = (TouchZero_Pos - TouchOne_Pos).magnitude;
                    float CurrentMagnitude = (TouchZero.position - TouchOne.position).magnitude;

                    float MagnitudesDifference = CurrentMagnitude - PrevMagnitude;

                    // вызов метода зума
                    Map_Zooming(MagnitudesDifference * 0.01f);
                }
            }
        }
        if (!IsMouseOverUI)
        {
            if (Application.platform == RuntimePlatform.WindowsPlayer ||
            Application.platform == RuntimePlatform.WindowsEditor)
            {
                Map_Zooming(Input.GetAxis("Mouse ScrollWheel"));
            }
        }

        #endregion
    }

    private void Map_Zooming(float increment)
    {
        Cam.orthographicSize = Mathf.Clamp(Cam.orthographicSize - (increment * Map_CloseUpSpeed), 2.5f, 10f);

        for (int i = 0; i < regionInfos.Count; i++)
        {
            if (regionInfos[i] != null)
            {
                regionInfos[i].UpdateSize();
            }
        }
    }
}
