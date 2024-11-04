using TMPro;
using UnityEngine;
using UnityEngine.U2D.IK;
using UnityEngine.UI;

#region Item Class
public class Item
{
    public Item(string name, int price, bool isUnlocked, string description)
    {
        Name = name;
        Price = price;
        IsUnlocked = isUnlocked;
        Description = description;
    }

    public string Name { get; set; }
    public int Price { get; set; }
    public bool IsUnlocked { get; set; }
    public string Description { get; set; }
}
#endregion
public class ItemsManager : MonoBehaviour
{
    public Item[] Items = {
        new("sword",20,false,"change hero weapon to sword, deal much more damage"),
        new("bow",10,false,"equipped hero a bow, allow him to shoot from afar"),
        new("super dashing",30,false,"hero turn into a fire and move super quickly")
    };
    public Sprite[] itemsSprites;
    public GameObject player;
    private PlayerControl _playerScript;
    public Sprite[] btnSprites;
    public Image itemImg;
    public Button buyBtn;
    public GameObject itemDescription;
    public GameObject itemName;
    public int selectedItemIdx;
    private bool _enoughMoney;
    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        _playerScript = player.GetComponent<PlayerControl>();
        selectedItemIdx = 0;
        UpdateItem();
    }

    private void UpdateItem()
    {

        var desTxt = itemDescription.GetComponent<TextMeshProUGUI>();
        var nameTxt = itemName.GetComponent<TextMeshProUGUI>();
        var selectedItem = Items[selectedItemIdx];
        //change start 
        itemImg.sprite = itemsSprites[selectedItemIdx];
        desTxt.text = selectedItem.Description;
        nameTxt.text = selectedItem.Name + " - price: " + selectedItem.Price.ToString();
        if (selectedItem.IsUnlocked)
        {
            buyBtn.image.sprite = btnSprites[0];
            buyBtn.enabled = false;
        }
        else
        {
            _enoughMoney = _playerScript.Currency > selectedItem.Price;
            buyBtn.image.sprite = _enoughMoney ?
                                btnSprites[1] :
                                btnSprites[2];
            buyBtn.enabled = _enoughMoney;
        }

    }
    public void Next_Click()
    {
        Debug.Log("NextClickCalled");
        selectedItemIdx++;
        if (selectedItemIdx > Items.Length - 1) selectedItemIdx = 0;
        UpdateItem();
    }

    public void Previous_Click()
    {
        selectedItemIdx--;
        if (selectedItemIdx < 0) selectedItemIdx = Items.Length - 1;
        UpdateItem();
    }

    public void Buy_Click()
    {
        if (!_enoughMoney) return;
        Items[selectedItemIdx].IsUnlocked = true;
        _playerScript.Currency -= Items[selectedItemIdx].Price;
        UpdateItem();
    }

    // public void CloseClick() => this.gameObject.SetActive(false);
}


