using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//Glowna klasa Sztucznej inteligencji
public class AIZombie : MonoBehaviour {

    //Pole przechowujące referencje do Animatora Zombie
    private Animator zombieAnim;
    //Pole przechowujące referencje do komponentu nawigacji Zombie
    private NavMeshAgent zombieAgent;

    //Pole przechowujące srodek postaci gracza
    public Transform transPlayer;
    //Minimalna odleglosc, po przekroczeniu ktorej przeciwnik nas
    //zauwazy bedac odwroconym tylem
    public float minDistanceSee = 2.0f;

    public int minDamage = 20;
    public int maxDamage = 40;

    //zmienna pomocnicza przechowujaca aktualny stan zdrowia
    public int currentHealth;
    //Czas do zakonczenia animacji ataku
    public float afterAttackTime = 2.3f;
    //Czas po ktorej przeciwnik moze zadac obrazenia postaci
    public float damageTime = 0.9f;

    public float waypointViewAngle = 20.0f;

    public float chaseViewAngle = 60.0f;

    public float fieldOfViewRange = 0f;
    //aktualny kat pomiedzy wektorem przednim, a promieniem wodzacym za graczem
    public float currentAngle = 0.0f;
    //Warstwy z ktorymi promien wodzacym za graczem ma nie kolidowac 
    private int layer = 0;

    //Referencja do dodatkowego kolidera przeciwnika
    public CapsuleCollider zombieCollider;
    //referencja do komponentu odpowiedzialnego za dzwieki wydawane przez gracza
    private SoundHuman brianSound;
    //Komponent zrodla dzwieku przeciwnika
    private AudioSource zombieAudio;
    //Odglos zadawania obrazen przez przeciwnika
    public AudioClip zombieAttackClip;
    //pusty obiekt od ktorego zaczyna sie promien wodzacy za graczem
    public GameObject rayObj;
    //referencja do postaci gracza
    private GameObject Player;
    //zdrowie przeciwnika
    private Health zombieHealth;
    //zdrowie postaci gracza
    private Health playerHealth;

    public bool isImmortal = false;
    //pole odpowiedzialne za aktualny stan przeciwnika
    public bool playerIsNear = false;
    public bool doorDestroyState = false;
    public bool seePlayer = false;
    private bool stayWaypoint = false;

    private GameObject posPlayerMemory;
    //zmianna odmierzajaca czas
    private float counter = 0.0f;
    private float counterDoor = 0.0f;
    //czas po ktorym przeciwnik zmienia aktualny cel do ktorego
    //dazy w stanie bezczynnosci
    public float timeStay = 5.0f;
    private float timeDoorDestroy = 1.7f;
    //maksymalna odleglosc od gracza, po ktorej przeciwnik
    //przestaje gonic gracza
    public float maxDisZombie = 10.0f;
    public float acceleration = 40.0f;
    //tablica przechowujaca punkty na drodze po miedzy ktorymi
    //przeciwnik moze sie poruszac w stanie domyslnym
    public Transform[] waypointZombie;
    //obecny punkt do ktorego dazy zombie w stanie domyslnym
    public int currentWaypoint = 0;
    //minimalny dystans po miedzy przeciwnikiem a graczem, gdy
    //przeciwnik goni gracza
    public float stoppingDis = 1.6f;
    //czas pomiedzy atakami
    public float timeBetweenAttack = 1.0f;
    //czas od poczatku gry do ataku
    private float breakInAttack;
    public float stopWaypointDis = 0.6f;
    //public float stopDis3 = 0.7f;
    public Rigidbody rb_door;
    public bool canDestroyDoor = true;

    public float dis_door = 0.0f;

    public float chaseSpeed = 2.7f;
    public float offsetDisStop = 0.3f;
    public bool pathInvalid = false;
    public Transform waypointHelp;
    private bool isEnd = false;
    public HelpWaipontSystem help;
    

    // Inicjalizacja zmiennych
    void Start () {
        
        layer = ~(1 << 12 | 1 << 13 | 1 << 10 | 1 << 9 | 1 << 15);
        breakInAttack = Time.time;
        zombieAgent = GetComponent<NavMeshAgent>();
        zombieAgent.acceleration = acceleration;

        zombieAnim = GetComponent<Animator>();
        zombieAgent.speed = 0.5f;
        Player = GameObject.FindGameObjectWithTag("Player");
        transPlayer = Player.transform.Find("TransPlayer").GetComponent<Transform>();
        zombieHealth = GetComponent<Health>();
        currentHealth = zombieHealth.currentHealth;
        playerHealth = Player.GetComponent<Health>();
        brianSound = Player.GetComponent<SoundHuman>();
        zombieCollider = GetComponent<CapsuleCollider>();
        zombieAudio = GetComponent<AudioSource>();

        posPlayerMemory = new GameObject("PPMemory" + gameObject.name);
        fieldOfViewRange = waypointViewAngle;
        help = GetComponent<HelpWaipontSystem>();
	}
	
    public void SetChase()
    {
        posPlayerMemory.transform.position = Player.transform.position;
        playerIsNear = true;
    }

    public void SetisEnd()
    {
        isEnd = true;
    }

	// Funkcja wywolywana co klatke gry
	void Update () {
        
        //Wykonywane, gdy przeciwnik zyje 
        if (!zombieHealth.isDead() && !isEnd)
        {
            //Jezeli postac gracza jest daleko stan przeciwnika jest domysny
            //(przeciwni przemieszcza sie pomiedzy punktami drogi)
            if (!doorDestroyState)
            {
                canDestroyDoor = true;
                if (!playerIsNear)
                {
                    WaypointState();
                }
                //Jezeli postac jest blisko, przeciwnik zaczyna ja gonic
                else
                {
                    
                    ChasePlayer();
                }
            }
            else
            {
                
                if(counterDoor >= timeDoorDestroy)
                {
                    doorDestroyState = false;
                    if (zombieAgent.enabled) zombieAgent.isStopped = false;
                    counterDoor = 0f;
                }
                else
                {
                    if(counterDoor >= 0.1f && canDestroyDoor)
                    {

                        rb_door.AddForce(rayObj.transform.forward * 110f *(dis_door/0.6f));
                        
                        canDestroyDoor = false;
                    }
                    //RotationZombie(rb_door.transform.parent.gameObject);
                    counterDoor += Time.deltaTime;
                }
            }
            //Wektor biegnacy od przeciwnika do gracza
            Vector3 disPlayerv = transPlayer.transform.position - rayObj.transform.position;
            //Odleglosc od przeciwnika do gracza
            float disPlayerf = Vector3.Distance(rayObj.transform.position, transPlayer.position);
            //inicjalizacja promien wodzacegop za grczem
            Ray zombieRay = new Ray(rayObj.transform.position, disPlayerv);
            //aktualny kat pomiedzy wektorem przednim, a promieniem wodzacym za graczem
            currentAngle = Vector3.Angle(disPlayerv, rayObj.transform.forward);

            //zmienna przechowujaca obiekt, ktory zostal trafiony promien 
            RaycastHit targetP;
            //Wywoluje sie tylko wtedy gdy promien aktywny
            //Debug.DrawRay(rayObj.transform.position, disPlayerv * 20f, new Color(0.0f, 0.5f, 1f, 1f));
            //Wywolane jezeli promien na cos natrafil
            if (!playerIsNear || (playerIsNear && !doorDestroyState))
            {
                if (Physics.Raycast(zombieRay, out targetP, maxDisZombie, layer) && !playerHealth.isDead())
                {
                    //Jezeli promien natrafil na gracza oraz gracz jest w jego zasiegu widzenia lub jest
                    //bardzo blisko przeciwnika, zombie zaczyna gonic gracza

                    if (targetP.collider.tag == "Player")
                    {

                        if (currentAngle <= fieldOfViewRange || disPlayerf <= minDistanceSee)
                        {
                            /*                            if(doorDestroyState)
                                                        {
                                                            zombieAgent.isStopped = false;
                                                            doorDestroyState = false;
                                                            counterDoor = 0f;
                                                        }
                                                        */
                            playerIsNear = true;
                            seePlayer = true;
                            pathInvalid = false;
                        }
                    }
                    else
                    {
                        seePlayer = false;
                    }
                }
            }

            //Promien, ktory sprawdza czy przeciwnik nie napotkal na swojej drodze drzwi pojedyncze 
            Ray detectionDoor = new Ray(rayObj.transform.position, rayObj.transform.forward);
            Debug.DrawRay(rayObj.transform.position, rayObj.transform.forward * 0.7f, new Color(0.5f, 0.5f, 1f, 1f));
            //zmienna przechowujaca obiekt, ktory zostal trafiony promien detekcji drzwi pojedynczych
            RaycastHit door;
            //Wywolane jezeli promien na cos natrafil
            if (Physics.Raycast(detectionDoor, out door, 0.5f))
            {
                
                //Wykonaj, kiedy napotkasz pojedyncze drzwi
                if (door.collider.tag == "door" || door.collider.gameObject.tag == "doorNH")
                {
                    
                    Door _door = door.collider.GetComponent<Door>();

                    //Jezeli drzwi nie sa otwarte i gracz jest blisko wykonaj 
                    if (!(_door.isOpen) && !(_door.isLock))
                    {
                        if (!seePlayer || playerHealth.isDead() || door.collider.tag == "doorNH") //if (playerIsNear)
                        {
                            if (_door.isFullClose && !_door.toiletDoor)
                            {
                                dis_door = door.distance;
                                /*
                                //Wybieranie najblizszy punkt drogi
                                SelectWaypoint();


                                //wylaczenie biegania
                                zombieAnim.SetBool("isRun", false);
                                //zmiana stanu na domyslny
                                playerIsNear = false;
                                */

                                MeshCollider _mc = _door.transform.GetComponent<MeshCollider>();
                                if (_mc)
                                {
                                    Destroy(_mc);
                                }
                                _door.transform.tag = "doorNoActive";
                                _door.transform.gameObject.layer = 16;
                                Destroy(_door);

                                Destroy(_door.transform.GetComponent<NavMeshObstacle>());
                                _door.transform.GetComponent<BoxCollider>().enabled = true;
                                rb_door = _door.transform.GetComponent<Rigidbody>();
                                rb_door.useGravity = true;
                                rb_door.isKinematic = false;


                                zombieAnim.SetBool("isWalk", false);
                                zombieAnim.SetBool("isRun", false);
                                if (zombieAgent.enabled) zombieAgent.isStopped = true;
                                doorDestroyState = true;
                                zombieAnim.SetTrigger("Attack");
                            }
                            else if(!_door.isFullClose || _door.toiletDoor)
                            {
                                _door.ChangeState();
                            }
                        }

                    }

                }
                if(door.collider.tag == "LabDoor")
                {
                    DoorLab doorLab = door.collider.GetComponent<DoorLab>();
                    if(!doorLab.isOpen && !doorLab.isLock)
                    {
                        doorLab.ChangeState();
                    }
                }
            }
            if (currentHealth < zombieHealth.currentHealth)
            {
                currentHealth = zombieHealth.currentHealth;
            }

            if (!playerIsNear || (playerIsNear && !doorDestroyState))
            {
                if (currentHealth > zombieHealth.currentHealth)
                {
                    if (!isImmortal) currentHealth = zombieHealth.currentHealth;
                    else zombieHealth.SetFullHealth();
                    playerIsNear = true;


                    posPlayerMemory.transform.position = Player.transform.position;
                }

            }
        }
        //Wykonywane, gdy przeciwnik zostal pokonany
        else
        {
            //zombieAgent.isStopped = true;
            zombieCollider.enabled = false;
            playerIsNear = false;
            zombieAnim.SetBool("isWalk", false);
            zombieAnim.SetBool("isRun", false);
            zombieAgent.enabled = false;
            this.enabled = false;
        }
    }

    //Metoda odpowiedzialna  za stan domyslny
    void WaypointState()
    {
        zombieAgent.acceleration = acceleration;
        fieldOfViewRange = waypointViewAngle;
        
        //Ustawienie predkosci
        zombieAgent.speed = 0.5f;
        //Ustawienie minimalnego dystansu pomiedzy przeciwnikem a punktem drogi
        zombieAgent.stoppingDistance = stopWaypointDis;
        //Obecny dystans pomiedzy punktem drogi a zombie
        float waypointDis = Vector3.Distance(transform.position, waypointZombie[currentWaypoint].transform.position);
        //waypointDis = (float)System.Math.Round(waypointDis, 1);
        //Jezeli dystans pomiedy punktem a przeciwnikiem jest wiekszy niz minimalny dystans to przeciwni zmierza do punktu
        //drogi
        if (!stayWaypoint && !pathInvalid)
        {
            if (waypointDis > (zombieAgent.stoppingDistance))
            {

                zombieAnim.SetBool("isWalk", true);
                zombieAnim.SetBool("isRun", false);
                
                zombieAgent.SetDestination(waypointZombie[currentWaypoint].transform.position);
                if (zombieAgent.pathStatus == NavMeshPathStatus.PathInvalid)
                {
                    pathInvalid = true;
                    if (help != null) help.SelectWaipoint();
                }
            }
            else if (waypointDis <= (zombieAgent.stoppingDistance))
            //W przeciwnim razie stan na klika sekund i zmien obecny cel 
            {
                if (zombieAgent.velocity.magnitude <= 0.1f)
                {
                    zombieAnim.SetBool("isWalk", false);
                    zombieAnim.SetBool("isRun", false);
                    stayWaypoint = true;
                }
                else
                {
                    zombieAnim.SetBool("isWalk", true);
                    zombieAnim.SetBool("isRun", false);
                }
                //Jezeli doszlsmy do ostatniego waypointa zmien waypoint na pierwszy w tablicy
                //<-- Tu był kod zmiany waypointu
            }
        }
        if (stayWaypoint && !pathInvalid)
        {
            if (currentWaypoint + 1 == waypointZombie.Length)
            {


                if (Counter(timeStay))
                {
                    currentWaypoint = 0;
                    stayWaypoint = false;
                }
            }
            //w przeciwnym razie zmien na kolejny w tablicy
            else
            {


                if (Counter(timeStay))
                {
                    currentWaypoint++;
                    stayWaypoint = false;
                }
            }
        }
        if(pathInvalid)
        {
            if (waypointHelp == null)
            {
                zombieAnim.SetBool("isWalk", false);
                zombieAnim.SetBool("isRun", false);
            }
            else
            {
                float disHelp = Vector3.Distance(transform.position, waypointHelp.position);
                if (disHelp > zombieAgent.stoppingDistance)
                {
                    zombieAnim.SetBool("isWalk", true);
                    zombieAnim.SetBool("isRun", false);
                    zombieAgent.SetDestination(waypointHelp.position);
                }
                else
                {
                    pathInvalid = false;
                    
                }
                

            }
        }
    }

    //Stan w ktorym przeciwnik goni i atakuje gracza
    void ChasePlayer()
    {
        stayWaypoint = false;
        zombieAgent.acceleration = 20f;
        //zombieAgent.stoppingDistance = stoppingDis;
        fieldOfViewRange = chaseViewAngle;

        if (seePlayer)
        {
            RotationZombie(Player);
            //Ustawienie gracza jako celu za  ktorym zombie podaza
            zombieAgent.SetDestination(Player.transform.position);

            //Odleglosc od przeciwnika do gracza
            float disPlayer = Vector3.Distance(transform.position, Player.transform.position);
        
            posPlayerMemory.transform.position = Player.transform.position;

            //ustawienie minimalnego dystansu pomiedzy graczem a przeciwnikiem
            zombieAgent.stoppingDistance = stoppingDis;
            //Jezeli dystans jest mniejszy od maksymalne i wiekszy od minimalnego dystansu
            //przeciwnik biegnie za graczem
            if (disPlayer < maxDisZombie && disPlayer > zombieAgent.stoppingDistance && !playerHealth.isDead())
            {
                
                zombieAnim.SetBool("isRun", true);

                counter = 0.0f;

                zombieAgent.speed = chaseSpeed;

            }
            //Jezeli dystans jest mniejszy lub równy minimalnemu dystansowi przeciwnik stoi 
            //i zaczyna atakowac gracza 
            if (disPlayer <= zombieAgent.stoppingDistance && !playerHealth.isDead())
            {
                
                zombieAnim.SetBool("isRun", false);

                //RotationZombie(Player);

                ZombieAttack();



            }
            //Jezeli dystans jest wiekszy lub równy maksymalnemu dystansowi przeciwnik wraca do 
            //stanu domyslnego
            if (disPlayer >= maxDisZombie)
            {
                playerIsNear = false;
                SelectWaypoint();

            }

            //Stan po pokonaniu gracza
            if (playerHealth.isDead())
            {


                if (zombieAgent.enabled) zombieAgent.isStopped = true;
                zombieAnim.SetBool("isRun", false);
                zombieAnim.SetBool("isWalk", false);

                if (Counter(3f))
                {

                    SelectWaypoint();
                    playerIsNear = false;
                    if (zombieAgent.enabled) zombieAgent.isStopped = false;
                    zombieAnim.SetBool("isWalk", true);
                }


            }


        }
        else
        {
            RotationZombie(posPlayerMemory);
            zombieAgent.stoppingDistance = stopWaypointDis;
            float disPosPlayerMemory = Vector3.Distance(transform.position, posPlayerMemory.transform.position);
            zombieAgent.SetDestination(posPlayerMemory.transform.position);

            if (disPosPlayerMemory > zombieAgent.stoppingDistance)
            {

                zombieAnim.SetBool("isRun", true);
                counter = 0.0f;



                zombieAgent.speed = chaseSpeed;

            }
            if (disPosPlayerMemory <= zombieAgent.stoppingDistance)
            {
                //RotationZombie(posPlayerMemory);
                zombieAnim.SetBool("isRun", false);
                zombieAnim.SetBool("isWalk", false);

                if (Counter(5f))
                {

                    SelectWaypoint();
                    playerIsNear = false;
                    
                    zombieAnim.SetBool("isWalk", true);
                }




            }
        }
    }

    //Metoda miezaca czas, jesli czas minie metoda zwraca true, przeciwnym razie false
    bool Counter(float _time)
    {
        counter += Time.deltaTime;
        if (counter >= _time)
        {
            counter = 0.0f;
            return true;
        }
        else return false;
    }
    //Metoda wybierajaca najblizszy punkt drogi od przeciwnika z dwoch zapisanych w tablicy
    public void SelectWaypoint()
    {
        float disWaypoint1 = Vector3.Distance(transform.position, waypointZombie[0].position);
        float disWaypoint2 = Vector3.Distance(transform.position, waypointZombie[1].position);
        if (disWaypoint1 < disWaypoint2)
        {
            currentWaypoint = 0;
        }
        else
        {
            currentWaypoint = 1;
        }
    }
    //Metoda odpowiadajaca za atak
	void ZombieAttack()
    {
        if(Time.time > breakInAttack && !playerHealth.isDead())
        {

            if (zombieAgent.enabled) zombieAgent.isStopped = true;
            zombieAnim.SetTrigger("Attack");
            zombieAudio.PlayOneShot(zombieAttackClip);
            Invoke("Damage", damageTime);
            
            breakInAttack = Time.time + timeBetweenAttack;
            Invoke("RunAfterAttack", afterAttackTime);
        }
    }
    //Metoda odpowiadajaca za zadawane obrazenia 
    void Damage()
    {
        float disPlayer = Vector3.Distance(transform.position, Player.transform.position);

        if (disPlayer <= (stoppingDis + offsetDisStop) && !zombieHealth.isDead()) //&& seePlayer
        {
            brianSound.PlayPainSound();
            playerHealth.ReduceHealth(Random.Range(minDamage, maxDamage));
        }
    }

    void RotationZombie(GameObject _obj)
    {
        Vector3 dir = (_obj.transform.position - transform.position);
        dir = dir.normalized;
        Vector3 dirXZ = new Vector3(dir.x, 0.0f, dir.z);
        if (dirXZ != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(dirXZ);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f); //10f
        }
    }

    void RunAfterAttack()
    {
        if(zombieAgent.enabled) zombieAgent.isStopped = false;
        //zombieAnim.SetBool("isRun", true);
    }

}
