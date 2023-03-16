using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Orbit : MonoBehaviour
{
	GameObject planet;
    GameObject moon;
	List<GameObject> asteroids;
	
	float nextTime = 0.0f;

    [SerializeField]
	float planetMass = 5.972f;	
	[SerializeField]
	float planetSize = 12.742f;
	
	[SerializeField]
	float moonMass = 0.007347f;
	[SerializeField]
	float moonSize = 3.4748f;	
	
	[SerializeField]
	float distance = 191.25f;
	
	[SerializeField]
    float g = 10000f;	// Real value of gravitational constant is 6.67408 × 10-11
	
	[SerializeField]
	public float renderDistance = 400.0f;	// The max distance from the planet an asteroid can reach before they are removed from the simulation
	
	[SerializeField]
	bool explosions = true;
	
	public AsteroidStorm storm;

    // Start is called before the first frame update
    void Start(){
        moon = GameObject.Find("Moon");
		planet = GameObject.Find("Planet");
		
		// Set the initial values for size, mass, and position of moon from the serialize fields
		planet.GetComponent<Rigidbody>().mass = planetMass;
		Vector3 planetScale = new Vector3(planetSize, planetSize, planetSize);
		planet.transform.localScale = planetScale;
		
		moon.GetComponent<Rigidbody>().mass = moonMass;
		Vector3 moonScale = new Vector3(moonSize, moonSize, moonSize);
		moon.transform.localScale = moonScale;
		
		moon.transform.position = new Vector3(distance, 0, 0);

        SetInitialVelocity();
		
		asteroids = new List<GameObject>();
    }
	
	// Makes sure that a new initial velocity is set if any changes are done whilst the simulation is running
	// Makes it so that the moon will keep its orbit and that the properties of the objects are altered appropriately
	void OnValidate(){
		// Only update if there exists both a moon and planet object
		if(moon != null && planet != null){
			// Update the mass and size of the planet
			planet.GetComponent<Rigidbody>().mass = planetMass;
			Vector3 planetScale = new Vector3(planetSize, planetSize, planetSize);
			planet.transform.localScale = planetScale;
			
			// Update the mass and size of the moon
			moon.GetComponent<Rigidbody>().mass = moonMass;
			Vector3 moonScale = new Vector3(moonSize, moonSize, moonSize);
			moon.transform.localScale = moonScale;
			
			// Update the position of the moon to represent it being moved closer or further away from the planet
			// To get the new position the normalized vector of the positions of the planet and moon can be used to get
			// the direction the moon should be placed in, which multiplied with the new distance and added to the position
			// of the planet will give a position that is the set distance away from the planet in the same direction the moon
			// is currently in
			Vector3 direction = (moon.transform.position - planet.transform.position).normalized;
			moon.transform.position = planet.transform.position + direction * distance;
			
			SetInitialVelocity();
		}
	}

    // Update is called once per frame
    void FixedUpdate(){
		if(moon != null){Gravity(planet, moon);}
			
		// Itterate over all asteroids and add the gravitational pulls the planet and its moon has on each of them
		foreach(GameObject asteroid in asteroids.ToList()){
			Gravity(planet, asteroid);
			
			if(moon != null){Gravity(moon, asteroid);}
			
			// Make sure to destroy any asteroids that move outside of the max distance set to render the asteroids
			if(Vector3.Distance(planet.transform.position, asteroid.transform.position) > renderDistance){
				GameObject temp = asteroid;
				
				// Remove the asteroid from the list and delete it
				asteroids.Remove(temp);
				Destroy(temp);
				
			}
		}
		
		// If enough time has passed since the last asteroid was spawned and the max number of asteroids has not been reached
		if(Time.time > nextTime && asteroids.Count < storm.maxAsteroids){
			//Create a new asteroid and add it to the list of asteroids
			asteroids.Add(storm.createAsteroid());
			
			nextTime += storm.delay;
		}
    }
	
	// Destroy the specified planetary body
	// If it is an asteroid remove it from the list of asteroids first
	public void DestroyObject(GameObject body){
		// If its an asteroid remove it from the list of asteroids
		if(asteroids.Contains(body)){
			asteroids.Remove(body);
		}
		
		if(body.name != "Planet"){
				body.GetComponent<Rigidbody>().isKinematic = true;
				body.GetComponent<Renderer>().enabled = false;
				body.GetComponent<Collider>().enabled = false;
				
				if(explosions){
					// Add a particle system component to the planetary body to be destroyed
					// Used to simulate it "exploding" upon impact
					body.AddComponent<ParticleSystem>();
					ParticleSystem ps = body.GetComponent<ParticleSystem>();
					// Stop the particle system so that values can be properly set
					ps.Stop();
					// Store the different interfaces that are needed to set the different values
					var main = ps.main;
					var emission = ps.emission;
					var shape = ps.shape;
					
					main.duration = 0.1f;		// The time it will emit particles
					main.loop = false;			// Will not loop and will stop emiting once the time is up
					main.startLifetime = 3;		// The lifetime of the particles
					
					// Scale the particles so that they are a reasonable size compared the object that is destroyed
					main.startSize = body.transform.localScale.x / 25;	 
					emission.rateOverTime = 300;	// The amount of particles that will be emited per time unit
					
					// The shape of the emitter with a sphere allowing particles to spawn in every direction
					shape.shapeType = ParticleSystemShapeType.Sphere;
					
					// Make the particles emmmited be 3D spheres with the same material as the moon and asteroids
					ps.GetComponent<ParticleSystemRenderer>().renderMode = ParticleSystemRenderMode.Mesh;
					ps.GetComponent<ParticleSystemRenderer>().mesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
					ps.GetComponent<ParticleSystemRenderer>().material = Resources.Load("Materials/Moon", typeof(Material)) as Material;
					
					// Play the particle system
					ps.Play();
				}
			
			// Destroy the planetary body after some time, leaving the trail visible for longer
			Destroy(body, 20);
		}
	}

	// Sets the initial velocity of the moon such that it will orbit the planet
    void SetInitialVelocity(){
		if(moon != null){
			float m = planet.GetComponent<Rigidbody>().mass;
			float r = Vector3.Distance(moon.transform.position, planet.transform.position);

			// Rotates the tranform so that the z axis points in the direction of the planet the moon is to orbit around
			// Making it so that the x axis will always point in the direction of the orbit no matter where the planet is placed
			moon.transform.LookAt(planet.transform);

			// The formula for initial orbital velocty is as follows:
			//		√(g * m) / r
			
			//		g - Gravitational constant
			//		m - Mass of the planet the body is to orbit around
			//		r - Distance between the centre of mass of the two bodies
			
			// moon.transfrom.right makes the moon orbit counter clockwise by returning a vector in the direction of the x axis
			// Making the value negative results in a clockwise orbit
			moon.GetComponent<Rigidbody>().velocity = moon.transform.right * Mathf.Sqrt((g * m) / r);
		}
    }

	// Calculates the affect of the gravitational pull between two planetary bodies
	// Adding this calculated force to the Rigidbody component of the second planetary body, b
    void Gravity(GameObject a, GameObject b){
		float m1 = b.GetComponent<Rigidbody>().mass;
		float m2 = a.GetComponent<Rigidbody>().mass;
		float r = Vector3.Distance(b.transform.position, a.transform.position);

		// The formula for the gravitiational pull is as follows:
		//		g * (m1 * m2) / (r * r) 
		
		//		g - Gravitational constant
		//		m1,m2 - Masses of the two planetary bodies
		//		r - Distance between the centre of mass of the two bodies
		
		// The normalization of the vector between the centre points of the two planetary bodies makes it so that the force of the 
		// gravitational pull has the correct direction
		// Which would be going from the centre point of the planetary body that i affected by gravity towards the pne that pulls it
		b.GetComponent<Rigidbody>().AddForce((a.transform.position - b.transform.position).normalized * (g * (m1 * m2) / (r * r)));
    }
}