using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class PlayerScript : MonoBehaviour {
	private Vector3 LastCrumb;
	private bool dead;
	public int levelNumber;

	public int collected;
	public float health;
	public Slider healthBar;
	public Text ToCollect;
	public float time;
	public Text timeDisplay;
	public Text GameOver;
	public Text Restart;

	// Use this for initialization
	void Start () {
		dead = false;
		GameOver.enabled = false;
		Restart.enabled = false;
		CreateCrumb();
		health = 1000;
		ToCollect.text = "5";
		healthBar.value = health;
		levelNumber = int.Parse(Regex.Match(Application.loadedLevelName, @"\d+").Value);
	}
	
	// Update is called once per frame
	void Update () {
		if( Vector3.Distance(gameObject.transform.position, LastCrumb)>4.0){
			CreateCrumb();
		}
		healthBar.value = health;
		if (time > (60*5)||health <= 0){
			GameOver.enabled = true;
			Restart.enabled = true;
			Time.timeScale = 0;
			dead = true;
		}
		else{
			timmer();
		}
		if(Input.GetKeyDown("return")&&dead)
		{
			Time.timeScale = 1;
			Application.LoadLevel(Application.loadedLevelName);
		}
	}

	void CreateCrumb(){
		GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		ball.transform.position = new Vector3(transform.position.x,3.5f,transform.position.z);
		MeshRenderer otherScript = ball.GetComponent<MeshRenderer>();
		otherScript.renderer.enabled = false;
		LastCrumb = ball.transform.position;
		ball.tag = "breadCrumb";
		Destroy(ball,3.0f);
	}
	void OnTriggerEnter(Collider other) {
		if(other.tag == "Trap" ){
			dmgPlayer(100, 0);
		} else if(other.tag == "Chest"){
			Animator anim = other.gameObject.GetComponent<Animator>();
			anim.SetBool("Open",true);
			collected++;
			ToCollect.text = ""+(int.Parse(Regex.Match(ToCollect.text, @"\d+").Value)-1);
			other.gameObject.GetComponent<BoxCollider>().enabled = false;
			if(collected ==5){
				Animator animExit = GameObject.Find("Grate").GetComponent<Animator>();
				animExit.SetBool("Open", true);
			}
		} else if(other.tag == "Exit"){
			Application.LoadLevel("Level_"+(levelNumber+1));
		}
	}	
	void OnControllerColliderHit(ControllerColliderHit hit) {
		if(hit.gameObject.tag == "TrapTrigger" ){
			hit.gameObject.GetComponent<TriggerTrap>().Trigger();
		}
	}
	public void dmgPlayer(int dmg, int force){
		Vector3 back = gameObject.transform.TransformDirection (Vector3.forward);
		Vector3 knockBack = gameObject.transform.position - back;
		transform.position = Vector3.MoveTowards(transform.position, knockBack, force*Time.deltaTime);
		health -= dmg;
	}
	void timmer(){
		time += Time.deltaTime;
		int timeMin = (int)(time/60);
		int timeSec = (int)(time%60);
		string timeMessage = "0"+ timeMin + ((timeSec<10) ? ":0":":") +timeSec;
		timeDisplay.text = timeMessage;
	}

}
