using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidStorm : MonoBehaviour
{
	GameObject planet;
	GameObject moon;
	
	[SerializeField]
	float size = 1.0f;
	[SerializeField]
	float mass = 1.0f;
	[SerializeField]
	Vector3 position = new Vector3(0,0,100);
	[SerializeField]
	Vector3 direction = new Vector3(-40,0,-40);
	
	[SerializeField]
	public float delay = 15.0f;		// the number of seconds to wait before creating a new asteroid
	[SerializeField]
	public int maxAsteroids = 40;	// THe max number of asteroids that may exist at one time
	
	[SerializeField]
	bool randomize = true;	// If true the size, mass and direction of the asteroid will be randomized within the set values
	
	[SerializeField]
	float minSize = 0.000001f;
	[SerializeField]
	float maxSize = 1.0f;
	[SerializeField]
	float minVelocity = 1.0f;	// Should be kept greater than 1 to keep a direction towards the orbiting system
	[SerializeField]
	float maxVelocity = 50.0f;
	
	[SerializeField]
	float sizeMass = 0.01759f;
	[SerializeField]
	int maxDegreeVariation = 45;
	
	Vector3 lastMoonPosition;
	
    // Start is called before the first frame update
    void Start(){
        planet = GameObject.Find("Planet");
		moon = GameObject.Find("Moon");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	// Creates a new asteroid gameobject
	public GameObject createAsteroid(){
		GameObject asteroid = new GameObject("Asteroid", typeof(Rigidbody), typeof(SphereCollider), typeof(TrailRenderer), typeof(AsteroidCollision));
		
		// Make it so that the gravity built into the Rigidbody component is not used
		asteroid.GetComponent<Rigidbody>().useGravity = false;

        // Create the gradient of red to transparent for the asteroid trail
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(new Color(1.0f, 0.2987421f, 0.3260961f), 0.0f), new GradientColorKey(new Color(1.0f, 0.2987421f, 0.3260961f), 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
        );
		
		// Set the new gradient for the trail and set the material to the default particle system one
		asteroid.GetComponent<TrailRenderer>().colorGradient = gradient;
		asteroid.GetComponent<TrailRenderer>().material = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Material>("Default-Particle.mat");
		
		// Set the time the trail remains visible to the same as the trail for the moon
		asteroid.GetComponent<TrailRenderer>().time = 20;
		
		// If randomize is set the values for the asteroid created should be randomized within the set ranges
		if(randomize){
			// Set a random value for the size of the asteroid that is between the set boundaries
			float randSize = Random.Range(minSize, maxSize);
			asteroid.transform.localScale = new Vector3(randSize, randSize, randSize);
			
			// Set the possible spawn area for the asteroid which should be as follows:
			//		outside of the sphere created by the moon's orbit
			//		inside an outer spherican bound
			float innerRadius;
			float outerRadius;
			if(moon != null){
				innerRadius = Vector3.Distance(planet.transform.position, moon.transform.position) + 50;
				outerRadius = innerRadius + 200;
				
				// Store the most recent position of the moon so that the distance between it and the planet can be used
				// Even when the moon no longer exists
				lastMoonPosition = moon.transform.position;
			}
			else {
				innerRadius = Vector3.Distance(planet.transform.position, lastMoonPosition) + 50;
				outerRadius = innerRadius + 200;
			}
			
			// Generate the random position within the bounds set above
			Vector3 spawn = Random.insideUnitSphere.normalized * Random.Range(innerRadius, outerRadius);
			asteroid.transform.position = spawn;
			
			// Multiple the random size value with the sizeMass which is set to be the mass for a sphere of a size 1
			asteroid.GetComponent<Rigidbody>().mass = randSize * sizeMass;
			
			// Temporarily store the normalized vector direction from the asteroid to the planet
			Vector3 tempDirection = (planet.transform.position - spawn).normalized;
			
			//Rotate the x axis
			Vector3 xVector = Quaternion.AngleAxis(Random.Range(-maxDegreeVariation, maxDegreeVariation), Vector3.right) * tempDirection;
			//Rotate the y axis
			Vector3 xyVector = Quaternion.AngleAxis(Random.Range(-maxDegreeVariation, maxDegreeVariation), Vector3.up) * xVector;
			//Rotate the z axis
			Vector3 xyzVector = Quaternion.AngleAxis(Random.Range(-maxDegreeVariation, maxDegreeVariation), Vector3.forward) * xyVector;
			
			// Set the direction of the asteroid to be within a set degree of the direction going straight towards the planet
			// Makes sure that all asteroids will be aimed towards the visible plane of the orbiting system
			asteroid.GetComponent<Rigidbody>().velocity = xyzVector * Random.Range(minVelocity, maxVelocity);	
		}
		// If not randomized the set values should be set instead for every asteroid created
		else {
			// Set the size and position of the asteroid
			asteroid.transform.localScale = new Vector3(size, size, size);
			asteroid.transform.position = position;
			
			// Set the mass and velocity of the asteroid
			asteroid.GetComponent<Rigidbody>().mass = mass;
			asteroid.GetComponent<Rigidbody>().velocity = direction;
		}

		return asteroid;
		
	}
}
