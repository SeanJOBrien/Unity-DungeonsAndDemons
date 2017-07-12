using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class DemonNpc : MonoBehaviour {

	private Animator anim;
	private int Walking; 
	private int Idle;
	private int Dead; 
	private int Attacking;
	private List<Vector3> NpcBreadcrumbs;
	private float timer;
	private GameObject Demon_Eye;
	private GameObject player;
	private bool ignorePlayer;

	public AnimatorStateInfo currentBaseState;
	public int idNumber;
	public float speed;

	// Use this for initialization
	void Start () {
		ignorePlayer = false;
		anim = gameObject.GetComponent<Animator>();
		Walking = Animator.StringToHash("Base Layer.Moving");
		Idle = Animator.StringToHash("Base Layer.Idle"); 
		Dead = Animator.StringToHash("Base Layer.Dead");
		Attacking = Animator.StringToHash("Base Layer.Attacking");
		anim.SetBool("Alive", true);
		anim.SetBool("Tracking", false);
		anim.SetBool("InRange", false);
		currentBaseState = anim.GetCurrentAnimatorStateInfo(0);
		idNumber = int.Parse(Regex.Match(gameObject.name, @"\d+").Value);
		Demon_Eye = GameObject.Find("Demon_Eye_"+idNumber);
		speed = 4.0f;
		NpcBreadcrumbs = new List<Vector3>();
		player = GameObject.Find("Player");
		timer = 0;
	}
	
	// Update is called once per frame
	void Update () {
		currentBaseState = anim.GetCurrentAnimatorStateInfo(0);;
		if (currentBaseState.nameHash == Walking){
			chasePlayer ();
			animation.Play("Run");
		}else if (currentBaseState.nameHash == Idle){
			idleState();
			animation.Play("Idle");
		}else if (currentBaseState.nameHash == Dead){
			animation.Play("Death");
			Destroy(gameObject, 1.5f);
		}else if (currentBaseState.nameHash == Attacking){
			attackPlayer();
			animation.Play("Attack");
		}
	}
	void OnTriggerEnter(Collider other) {
		if(other.tag == "Trap" )
		{
			//anim.SetBool("Alive", false);
		}
	}	
	void OnCollisionEnter(Collision collision) {
		if(collision.gameObject.tag == "TrapTrigger" )
		{
			ignorePlayer = true;
			anim.SetBool("Tracking", false);
		}
	}
	void idleState(){
		RaycastHit hit;
		Vector3 perif = Demon_Eye.transform.position - player.transform.position;
		Vector3 fwd = Demon_Eye.transform.TransformDirection (Vector3.forward);
		if(Vector3.Angle(fwd, perif) > 90.0f){
			Demon_Eye.transform.LookAt(player.transform.position);
			fwd = Demon_Eye.transform.TransformDirection (Vector3.forward);
			if(Physics.Raycast(Demon_Eye.transform.position, fwd, out hit, 100))
			{
				if(hit.collider.gameObject.name == "Player")
				{
					animation.Play("Roar");
					anim.SetBool("Tracking", true);
					anim.SetBool("Alive", true);
				}
			}
		}
	}
	
	void chasePlayer(){
		transform.LookAt(player.transform.position);
		Demon_Eye.transform.LookAt(player.transform.position);
		RaycastHit hit; 
		Vector3 fwd = Demon_Eye.transform.TransformDirection (Vector3.forward);
		if (Physics.Raycast(Demon_Eye.transform.position, fwd, out hit, 100)) { 
			if(hit.collider.gameObject.name == "Player" && !ignorePlayer){	 
				if (NpcBreadcrumbs.Count > 0){	
					if( Vector3.Distance(transform.position, NpcBreadcrumbs[NpcBreadcrumbs.Count-1])>1.0f){
						NpcBreadcrumbs.Add(gameObject.transform.position);
					}
				}else{
					NpcBreadcrumbs.Add(transform.position);
				}	
				if (Vector3.Distance(transform.position, player.transform.position) < 3.0f){
					anim.SetBool("InRange", true);
				}else{
					transform.position = Vector3.MoveTowards(transform.position, new Vector3(player.transform.position.x,0.0f,player.transform.position.z), speed*Time.deltaTime);
					transform.position = new Vector3 (gameObject.transform.position.x,0.0f,gameObject.transform.position.z);
				}
			} else {	
				GameObject[] crumbs = GameObject.FindGameObjectsWithTag("breadCrumb");
				float shortest = 99999999;
				if (crumbs.Length > 0){
					GameObject closestCrumb = crumbs[0];
					for (int i = 0; i < crumbs.Length; i++){
						if(Vector3.Distance(transform.position, crumbs[i].transform.position)< shortest)
						{
							shortest = Vector3.Distance(transform.position, crumbs[i].transform.position);
							closestCrumb = crumbs[i];
						}
					}
					Demon_Eye.transform.LookAt(closestCrumb.transform.position);
					fwd = Demon_Eye.transform.TransformDirection (Vector3.forward);
					if (Physics.Raycast(Demon_Eye.transform.position, fwd, out hit, 100)) { 
						if(hit.collider.gameObject.tag == "breadCrumb"){
							if( Vector3.Distance(transform.position, NpcBreadcrumbs[NpcBreadcrumbs.Count-1])>1.0f){
								NpcBreadcrumbs.Add(gameObject.transform.position);
							}
							transform.position = Vector3.MoveTowards(transform.position, new Vector3(closestCrumb.transform.position.x,gameObject.transform.position.y,closestCrumb.transform.position.z), speed*Time.deltaTime);
							transform.position = new Vector3 (gameObject.transform.position.x,0.0f,gameObject.transform.position.z);
							if( Vector3.Distance(transform.position, closestCrumb.transform.position)<4.0f){
								Destroy(closestCrumb);
							}
						}else if(NpcBreadcrumbs.Count > 0){
							returnToStart();
						} else {
							anim.SetBool("Tracking", false);
						}
					}else if (NpcBreadcrumbs.Count > 0){
						returnToStart();
					} else {
						anim.SetBool("Tracking", false);
					}
				} else if (NpcBreadcrumbs.Count > 0){
					returnToStart();
				} else {
					anim.SetBool("Tracking", false);
				}
			} 
		}
	}
	void attackPlayer()
	{
		transform.position = new Vector3 (gameObject.transform.position.x,0.0f,gameObject.transform.position.z);
		if( timer < 0.5f)
		{
			timer += Time.deltaTime;
		}
		else{
			if (Vector3.Distance(transform.position, player.transform.position) > 8.0f){
				anim.SetBool("InRange", false);
				timer = 0;
			}else{
				player.GetComponent<PlayerScript>().dmgPlayer(10, 200);

			}
		}
	}
	void returnToStart()
	{
		transform.LookAt(NpcBreadcrumbs[NpcBreadcrumbs.Count-1]);
		transform.position = Vector3.MoveTowards(transform.position, NpcBreadcrumbs[NpcBreadcrumbs.Count-1], speed*Time.deltaTime);
		transform.position = new Vector3 (gameObject.transform.position.x,0.0f,gameObject.transform.position.z);
		if(Vector3.Distance(gameObject.transform.position, NpcBreadcrumbs[NpcBreadcrumbs.Count-1])<0.1f){
			NpcBreadcrumbs.RemoveAt(NpcBreadcrumbs.Count-1);
		}
	}
}

