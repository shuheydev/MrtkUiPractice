using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyScrollViewItemManager : MonoBehaviour
{
    public GameObject Prefab;
    public GameObject TargetCollection;
    public GameObject TargetScrollingObjectCollectionObject;

    private ScrollingObjectCollection scrollingObjectCollectionComponent;
    private GridObjectCollection gridObjectCollectionComponent;

    public void AddNewItem()
    {
        //get ScrollingObjectCollection componet
        scrollingObjectCollectionComponent = TargetScrollingObjectCollectionObject.GetComponent<ScrollingObjectCollection>();

        //get GridObjectCollection componet from children of ScrollingObjectCollection component
        gridObjectCollectionComponent = scrollingObjectCollectionComponent.GetComponentInChildren<GridObjectCollection>();

        //Create new item into GridObjectCollection component
        GameObject itemInstance = Instantiate(Prefab, gridObjectCollectionComponent.transform);
        itemInstance.SetActive(true);

        gridObjectCollectionComponent.UpdateCollection();
        scrollingObjectCollectionComponent.UpdateContent();
    }
}
