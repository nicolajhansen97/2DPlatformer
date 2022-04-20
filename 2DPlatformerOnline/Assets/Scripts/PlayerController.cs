using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController : MonoBehaviourPunCallbacks
{

    [SerializeField] private LayerMask platformLayerMask;
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider2D;
    private float horInput;


    [Header("Movement Settings")]
    public float moveSpeed;
    public float acceleration;
    public float decceleration;
    public float velPower;
    public float frictionAmount;

    [Header("Jump Settings")]
    public float jumpCoyoteTime;
    public float jumpForce;
    public float jumpBufferTime;
    public float jumpCutMuliplier;

    public float lastJumpTime;
    public bool isJumping;
    public bool jumpInputReleased;
    public float lastGroundedTime = 0;

    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;



    //NICOLAJ LAVER VÅBEN DEL HERUNDER

    [Header("Weapon Settings")]
    [SerializeField]
    Item[] items;
    int itemIndex;
    int previousItemIndex = -1;

    public Transform Gun;
    Vector2 direction;

    //HER SLUTTER NICOLAJS VÅBENDEL

    void Awake()
    {
        // #Important
        // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
        if (photonView.IsMine)
        {
            PlayerController.LocalPlayerInstance = this.gameObject;
            EquipItem(0);
        }
        // #Critical
        // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider2D = transform.GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }

        horInput = Input.GetAxis("Horizontal");
        if (Input.GetKeyDown("space"))
        {
            lastJumpTime = jumpBufferTime;
        }
        if (!Input.GetKey("space"))
        {
            jumpInputReleased = true;
        }
        else
        {
            jumpInputReleased = false;
        }



        //NICOLAJS VÅBEN DEL

        for (int i = 0; i < items.Length; i++)
        {
            if(Input.GetKeyDown((i + 1).ToString()))
            {
                EquipItem(i);
                break;
            }
        }

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        direction = mousePos - (Vector2)Gun.position;
        FaceMouse();

       
        }

    void FaceMouse()
    {
        Gun.transform.right = direction;
    }

    //SLUT NICOLAJS VÅBEN DEL


    private void FixedUpdate()
    {
        #region Run


        float targetSpeed = horInput * moveSpeed;
        float speedDif = targetSpeed - rb.velocity.x;
        float accelRate;
        if (Mathf.Abs(targetSpeed) > 0.01f)
        {
            accelRate = acceleration;
        }
        else
        {
            accelRate = decceleration;
        }
        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);
        rb.AddForce(movement * Vector2.right);
        #endregion

        #region Jump
        if (isJumping && rb.velocity.y < 0)
            isJumping = false;

        if (lastGroundedTime > 0 && lastJumpTime > 0)
        {
            isJumping = true;
            Jump();
        }

        if (jumpInputReleased)
        {
            OnJumpUp();
        }
        #endregion

        #region Friction
        if (lastGroundedTime > 0 && Mathf.Abs(horInput) < 0.01f)
        {
            float amount = Mathf.Min(Mathf.Abs(rb.velocity.x), Mathf.Abs(frictionAmount));
            amount *= Mathf.Sign(rb.velocity.x);
            rb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
        }
        #endregion

        #region Timer
        lastGroundedTime -= Time.deltaTime;
        lastJumpTime -= Time.deltaTime;
        #endregion

        #region GroundCheck
        if (isGrounded())
        {
            lastGroundedTime = jumpCoyoteTime;

        }
        #endregion
    }

    private void Jump()
    {
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        lastGroundedTime = 0;
        lastJumpTime = 0;

    }

    private void OnJumpUp()
    {
        if (rb.velocity.y > 0 && isJumping)
        {
            rb.AddForce(Vector2.down * rb.velocity.y * (1 - jumpCutMuliplier), ForceMode2D.Impulse);
        }

        //jumpInputReleased = true;
        //lastJumpTime = 0;
    }

    private bool isGrounded()
    {
        //Overlap box is also an option here, not sure how it works but if we get errors look into it.
        float extraHeightText = .02f;
        //RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider2D.bounds.center, boxCollider2D.bounds.size, 0f, Vector2.down, extraHeightText, platformLayerMask);
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider2D.bounds.center, boxCollider2D.bounds.size - new Vector3(0.1f, 0f, 0f), 0f, Vector2.down, extraHeightText, platformLayerMask);

        Color rayColor;
        if (raycastHit.collider != null)
        {
            rayColor = Color.green;
        }
        else
        {
            rayColor = Color.red;
        }
        Debug.DrawRay(boxCollider2D.bounds.center + new Vector3(boxCollider2D.bounds.extents.x, 0), Vector2.down * (boxCollider2D.bounds.extents.y + extraHeightText), rayColor);
        Debug.DrawRay(boxCollider2D.bounds.center - new Vector3(boxCollider2D.bounds.extents.x, 0), Vector2.down * (boxCollider2D.bounds.extents.y + extraHeightText), rayColor);
        Debug.DrawRay(boxCollider2D.bounds.center - new Vector3(boxCollider2D.bounds.extents.x, boxCollider2D.bounds.extents.y + extraHeightText), Vector2.right * (boxCollider2D.bounds.extents.x * 2), rayColor);
        return raycastHit.collider != null;
    }



    //NICOLAJ LAVER VÅBEN DEL HERUNDER

    void EquipItem(int _index)
    {
        if (_index == previousItemIndex)
        {
            return;
        }
        itemIndex = _index;

        Debug.Log("Equiped " + itemIndex);
        items[itemIndex].itemGameObject.SetActive(true);


        if (previousItemIndex != -1)
        {
            items[previousItemIndex].itemGameObject.SetActive(false);
        }

        previousItemIndex = itemIndex;

        //Check if its local player
        if (photonView.IsMine)
        {
            //Send the local item over the network
            Hashtable hash = new Hashtable();
            hash.Add("itemIndex", itemIndex); ;
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }

       
     

    //Called when information is recieved.
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        //Synced if its not the local player
        if(!photonView.IsMine && targetPlayer == photonView.Owner)
        {
            EquipItem((int)changedProps["itemIndex"]);   
        }
    }





    //HER SLUTTER NICOLAJS VÅBENDEL

}
