using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData", menuName = "Bestiary/Monster")]
public class MonsterData : ScriptableObject
{
    public string monsterName;
    public Sprite illustration;
    [TextArea] public string description;
    public bool imageUnlocked = false;
    public bool descriptionUnlocked = false;
}