using UnityEngine;

public class DisableAction : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            PlayerController.Instance.ActionsEnabled = false;
            if (PlayerController.Instance.State == State.hiding)
                PlayerController.Instance.ExitState(State.hiding);

            if (PlayerController.Instance.State == State.digging)
                PlayerController.Instance.ExitState(State.digging);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
            PlayerController.Instance.ActionsEnabled = true;
    }
}
