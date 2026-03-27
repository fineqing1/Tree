using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    [Header("Stats")]
    public int maxHp = 100;
    public int currentHP = 100;
    public int maxFuel = 100;
    public int currentFuel = 100;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;
}
