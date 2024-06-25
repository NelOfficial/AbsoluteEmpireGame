using System.Collections.Generic;
using UnityEngine;

public class Aviation_Storage : MonoBehaviour
{
    public List<Aviation_Cell> planes = new List<Aviation_Cell>();

    void Buy(Aviation_ScriptableObj airplane, CountrySettings owner)
    {
        if (planes.Count < 4)
        {
            if (owner.money >= airplane.price)
            {
                planes.Add(new Aviation_Cell(airplane, owner));
                owner.money -= airplane.price;
            }
        }
        else
        {

        }
    }
    bool CheckFighter()
    {
        foreach (Aviation_Cell cell in planes)
        {
            if (cell.AirPlane.type == Aviation_ScriptableObj.Type.fighter)
            {
                return true;
            }
        }
        return false;
    }
}

[System.Serializable]
public class Aviation_Cell
{
    public Aviation_ScriptableObj AirPlane;
    public CountrySettings Owner;
    public float hp;
    public float fuel;

    public Aviation_Cell(Aviation_ScriptableObj airplane, CountrySettings owner)
    {
        AirPlane = airplane;
        fuel = 0;
        Owner = owner;   
        hp = airplane.maxhp;
    }
}
