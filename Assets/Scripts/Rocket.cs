using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour {

	[SerializeField] float rcsThrust = 100f;
	[SerializeField] float mainThrust = 100f;
	[SerializeField] float levelLoadDelay = 2f;
	[SerializeField] AudioClip mainEngine;
	[SerializeField] AudioClip death;
	[SerializeField] AudioClip complete;
	[SerializeField] ParticleSystem mainEngineParticles;
	[SerializeField] ParticleSystem deathParticles;
	[SerializeField] ParticleSystem completeParticles;


	Rigidbody rigidBody;
	AudioSource audioSource;

    bool isTransitioning = false;
    bool collisionsAreEnabled = true;

	// Use this for initialization
	void Start () {
		rigidBody = GetComponent<Rigidbody>();
		audioSource = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
		if (!isTransitioning){
			RespondToThrustInput();
			RespondToRotateInput();
		}
        if (Debug.isDebugBuild)
        {
            RespondToDebugKeys();
        }
	}

    private void RespondToDebugKeys()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadNextLevel();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            collisionsAreEnabled = !collisionsAreEnabled; // toggle
        }
    }

    void OnCollisionEnter(Collision collision)
	{
		if (isTransitioning || !collisionsAreEnabled) {return;}
		switch (collision.gameObject.tag)
		{
			case "Friendly":
				// do nothing
				break;
            case "Finish":
                StartSuccessSequence();
                break;
            default:
                StartDeathSequence();
                break;
        }
	}

    private void StartDeathSequence()
    {
        isTransitioning = true;        
        audioSource.Stop();
        audioSource.PlayOneShot(death);
		deathParticles.Play();
        Invoke("LoadFirstLevel", levelLoadDelay);
    }

    private void StartSuccessSequence()
    {
        isTransitioning = true;
        float volume = .6f;
        audioSource.Stop();
        audioSource.PlayOneShot(complete, volume);
		completeParticles.Play();
        Invoke("LoadNextLevel", levelLoadDelay);
    }

    private void LoadFirstLevel()
    {
        SceneManager.LoadScene(0);
    }

    private void LoadNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;
    if (nextSceneIndex == SceneManager.sceneCountInBuildSettings)
    {
        nextSceneIndex = 0; // at the end, loop back to the start
    }
        SceneManager.LoadScene(nextSceneIndex);
    }

    private void RespondToRotateInput()
    {
		float rotationThisFrame = rcsThrust * Time.deltaTime;
        if (Input.GetKey(KeyCode.A))
        {
            RotateManually(rotationThisFrame);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            RotateManually(-rotationThisFrame);
        }
    }

    private void RotateManually(float rotationThisFrame)
    {
        rigidBody.freezeRotation = true; // manual control of rotation
        transform.Rotate(Vector3.forward * rotationThisFrame);
        rigidBody.freezeRotation = false; // end of manual rotation control
    }

    private void RespondToThrustInput()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            ApplyThrust();
        }
        else
        {
            StopApplyingThrust();
        }
    }

    private void StopApplyingThrust()
    {
        audioSource.Stop();
        mainEngineParticles.Stop();
    }

    private void ApplyThrust()
    {
        rigidBody.AddRelativeForce(Vector3.up * mainThrust * Time.deltaTime);
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(mainEngine);
        }
		mainEngineParticles.Play();
    }
}
