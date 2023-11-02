using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LootLocker.Requests;

public class ButtonPurchaseManager : MonoBehaviour
{
    private string playerULID;
    private string walletID;

    private Button button;

    private PurchaseManager purchaseManager;

    public string listingID;
    public string assetID;
    
    void Awake()
    {
        playerULID = purchaseManager.playerULID;
        walletID = purchaseManager.walletID;
        
        CheckItemStatus();

        button = GetComponent<Button>();
        button.onClick.AddListener(PurchaseItem);
    }
    
    public void CheckItemStatus()
    {
        //Check if they own the item, if they dont, then show the purchase button, if they do, hide it.
    }    

    public void PurchaseItem()
    {
        //Show a confirmation prompt.
        
        LootLockerCatalogItemAndQuantityPair[] items = { new LootLockerCatalogItemAndQuantityPair { catalog_listing_id = listingID, quantity = 1 } };

        LootLockerSDKManager.LootLockerPurchaseCatalogItems(walletID, items, (response) =>
        {
            if (!response.success)
            {
                //Show a failed screen.
                
                Debug.Log("error: " + response.errorData.message);
                Debug.Log("request ID: " + response.errorData.request_id);
                return;
            }
        });
    }
}
