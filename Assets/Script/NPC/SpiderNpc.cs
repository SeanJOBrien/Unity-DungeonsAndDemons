using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class SpiderNpc: MonoBehaviour {
	private Animator anim;
	private int Walking = Animator.StringToHash("Base Layer.WALKING");
	private int Idle = Animator.StringToHash("Base Layer.IDLE"); 
	private int Running = Animator.StringToHash("Base Layer.RUNNING"); 
	private int Attacking = Animator.StringToHash("Base Layer.ATTACKING");
	private GameObject spiderEye;
	private GameObject player;
	private float timer;

	public AnimatorStateInfo currentBaseState;
	public int WPCounter = 1;
	public float speed = 2.0f;
	public GameObject currentWayPoint;
	public int idNumber;

	void Start () {
		anim = gameObject.GetComponent<Animator>();
		anim.SetBool("Moving", true);
		currentBaseState = anim.GetCurrentAnimatorStateInfo(0);
		currentWayPoint = GameObject.Find("WP1_"+WPCounter);
		idNumber = int.Parse(Regex.Match(gameObject.name, @"\d+").Value);
		spiderEye = GameObject.Find("Spider_Eye_"+idNumber);
		player = GameObject.Find("Player");
		speed = 2.0f;
	}
	
	// Update is called once per frame
	void Update () {
		currentBaseState = anim.GetCurrentAnimatorStateInfo(0);
		if (currentBaseState.nameHash == Walking){
			animation.Play("Walk");
			waypointing();
		}else if (currentBaseState.nameHash == Idle){
			animation.Play("Death");
			Destroy(gameObject,1.5f);
		}else if (currentBaseState.nameHash == Running){
			animation.Play("Run");
			chasePlayer ();
		}else if (currentBaseState.nameHash == Attacking){
			attackPlayer();
			animation.Play("Attack");
		}
	}
	void OnTriggerEnter(Collider other) {
		if(other.tag == "Trap" ){
			anim.SetBool("Moving", false);
		}
	}	
	void OnCollisionEnter(Collision collision) {
		if(collision.gameObject.tag == "TrapTrigger" ){
			collision.gameObject.GetComponent<TriggerTrap>().Trigger();
		}
	}

	void chasePlayer (){
		RaycastHit hit;
		transform.LookAt(player.transform.position);
		spiderEye.transform.LookAt(player.transform.position); 
		Vector3 fwd = spiderEye.transform.TransformDirection (Vector3.forward);
		if (Physics.Raycast(spiderEye.transform.position, fwd, out hit, 100)) { 
			if(hit.collider.gameObject.name == "Player"){	 
				if (Vector3.Distance(transform.position, player.transform.position) < 8.0f){
					anim.SetBool("PlayerInRange", true);
				}else{
					transform.position = Vector3.MoveTowards(transform.position, new Vector3(player.transform.position.x,gameObject.transform.position.y,player.transform.position.z), speed*Time.deltaTime);
					transform.position = new Vector3 (gameObject.transform.position.x,0.0f,gameObject.transform.position.z);
				}
			} else{	
				CheckForObject();
				anim.SetBool("CanSeePlayer", false);
				speed = 2.0f;
			}
		}
	}
	void waypointing(){
		RaycastHit hit; 
		Vector3 perif = spiderEye.transform.position - player.transform.position;
		Vector3 fwd = spiderEye.transform.TransformDirection (Vector3.forward);
		if(Vector3.Angle(fwd, perif) > 90.0f){
			spiderEye.transform.LookAt(player.transform.position);
			fwd = spiderEye.transform.TransformDirection (Vector3.forward);
			if(Physics.Raycast(spiderEye.transform.position, fwd, out hit, 100)){
				if(hit.collider.gameObject.name == "Player"){
					anim.SetBool("CanSeePlayer", true);
					speed = 4.0f;
				}
			}
		}
		currentWayPoint = GameObject.Find("WP"+idNumber+"_"+WPCounter);
		transform.LookAt(currentWayPoint.transform.position);
		transform.position = Vector3.MoveTowards(transform.position, new Vector3(currentWayPoint.transform.position.x,gameObject.transform.position.y,currentWayPoint.transform.position.z), speed*Time.deltaTime);
		transform.position = new Vector3 (gameObject.transform.position.x,0.0f,gameObject.transform.position.z);
		if(Vector3.Distance(transform.position, currentWayPoint.transform.position) < 0.5)
		{
			if(WPCounter == 4){
				WPCounter = 1;
			}else{
				WPCounter++;
			}
		}
	}
	void attackPlayer(){
		transform.position = new Vector3 (gameObject.transform.position.x,0.0f,gameObject.transform.position.z);
		if( timer < 1.5f){
			timer += Time.deltaTime;
		}
		else{
			if (Vector3.Distance(transform.position, player.transform.position) > 8.0f){
				anim.SetBool("PlayerInRange", false);
				timer = 0;
			}else{
				player.GetComponent<PlayerScript>().dmgPlayer(10, 500);
			}
		}
	}
	void CheckForObject(){
		float shortest = 1000000.0f;
		int i = 1;
		while (i <= 4){
			currentWayPoint = GameObject.Find("WP1_"+i);
			transform.LookAt(currentWayPoint.transform.position);
			var fwd = transform.TransformDirection (Vector3.forward);
			if (!Physics.Raycast(transform.position, fwd, 50)) {
				if(Vector3.Distance(transform.position, currentWayPoint.transform.position) < shortest){
					shortest = Vector3.Distance(transform.position, currentWayPoint.transform.position);
					WPCounter = i;
				}
			}
			i++;
		}
	}
}