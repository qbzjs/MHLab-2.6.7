using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;

public class PurchaseManager : MonoBehaviour
{
    [SerializeField] public string playerULID;
    [SerializeField] public string walletID;
    
    void Awake()
    {
        playerULID = PlayerPrefs.GetString("PlayerULID");
        
        LootLocker.LootLockerEnums.LootLockerWalletHolderTypes player = LootLocker.LootLockerEnums.LootLockerWalletHolderTypes.player;
        LootLockerSDKManager.GetWalletByHolderId(playerULID, player, (response) =>
        {
            if(!response.success)
            {
                //If wallet is not found, it will automatically create one on the holder.
                Debug.Log("error: " + response.errorData.message);
                Debug.Log("request ID: " + response.errorData.request_id);
                return;
            }
            
            walletID = response.id;
        
        });
    }
}
