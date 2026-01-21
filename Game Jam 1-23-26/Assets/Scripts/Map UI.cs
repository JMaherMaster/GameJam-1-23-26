using UnityEngine;

public class HoldMapUI : MonoBehaviour
{
    [SerializeField] private GameObject mapUI;

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            mapUI.SetActive(true);
        }
        if (Input.GetKey(KeyCode.RightShift))
        {
            mapUI.SetActive(true);
        }
        if (Input.GetKey(KeyCode.Return))
        {
            mapUI.SetActive(true);
        }
        else
        {
            mapUI.SetActive(false);
        }
    }
}
