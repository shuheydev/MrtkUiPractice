using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyScrollViewItemManager : MonoBehaviour
{
    public GameObject Prefab;

    private ScrollingObjectCollection scrollingObjectCollectionComponent;
    private GridObjectCollection gridObjectCollectionComponent;

    private void Start()
    {
        //get ScrollingObjectCollection componet
        scrollingObjectCollectionComponent = GetComponent<ScrollingObjectCollection>();
        //get GridObjectCollection componet from children of ScrollingObjectCollection component
        gridObjectCollectionComponent = scrollingObjectCollectionComponent.GetComponentInChildren<GridObjectCollection>();
    }

    public void AddNewItem()
    {
        //Create new item into GridObjectCollection component
        GameObject itemInstance = Instantiate(Prefab, gridObjectCollectionComponent.transform);
        itemInstance.SetActive(true);

        gridObjectCollectionComponent.UpdateCollection();
        scrollingObjectCollectionComponent.UpdateContent();
    }

    public void RemoveLastItem()
    {
        int lastItemIndex = gridObjectCollectionComponent.transform.childCount - 1;
        if (lastItemIndex < 0)
        {
            Debug.Log("No items in ScrollObjectCollection");
            return;
        }

        var target = gridObjectCollectionComponent.transform.GetChild(lastItemIndex);

        Destroy(target.gameObject);

        gridObjectCollectionComponent.UpdateCollection();
        scrollingObjectCollectionComponent.UpdateContent();
    }
}
