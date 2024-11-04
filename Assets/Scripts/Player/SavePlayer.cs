using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SavePlayer : MonoBehaviour
{
    private PlayerControl _player;
    private bool EscPressed => Input.GetKeyDown(KeyCode.Escape);
    void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (EscPressed) gameObject.SetActive(false);
    }
    public void Save()
    {
        CharacterData data = new()
        {
            position = _player.transform.position,
            health = _player.currentHealth,
            currency = _player.Currency,
            weapon = _player.Weapon,
            sideWeapon = _player.SideWeapon,
            superDash = _player.CanSuperDash,
            senceIdx = SceneManager.GetActiveScene().buildIndex
        };

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(Application.persistentDataPath + "/savefile.json", json);

    }

    public void Load()
    {
        string path = Application.persistentDataPath + "/savefile.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            CharacterData data = JsonUtility.FromJson<CharacterData>(json);

            if (SceneManager.GetActiveScene().buildIndex != data.senceIdx)
            {
                SceneManager.LoadScene(data.senceIdx);
                return;
            }
            //load data out
            _player.transform.position = data.position;
            _player.currentHealth = (sbyte)data.health;
            _player.Currency = data.currency;
            _player.Weapon = data.weapon;
            _player.SideWeapon = data.sideWeapon;
            _player.CanSuperDash = data.superDash;
        }
    }

    public void Exit()
    {
#if UNITY_EDITOR
        Debug.Log("Exit application");
        UnityEditor.EditorApplication.isPlaying = false; // Stop playing in the Editor
#else
        Application.Quit();
#endif
    }
}

[System.Serializable]
public class CharacterData
{
    public Vector3 position;
    public int health;
    public int currency;
    public bool weapon;
    public bool sideWeapon;
    public bool superDash;
    public int senceIdx;
}