using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform camTransform;
    
    private void Update()
    {
        if (Input.GetKey(KeyCode.E))
        {
            Debug.Log("Hit attack controller");
            ClientSend.PlayerAttack(camTransform.forward);
        }
    }

    private void FixedUpdate()
    {
        SendInputToServer();
    }

    private void SendInputToServer()
    {
        bool[] _inputs = new bool[]
        {
            Input.GetKey(KeyCode.W),
            Input.GetKey(KeyCode.S),
            Input.GetKey(KeyCode.D),
            Input.GetKey(KeyCode.A),
            Input.GetKey(KeyCode.Space)
        };

        ClientSend.PlayerMovement(_inputs);
    }
    
}
