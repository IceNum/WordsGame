using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SoundController : MonoBehaviour {

	public AudioClip tap;
	public AudioClip apply;
	public AudioClip finish;
	public static SoundController data;
	private AudioSource soundSource;
	
	void Start () {
		data = this;
		soundSource = GetComponent<AudioSource>();
	}
	
	public void playTap(){
		soundSource.PlayOneShot(tap);
	}

    public void playApply()
    {
        soundSource.PlayOneShot(apply);
    }

    public void playFinish()
    {
        soundSource.PlayOneShot(finish);
    }


}
