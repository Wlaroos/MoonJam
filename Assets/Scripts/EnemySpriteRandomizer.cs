using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpriteRandomizer : MonoBehaviour
{
    // Weights for each rarity
    [SerializeField] private float baseWeight = 0.85f;
    [SerializeField] private float uncommonWeight = 0.1f;
    [SerializeField] private float rareWeight = 0.05f;

    private Sprite[] baseHeads;
    private Sprite[] baseBodies;
    private Sprite[] uncommonHeads;
    private Sprite[] uncommonBodies;
    private Sprite[] rareHeads;
    private Sprite[] rareBodies;

private void Start()
{
    // Load sprites from Resources folder
    baseHeads = Resources.LoadAll<Sprite>("BaseHeads");
    baseBodies = Resources.LoadAll<Sprite>("BaseBodies");
    uncommonHeads = Resources.LoadAll<Sprite>("UncommonHeads");
    uncommonBodies = Resources.LoadAll<Sprite>("UncommonBodies");
    rareHeads = Resources.LoadAll<Sprite>("RareHeads");
    rareBodies = Resources.LoadAll<Sprite>("RareBodies");

    // Determine rarity and assign sprites
    string rarity = GetRandomRarity();
    int selectedIndex;

    Sprite selectedHead = GetRandomSpriteFromRarity(rarity, baseHeads, uncommonHeads, rareHeads, out selectedIndex);
    Sprite selectedBody;

    if (rarity == "Rare" && selectedIndex != -1)
    {
        // Use the same index for the body if the rarity is "Rare"
        selectedBody = rareBodies.Length > selectedIndex ? rareBodies[selectedIndex] : null;
    }
    else
    {
        // Otherwise, pick a random body sprite
        selectedBody = GetRandomSpriteFromRarity(rarity, baseBodies, uncommonBodies, rareBodies, out _);
    }

    // Assign the selected sprites to SpriteRenderers
    Transform spriteHolder = transform.Find("SpriteHolder");
    SpriteRenderer headRenderer = spriteHolder.Find("Head").GetComponent<SpriteRenderer>();
    SpriteRenderer bodyRenderer = spriteHolder.Find("Body").GetComponent<SpriteRenderer>();

    if (headRenderer != null) headRenderer.sprite = selectedHead;
    if (bodyRenderer != null) bodyRenderer.sprite = selectedBody;
}

    private string GetRandomRarity()
    {
        float totalWeight = baseWeight + uncommonWeight + rareWeight;
        float randomValue = Random.Range(0f, totalWeight);

        if (randomValue < baseWeight)
        {
            return "Base";
        }
        else if (randomValue < baseWeight + uncommonWeight)
        {
            return "Uncommon";
        }
        else
        {
            return "Rare";
        }
    }

    private Sprite GetRandomSpriteFromRarity(string rarity, Sprite[] baseArray, Sprite[] uncommonArray, Sprite[] rareArray, out int selectedIndex)
    {
        selectedIndex = -1; // Default index
        switch (rarity)
        {
            case "Base":
                return GetRandomFromArray(baseArray, out selectedIndex);
            case "Uncommon":
                return GetRandomFromArray(uncommonArray, out selectedIndex);
            case "Rare":
                return GetRandomFromArray(rareArray, out selectedIndex);
            default:
                return null;
        }
    }

    private Sprite GetRandomFromArray(Sprite[] spriteArray, out int selectedIndex)
    {
        if (spriteArray.Length == 0)
        {
            selectedIndex = -1;
            return null;
        }
        selectedIndex = Random.Range(0, spriteArray.Length);
        return spriteArray[selectedIndex];
    }
}
