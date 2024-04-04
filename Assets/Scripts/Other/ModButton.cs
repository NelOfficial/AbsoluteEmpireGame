using UnityEngine;

public class ModButton : MonoBehaviour
{
    public int id;

    private ModificationPanel modificationPanel;

    private void Awake()
    {
        modificationPanel = FindObjectOfType<ModificationPanel>();
    }

    public void Select()
    {
        

        foreach (ModificationPanel.Modification mod in modificationPanel.loadedModifications)
        {
            if (mod.id == id)
            {
                modificationPanel.currentLoadedModification = mod;
                modificationPanel.UpdateModUI();
            }
        }
    }

    public void PlayLocalMod()
    {
        
    }
}
