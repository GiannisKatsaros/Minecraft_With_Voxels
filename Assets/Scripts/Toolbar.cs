using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Toolbar : MonoBehaviour
{
    private World world;
    [SerializeField] private Player player;

    [SerializeField] private RectTransform highlightBlock;
    [SerializeField] private ItemSlot[] itemSlots;

    private int slotIndex = 0;

    private void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        
        foreach (ItemSlot slot in itemSlots)
        {
            slot.icon.sprite = world.blockTypes[slot.itemId].icon;
            slot.icon.enabled = true;
        }
        player.selectedBlockIndex = itemSlots[slotIndex].itemId;
    }

    private void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0)
        {
            if (scroll > 0)
                slotIndex--;
            else
                slotIndex++;

            if (slotIndex > itemSlots.Length - 1)
                slotIndex = 0;
            else if (slotIndex < 0)
                slotIndex = itemSlots.Length - 1;

            highlightBlock.position = itemSlots[slotIndex].icon.transform.position;
            player.selectedBlockIndex = itemSlots[slotIndex].itemId;
        }
    }

}

[System.Serializable]
public class ItemSlot
{
    public byte itemId;
    public Image icon;
}