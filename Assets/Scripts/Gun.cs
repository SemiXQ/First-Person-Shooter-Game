using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class Gun : MonoBehaviour {

    public GameObject end, start; // The gun start and end point
    public GameObject gun;
    public Animator animator;
    
    public GameObject spine;
    public GameObject handMag;
    public GameObject gunMag;

    float gunShotTime = 0.1f;
    float gunReloadTime = 1.0f;
    Quaternion previousRotation;
    public float health = 100;
    public bool isDead;
    public bool isWin;
 

    public Text magBullets;
    public Text remainingBullets;
    public Text remainingHealth;

    int magBulletsVal = 30;
    int remainingBulletsVal = 90;
    int magSize = 30;
    public GameObject headMesh;
    public static bool leftHanded { get; private set; }

    public GameObject bulletHole;
    public GameObject muzzleFlash;
    public GameObject shotSound;

    public float damage = 30;
    public GameObject enermy;
    public GameObject enermy2;
    public GameObject enermy3;
    public GameObject door;
    public GameObject winPanel;
    public GameObject winText;
    public GameObject loseText;
    public GameObject[] AmmoCrates;
    public bool[] use;

    // Use this for initialization
    void Start() {
        headMesh.GetComponent<SkinnedMeshRenderer>().enabled = false; // Hiding player character head to avoid bugs :)
    }

    // Update is called once per frame
    void Update() {
        
        // Cool down times
        if (gunShotTime >= 0.0f)
        {
            gunShotTime -= Time.deltaTime;
        }
        if (gunReloadTime >= 0.0f)
        {
            gunReloadTime -= Time.deltaTime;
        }


        if ((Input.GetMouseButtonDown(0) || Input.GetMouseButton(0)) && gunShotTime <= 0 && gunReloadTime <= 0.0f && magBulletsVal > 0 && !isDead && !isWin)
        { 
            shotDetection(); // Should be completed

            addEffects(); // Should be completed

            animator.SetBool("fire", true);
            gunShotTime = 0.5f;
            
            // Instantiating the muzzle prefab and shot sound
            
            magBulletsVal = magBulletsVal - 1;
            if (magBulletsVal <= 0 && remainingBulletsVal > 0)
            {
                animator.SetBool("reloadAfterFire", true);
                gunReloadTime = 2.5f;
                Invoke("reloaded", 2.5f);
            }
        }
        else
        {
            animator.SetBool("fire", false);
        }

        if ((Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.R)) && gunReloadTime <= 0.0f && gunShotTime <= 0.1f && remainingBulletsVal > 0 && magBulletsVal < magSize && !isDead && !isWin)
        {
            animator.SetBool("reload", true);
            gunReloadTime = 2.5f;
            Invoke("reloaded", 2.0f);
        }
        else
        {
            animator.SetBool("reload", false);
        }
        if (isDead) {
            winPanel.GetComponent<CanvasGroup>().alpha = 1;
            loseText.GetComponent<CanvasGroup>().alpha = 1;
            animator.SetBool("dead", true);
            animator.GetComponent<CharacterMovement>().isDead = true;
            Invoke("reStart", 10f);
        }
        // restart when player win
        if (!isDead && isWin)
        {
            Invoke("reStart", 10f);
        }
        // decide when to trigger the collision to escape door
        if (!isWin && !isDead && Vector3.Distance(door.transform.position, transform.position)<2)
        {
            door.GetComponent<BoxCollider>().isTrigger = true;
            onCollisionEnter(door.GetComponent<BoxCollider>());
        }
        // decide when to refill Ammo and detect which ammo crate is used
        if(!isDead && !isWin && !(Input.GetMouseButtonDown(0) || Input.GetMouseButton(0)) && !(Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.R)))
        {
            if(Vector3.Distance(AmmoCrates[0].transform.position, transform.position) < 2 && !use[0])
            {
                refillAmmo(0);
            }
            else if (Vector3.Distance(AmmoCrates[1].transform.position, transform.position) < 2 && !use[1])
            {
                refillAmmo(1);
            }
        }
        updateText();
    }

  

    public void Being_shot(float damage) // getting hit from enemy
    {
        if (!isDead)
        {
            if (health > damage && damage > 0)
            {
                health = health - damage;
            }
            else if (damage <= 0)
            {
                print("Set enermy damage error: should larger than 0");
            }
            else {
                isDead = true;
                health = 0;
            }
        }
    }

    public void ReloadEvent(int eventNumber) // appearing and disappearing the handMag and gunMag
    {
        if (eventNumber == 1)
        {
            handMag.GetComponent<SkinnedMeshRenderer>().enabled = true;
            gunMag.GetComponent<SkinnedMeshRenderer>().enabled = false;
        }
        if (eventNumber == 2)
        {
            handMag.GetComponent<SkinnedMeshRenderer>().enabled = false;
            gunMag.GetComponent<SkinnedMeshRenderer>().enabled = true;
        }
    }

    void reloaded()
    {
        int newMagBulletsVal = Mathf.Min(remainingBulletsVal + magBulletsVal, magSize);
        int addedBullets = newMagBulletsVal - magBulletsVal;
        magBulletsVal = newMagBulletsVal;
        remainingBulletsVal = Mathf.Max(0, remainingBulletsVal - addedBullets);
        animator.SetBool("reloadAfterFire", false);
    }

    void updateText()
    {
        magBullets.text = magBulletsVal.ToString() ;
        remainingBullets.text = remainingBulletsVal.ToString();
        remainingHealth.text = health.ToString();
    }

    void shotDetection() // Detecting the object which player shot 
    {
        RaycastHit rayHit;
        int layerMask = 1 << 8;
        layerMask = ~ layerMask;
        if (Physics.Raycast(end.transform.position, (end.transform.position - start.transform.position).normalized, out rayHit, 100.0f, layerMask)) { 
            GameObject bulletHoleObject = Instantiate(bulletHole, rayHit.point + rayHit.collider.transform.up*0.01f, rayHit.collider.transform.rotation);
            if (rayHit.collider.tag == "enemy")
            {
                enermy.GetComponent<Enermy>().beingShoot = true;
                enermy.GetComponent<Enermy>().BeingShoot(damage);
            }
            else if (rayHit.collider.tag == "enermy2")
            {
                enermy2.GetComponent<Enermy>().beingShoot = true;
                enermy2.GetComponent<Enermy>().BeingShoot(damage);
            }
            else if (rayHit.collider.tag == "enermy3")
            {
                enermy3.GetComponent<Enermy>().beingShoot = true;
                enermy3.GetComponent<Enermy>().BeingShoot(damage);
            }
            Destroy(bulletHoleObject, 2.0f);
            print(rayHit.collider.tag);
        }

        GameObject muzzleFlashObject = Instantiate(muzzleFlash, end.transform.position, end.transform.rotation);
        muzzleFlashObject.GetComponent<ParticleSystem>().Play();
        Destroy(muzzleFlashObject, 2.0f);

        Destroy((GameObject)Instantiate(shotSound, transform.position, transform.rotation), 1.0f);
    }

    void addEffects() // Adding muzzle flash, shoot sound and bullet hole on the wall
    {
        // I add those in shotDetection part, as we learnt in tutorial
    }

    void reStart() {
        SceneManager.LoadScene(0);
    }

    private void onCollisionEnter(Collider collider)
    {
        isWin = true;
        winPanel.GetComponent<CanvasGroup>().alpha = 1;
        winText.GetComponent<CanvasGroup>().alpha = 1;
    }
    public void refillAmmo(int i)
    {
        if (remainingBulletsVal < 90)
        {
            use[i] = true;
            if(remainingBulletsVal+10 < 90)
            {
                remainingBulletsVal = remainingBulletsVal + 10;
            }
            else
            {
                remainingBulletsVal = 90;
            }
        }
    }
}
