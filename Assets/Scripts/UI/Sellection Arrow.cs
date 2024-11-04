using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SellectionArrow : MonoBehaviour
{
    [SerializeField] private RectTransform[] options;
    private RectTransform rect;
    private int currentPosition;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();

    }

    private void Update()
    {
        //change position of the selection arrow
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) {
            ChangePosition(-1);
        } else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            ChangePosition(-1);
        }

        //Interact with current option
        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.E))
            Interact();

    }
    private void ChangePosition(int _change)
    {
        currentPosition += _change;
        if(currentPosition < 0)
        {
            currentPosition = options.Length -1;
        }
        else if (currentPosition > options.Length - 1)
        {
            currentPosition = 0;
        }


        //assign the Y posiotion of current option to the arrow (move up/down)
        rect.position = new Vector3(rect.position.x, options[currentPosition].position.y, 0);
    }

    //private void AssignPosition()
    //{
    //    //Assign the Y position of the current option to the arrow (basically moving it up and down)
    //    arrow.position = new Vector3(arrow.position.x, buttons[currentPosition].position.y);
    //}
    private void Interact()
    {

        //Access the button component on each option and call its function
        options[currentPosition].GetComponent<Button>().onClick.Invoke();
    }
}
