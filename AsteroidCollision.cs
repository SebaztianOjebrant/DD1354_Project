using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidCollision : MonoBehaviour
{
	
	public Orbit orbit;
	
    // Start is called before the first frame update
    void Start()
    {
		// Make sure that every object that has this script has the proper reference for the Orbt script
        orbit = GameObject.Find("Orbiting System").GetComponent<Orbit>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	// Makes sure that to proper forces are applied to objects that collide
	void OnCollisionEnter(Collision collision){
		float m1 = GetComponent<Rigidbody>().mass;
		float m2 = collision.rigidbody.mass;
		Vector3 u1 = GetComponent<Rigidbody>().velocity;
		Vector3 u2 = collision.rigidbody.velocity;
		
		//The formula for calculating the new velocities after collision is as follows:
			//v1 = ((m1 – m2) / (m1 + m2)) * u1 + ((2 * m2)/ (m1 + m2)) * u2
			//v2 = ((2 * m1) / (m1 + m2)) * u1 + ((m2 - m1)/ (m1 + m2)) * u2
			
			//v1,v2 - Velocities after collision
			//u1,u2 - Velocities before collision
			//m1,m2 - Masses of the two objects
			
			// 1 applying to this object and 2 applying to the object it collides with
		Vector3 v1 = ((m1 - m2) / (m1 + m2)) * u1 + ((2 * m2) / (m1 + m2)) * u2;
		Vector3 v2 = ((2 * m1) / (m1 + m2)) * u1 + ((m2 - m1)/ (m1 + m2)) * u2;
		
		// Check if either object has a lesser mass than the other, if so destroy the one with less mass upon impact
		// Or both if they have an equal mass
		// The surviving object should have an updated velocity based upon elastic collision
		if(m1 <= m2){
			orbit.DestroyObject(gameObject);
			GetComponent<Rigidbody>().velocity = v1;
		}
	}
}
