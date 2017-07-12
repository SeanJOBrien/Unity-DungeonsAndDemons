using UnityEngine;
using System.Collections;

public class TriggerTrap : MonoBehaviour {
	public int TrapTriggerCount;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	public void Trigger(){
		if (TrapTriggerCount > 0){
			Animation anim = gameObject.GetComponent<Animation>();
			anim.Play();
			TrapTriggerCount--;
		}
	}
}
