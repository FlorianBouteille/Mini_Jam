using UnityEngine;
using System.Collections.Generic;

public class BestiaryManager : MonoBehaviour
{
    public static BestiaryManager Instance { get; private set; }

    public List<MonsterData> monsters = new List<MonsterData>();

    void Awake()
    {
        Instance = this;
    }

	void Start()
	{
		foreach (MonsterData monster in monsters)
		{
			monster.imageUnlocked = false;
			monster.descriptionUnlocked = false;
		}
	}       

    public void UnlockImage(string monsterName)
    {
        MonsterData data = monsters.Find(m => m.monsterName == monsterName);
        if (data != null)
            data.imageUnlocked = true;
    }

    public void UnlockDescription(string monsterName)
    {
        MonsterData data = monsters.Find(m => m.monsterName == monsterName);
        if (data != null)
            data.descriptionUnlocked = true;
    }
}