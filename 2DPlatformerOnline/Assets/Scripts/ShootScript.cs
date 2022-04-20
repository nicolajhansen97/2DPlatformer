using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootScript : MonoBehaviourPun
{

    public Transform Gun;
    public Animator gunAnimator;

    Vector2 direction;

    public GameObject Bullet;

    public float BulletSpeed;
    public Transform ShootPoint;
    public float fireRate;
    float ReadyForNextShot;

    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;

    void Awake()
    {
        // #Important
        // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
        if (photonView.IsMine)
        {
            PlayerController.LocalPlayerInstance = this.gameObject;
        }
        // #Critical
        // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
        DontDestroyOnLoad(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        direction = mousePos - (Vector2)Gun.position;
        FaceMouse();

        if (Input.GetMouseButton(0))
        {
            if (Time.time > ReadyForNextShot)
            {
                ReadyForNextShot = Time.time + 1 / fireRate;
                shoot();
            }
        }
    }

    void FaceMouse()
    {
        Gun.transform.right = direction;
    }
    void shoot()
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }

        GameObject BulletIns = Instantiate(Bullet, ShootPoint.position, ShootPoint.rotation);
        BulletIns.GetComponent<Rigidbody2D>().AddForce(BulletIns.transform.right * BulletSpeed);
        gunAnimator.SetTrigger("Shoot");
        Destroy(BulletIns, 3);
    }
}

