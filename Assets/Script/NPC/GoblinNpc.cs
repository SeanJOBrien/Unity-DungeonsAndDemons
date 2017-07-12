using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GoblinNpc : MonoBehaviour {
	private Animator anim;
	private int Walking; 
	private int Idle;
	private int Dead; 
	private int Attacking;
	private List<Vector3> NpcBreadcrumbs;
	private GameObject player;
	private GameObject Goblin_Eye;
	private float timer;
	
	public AnimatorStateInfo currentBaseState;
	public int idNumber;
	public float speed;

	// Use this for initialization
	void Start () {
		anim = gameObject.GetComponent<Animator>();
		Walking = Animator.StringToHash("Base Layer.Moving");
		Idle = Animator.StringToHash("Base Layer.Idle"); 
		Dead = Animator.StringToHash("Base Layer.Dead");
		Attacking = Animator.StringToHash("Base Layer.Attacking");
		anim.SetBool("Alive", true);
		anim.SetBool("CanSeePlayer", false);
		anim.SetBool("PlayerInRange", false);
		currentBaseState = anim.GetCurrentAnimatorStateInfo(0);
		idNumber = int.Parse(Regex.Match(gameObject.name, @"\d+").Value);
		Goblin_Eye = GameObject.Find("Goblin_Eye_"+idNumber);
		speed = 4.0f;
		NpcBreadcrumbs = new List<Vector3>();
		player = GameObject.Find("Player");
	}
	
	// Update is called once per frame
	void Update () {
		currentBaseState = anim.GetCurrentAnimatorStateInfo(0);;
		if (currentBaseState.nameHash == Walking){
			chasePlayer ();
		}else if (currentBaseState.nameHash == Idle){
			idleState();
		}else if (currentBaseState.nameHash == Dead){
			Destroy(gameObject, 1.5f);
		}else if (currentBaseState.nameHash == Attacking){
			attackPlayer();
		}
	}

	void OnTriggerEnter(Collider other) {
		if(other.tag == "Trap" ){
			anim.SetBool("Alive", false);
		}
	}	
	void OnCollisionEnter(Collision collision) {
		if(collision.gameObject.tag == "TrapTrigger" ){
			collision.gameObject.GetComponent<TriggerTrap>().Trigger();
		}
	}

	void idleState(){
		RaycastHit hit;
		Vector3 perif = Goblin_Eye.transform.position - player.transform.position;
		Vector3 fwd = Goblin_Eye.transform.TransformDirection (Vector3.forward);
		if(Vector3.Angle(fwd, perif) > 90.0f){
			Goblin_Eye.transform.LookAt(player.transform.position);
			fwd = Goblin_Eye.transform.TransformDirection (Vector3.forward);
			if(Physics.Raycast(Goblin_Eye.transform.position, fwd, out hit, 100)){
				if(hit.collider.gameObject.name == "Player"){
					anim.SetBool("CanSeePlayer", true);
					anim.SetBool("Alive", true);
				}
			}
		}
	}
	void attackPlayer(){
		transform.position = new Vector3 (gameObject.transform.position.x,0.0f,gameObject.transform.position.z);
		if( timer < 0.5f){
			timer += Time.deltaTime;
		}
		else{
			if (Vector3.Distance(transform.position, player.transform.position) > 8.0f){
				anim.SetBool("PlayerInRange", false);
				timer = 0;
			}else{
				player.GetComponent<PlayerScript>().dmgPlayer(10, 200);
			}
		}
	}
	void chasePlayer(){
		transform.LookAt(player.transform.position);
		Goblin_Eye.transform.LookAt(player.transform.position);
		RaycastHit hit; 
		Vector3 fwd = Goblin_Eye.transform.TransformDirection (Vector3.forward);
		if (Physics.Raycast(Goblin_Eye.transform.position, fwd, out hit, 100)) { 
			if(hit.collider.gameObject.name == "Player"){	 
				if (NpcBreadcrumbs.Count > 0){	
					if( Vector3.Distance(transform.position, NpcBreadcrumbs[NpcBreadcrumbs.Count-1])>1.0f){
						NpcBreadcrumbs.Add(gameObject.transform.position);
					}
				}else{
					NpcBreadcrumbs.Add(transform.position);
				}	
				transform.position = Vector3.MoveTowards(transform.position, new Vector3(player.transform.position.x,gameObject.transform.position.y,player.transform.position.z), speed*Time.deltaTime);
				transform.position = new Vector3 (gameObject.transform.position.x,0.0f,gameObject.transform.position.z);
				if (Vector3.Distance(transform.position, player.transform.position) < 3.0f){
					anim.SetBool("PlayerInRange", true);
				}
					
			} else if (NpcBreadcrumbs.Count > 0){	
				transform.LookAt(NpcBreadcrumbs[NpcBreadcrumbs.Count-1]);
				transform.position = Vector3.MoveTowards(transform.position, NpcBreadcrumbs[NpcBreadcrumbs.Count-1], speed*Time.deltaTime);
				transform.position = new Vector3 (gameObject.transform.position.x,0.0f,gameObject.transform.position.z);
				if(Vector3.Distance(gameObject.transform.position, NpcBreadcrumbs[NpcBreadcrumbs.Count-1])<0.1f){
					NpcBreadcrumbs.RemoveAt(NpcBreadcrumbs.Count-1);
				}
			} else {
				anim.SetBool("CanSeePlayer", false);
			}
		}
	}
}
