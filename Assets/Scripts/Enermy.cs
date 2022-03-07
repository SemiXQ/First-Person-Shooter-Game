using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enermy : MonoBehaviour
{
    // Start is called before the first frame update
    public Animator animator;
    public GameObject[] targets;
    public bool isDead;
    public int dist_num;
    public bool isDetected;
    public float dist_to_player;
    public GameObject player;
    float gunShotTime = 0.5f; // 0.4f
    public GameObject end, start;
    public GameObject bulletHole;
    public GameObject muzzleFlash;
    public GameObject shotSound;
    public float damage = 30;
    public bool beingShoot;
    public float health = 100;
    public GameObject gun;
    private bool crash = false;
    public int enermyNumber;
    void Start()
    {
        //Invoke("Reloading", 2.0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isDead && !player.GetComponent<Gun>().isWin)
        {
            dist_to_player = Vector3.Distance(player.transform.position, transform.position);
            if (!isDetected && !beingShoot)
            {
                Invoke("MoveRandom", 2.0f);
                if (enermyNumber != null){
                    // This is used for decide which room's enermy should detect the player.
                    // e.g. If player in room2, then enemy in room 1 or room 3 should not detect player.
                    if (-60+40*enermyNumber < player.transform.position.z && player.transform.position.z < -20 + 40 * enermyNumber)
                    {
                        Detect_Player();
                    }
                }
            }
            else
            {
                if (dist_to_player > 10)
                {
                    animator.SetTrigger("run_trigger");
                    Invoke("Run", 2.0f);
                }
                else
                {
                    animator.SetTrigger("aim_trigger");
                    if (!player.GetComponent<Gun>().isDead)
                    {
                        Invoke("Shoot", 0.3f);
                    }
                }
            }
        }
        else if (isDead) {
            animator.SetTrigger("die_trigger");
            Invoke("gunLeave", 1.0f);
        }
    }

    // If enemy is alive, player doesn't win, enemy doesn't detect player, and enemy isn't being shoot, then enemy wandering in the room, from target 1->2->3->4->1 as a loop.
    void MoveRandom()
    {
        float dist0 = Vector3.Distance(targets[0].transform.position, transform.position);
        float dist1 = Vector3.Distance(targets[1].transform.position, transform.position);
        float dist2 = Vector3.Distance(targets[2].transform.position, transform.position);
        float dist3 = Vector3.Distance(targets[3].transform.position, transform.position);
        if (dist0 < 5)
        {
            dist_num = 1;
        }
        if(dist1 < 5)
        {
            dist_num = 2;
        }
        if (dist2 < 5)
        {
            dist_num = 3;
        }
        if (dist3 < 5)
        {
            dist_num = 0;
        }

        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(targets[dist_num].transform.position - transform.position), Time.deltaTime*0.2f);
    }

    void Detect_Player() 
    {
        float angle_with_player = Vector3.Angle(player.transform.position - transform.position, transform.forward);
        if (angle_with_player < 60) {
            isDetected = true;
            transform.rotation = Quaternion.LookRotation(player.transform.position - transform.position);
            animator.SetTrigger("run_trigger");
        }
    }

    void Run() {
        transform.rotation = Quaternion.LookRotation(player.transform.position - transform.position);
        transform.position = transform.position + transform.forward * Time.deltaTime * 0.1f;
    }

    void ShootDetection() {
        RaycastHit rayHit;
        int layerMask = 1 << 8;
        layerMask = ~layerMask;
        if (Physics.Raycast(end.transform.position, (end.transform.position - start.transform.position).normalized, out rayHit, 100.0f, layerMask))
        {
            GameObject bulletHoleObject = Instantiate(bulletHole, rayHit.point + rayHit.collider.transform.up * 0.01f, rayHit.collider.transform.rotation);
            if (rayHit.collider.tag == "Player")
            {
                AddEffects();
            }
            Destroy(bulletHoleObject, 0.5f);
        }

        GameObject muzzleFlashObject = Instantiate(muzzleFlash, end.transform.position, end.transform.rotation);
        muzzleFlashObject.GetComponent<ParticleSystem>().Play();
        Destroy(muzzleFlashObject, 1.0f);

        Destroy((GameObject)Instantiate(shotSound, transform.position, transform.rotation), 1.0f);
    }

    void Shoot() {
        //cool down
        if (gunShotTime >= 0.0f)
        {
            gunShotTime -= Time.deltaTime;
        }
        if (gunShotTime <= 0.0f) {
            float rd = Random.Range(0f, 1f);
            // 80% chance to shoot in another direction (with random value), 20% chance to shoot accurate
            if (rd > 0.6 || rd < 0.4)
            {
                var randomAngle = new Vector3(1.0f + rd, 0.0f, 0.0f);
                transform.rotation = Quaternion.LookRotation(player.transform.position - transform.position + randomAngle);
            }
            else
            {
                // while, if the distance between enermy and player is more than 8, it won't shoot the player. I don't know why. maybe the animation won't make the enemy aim that accurately.
                transform.rotation = Quaternion.LookRotation(player.transform.position - transform.position);
            }
            ShootDetection();
            gunShotTime = 0.5f;
        }
    }

    void AddEffects() { 
        player.GetComponent<Gun>().Being_shot(damage);
        print("Hit player");
    }

    public void BeingShoot(float damage) {
        if (!isDead)
        {
            if (health > damage && damage > 0)
            {
                health = health - damage;
            }
            else if (damage <= 0)
            {
                print("Set player damage error: should larger than 0");
            }
            else
            {
                isDead = true;
                health = 0;
            }
        }
        print("Hit enermy");
        print(health);
    }

    // make the gun independent after enemy dies
    void gunLeave()
    {
        if (!crash)
        {
            gun.GetComponent<Rigidbody>().constraints = ~RigidbodyConstraints.FreezeRotationZ;
            gun.GetComponent<Rigidbody>().useGravity = true;
            gun.GetComponent<BoxCollider>().isTrigger = true;
            onCollisionEnter(gun.GetComponent<BoxCollider>());
        }
        else
        {
            gun.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
            gun.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
            gun.GetComponent<Rigidbody>().useGravity = false;
            gun.GetComponent<BoxCollider>().isTrigger = false;
        }        
    }
    private void onCollisionEnter(Collider collider)
    {
        crash = true;
    }
}
