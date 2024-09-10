using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructObject : MonoBehaviour
{   
    public Transform building;
    public GameObject buildingPrefab;
    public float bulletSpeed = 10;
    Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        anim = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            var bullet = Instantiate(buildingPrefab,  building.position, new Quaternion(0,90,0,0));
            bullet.GetComponent<Rigidbody>().velocity = building.forward * bulletSpeed;
            anim.SetTrigger("isconstructing");
        }
        else if(Input.GetKeyDown(KeyCode.V))
        {
            anim.SetTrigger("iswalking");
        }
    }
}
