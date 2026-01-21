using UnityEngine;

public class HoldMapUI : MonoBehaviour
{
    [SerializeField] private GameObject mapUI;

    void Update()
    {
        // Show map if ANY of these keys are held
        if (Input.GetKey(KeyCode.LeftShift) ||
            Input.GetKey(KeyCode.RightShift) ||
            Input.GetKey(KeyCode.Return))
        {
            mapUI.SetActive(true);
        }
        else
        {
            mapUI.SetActive(false);
        }
    }
}