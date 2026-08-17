using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using System.Reflection;
using UnityEngine;

namespace No_Time_To_Waste;

public class No_Time_To_Waste : ModBehaviour
{
    public static No_Time_To_Waste Instance;
    public INewHorizons NewHorizons;
    private GameObject ourea;
    private GameObject silvanus;
    private GameObject rest;
    public static bool heartb = false;
    public static bool hinsert = false;
    public static bool cinsert = false;
    public static bool vinsert = false;
    public static bool future1 = false;

    public void Awake()
    {
        Instance = this;
        GlobalMessenger<string, bool>.AddListener("DialogueConditionChanged", OnDialogueConditionChanged);
    }

    public void Start()
    {
        ModHelper.Console.WriteLine($"My mod {nameof(No_Time_To_Waste)} is loaded!", MessageType.Success);

        NewHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
        NewHorizons.LoadConfigs(this);

        new Harmony("RiddleMajor.No_Time_To_Waste").PatchAll(Assembly.GetExecutingAssembly());

        NewHorizons.GetStarSystemLoadedEvent().AddListener(OnStarSystemLoaded);
    }

    private void OnStarSystemLoaded(string systemName)
    {
        ourea = GameObject.Find("OureasPeak_Body");
        silvanus = GameObject.Find("SilvanusPeak_Body");
        rest = GameObject.Find("KanaloasRest_Body");
        if (systemName == "Kronos' Inferno")
        {
            NewHorizons.SetDefaultSystem("Future");
            FixScroll();
            FixDoor();
            PlayerData.SetPersistentCondition("BrevipesConvinced", false);
            if (future1)
            {
                Locator.GetShipLogManager().RevealFact("FutureEnd");
            }
        }
        else if (systemName == "Future")
        {
            NewHorizons.SetDefaultSystem("Kronos' Inferno");
            FixMachine();
            HideMusic();
            if (heartb)
            {
                Invoke(nameof(ReturnHeart), 0.1f);
            }
            if (!future1)
            {
                future1 = true;
            }
                
            
        }

    }

    private void OnDialogueConditionChanged(string conditionName, bool conditionState)
    {
        if (conditionName == "VaultKeyed" && conditionState)
        {
            Invoke(nameof(OpenVaultDoor), 1f);
        }
        if (conditionName == "ADDInserted" && conditionState)
        {
            Locator.GetDeathManager().KillPlayer(DeathType.Default);
        }
        if (conditionName == "HeartInsert" && conditionState)
        {
            hinsert = true;
            if (cinsert && vinsert)
            {
                ActivateMachine();
            }
        }
        if (conditionName == "HeartInsert" && !conditionState)
        {
            hinsert = false;
        }
        if (conditionName == "VialInsert" && conditionState)
        {
            vinsert = true;
            if (cinsert && hinsert)
            {
                ActivateMachine();
            }
        }
        if (conditionName == "VialInsert" && !conditionState)
        {
            vinsert = false;
        }
        if (conditionName == "CrystalInsert" && conditionState)
        {
            cinsert = true;
            if (hinsert && vinsert)
            {
                ActivateMachine();
            }
        }
        if (conditionName == "CrystalInsert" && !conditionState)
        {
            cinsert = false;
        }
    }

    private void OpenVaultDoor()
    {
        Transform vault = ourea.transform.Find("Sector/Ourea's Peak/VaultDoor");
        Transform text = vault.Find("Wall_Corner_BOTH (3)/WS_Scroll_Hole/Socket/Prefab_NOM_Scroll/NomaiWallText");

           NomaiWallText x = text.GetComponent<NomaiWallText>();
           if (x._nomaiTextAsset.name == "Clue5")
           {
               vault.GetComponent<Animation>().Play("vaultdoor");
           }
    }

    private void ActivateMachine()
    {
        Transform machine = rest.transform.Find("Sector/machine");
        machine.GetComponent<Animation>().Play("machine");
        ActivateMusic();
        Invoke(nameof(stupid), 0.1f);
    }

    private void stupid()
    {
        DialogueConditionManager.SharedInstance.SetConditionState("MachineOn", true);
    }

    private void HideMusic()
    {
        Transform sector = rest.transform.Find("Sector");
        int count = 0;
        foreach (Transform child in sector)
        {
            if (child.name == "AudioVolume")
            {
                count++;

                if (count == 5)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
    }

    private void ActivateMusic()
    {
        Transform sector = rest.transform.Find("Sector");
        int count = 0;
        foreach (Transform child in sector)
        {
            if (child.name == "AudioVolume")
            {
                count++;

                if (count == 5)
                {
                    child.gameObject.SetActive(true);
                }
            }
        }
    }

    private void FixScroll()
    {
        Transform sector = ourea.transform.Find("Sector");
        int count = 0;
        foreach (Transform child in sector)
        {
            if (child.name == "Prefab_NOM_Scroll")
            {
                count++;

                if (count == 1 || count == 3)
                {
                    child.GetComponent<CapsuleCollider>().radius = 1.5f;
                }
            }
        }
    }

    private void FixDoor()
    {
        Animation anim = ourea.transform.Find("Sector/Ourea's Peak/VaultDoor").GetComponent<Animation>();
        AnimationState state = anim["vaultdoor"];

        state.time = 0f;
        anim.Play("vaultdoor");
        anim.Sample();
        anim.Stop();
    }

    private void FixMachine()
    {
        Animation anim = rest.transform.Find("Sector/machine").GetComponent<Animation>();
        AnimationState state = anim["machine"];

        state.time = 0f;
        anim.Play("machine");
        anim.Sample();
        anim.Stop();
    }

    private void ReturnHeart()
{
        Transform heart = silvanus.transform.Find("Sector/Heart");
        OWItem item = heart.GetComponent<OWItem>();
        ToolModeSwapper swapper = Object.FindObjectOfType<ToolModeSwapper>();
        ItemTool itemTool = swapper.GetItemCarryTool();
        swapper.EquipToolMode(ToolMode.Item);
        itemTool.PickUpItemInstantly(item);
        heartb = false;
    }

    [HarmonyPatch(typeof(OWItem), nameof(OWItem.PickUpItem))]
    public static class OWItem_PickUpItem_Patch
    {
        public static void Postfix(OWItem __instance)
        {
            if (__instance.gameObject.name.Contains("Heart"))
            {
                heartb = true;
            }
        }
    }

    [HarmonyPatch(typeof(ItemTool), nameof(ItemTool.DropItem))]
    public static class ItemTool_DropItem_Patch
    {
        public static void Prefix(ItemTool __instance)
        {
            OWItem item = __instance.GetHeldItem();
            if (item.gameObject.name.Contains("Heart"))
            {
                heartb = false;
            }
        }
    }
}