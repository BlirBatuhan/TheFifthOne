using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Fusion;

public class MonsterAI : NetworkBehaviour
{
    public enum AIState { Idle, Patrol, Chase, Attack }
    public AIState currentState = AIState.Idle;

    [Header("G�r�� Ayarlar�")]
    public float detectionRadius = 10f;
    public float viewAngle = 90f;
    public Transform eyePoint;
    public Transform attackPoint;
    public LayerMask playerMask;
    public LayerMask obstacleMask;
    float[] blendValues = { 0f, 0.25f, 0.5f, 0.75f, 1f };
    private float currentSpeed = 0f;

    [Header("Takip ve Sald�r�")]
    public float attackDistance = 2f;
    private bool isAttacking = false;

    [Header("Audios")]
    private AudioSource biteAudio;
    public AudioClip biteClip;
    

    [Header("Effects")]
    public GameObject bloodEffect;
    public GameObject kickEffect;

    [Header("Devriye")]
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;

    private NavMeshAgent agent;
    private Transform targetPlayer;
    private Animator anim;
    
    


    public override void Spawned()
    {
        biteAudio = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        currentSpeed = agent.speed;

        if (patrolPoints.Length > 0)
            currentState = AIState.Patrol;
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        switch (currentState)
        {
            case AIState.Idle:
                SearchForPlayer();
                break;

            case AIState.Patrol:
                Patrol();
                break;

            case AIState.Chase:
                ChasePlayer();
                break;

            case AIState.Attack:
                AttackPlayer();
                break;
        }
        if (targetPlayer == null && currentState == AIState.Attack)
        {
            anim.SetBool("attack", false);
            currentState = patrolPoints.Length > 0 ? AIState.Patrol : AIState.Idle;
        }
    }


    void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        agent.isStopped = false;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            // Hedefe ula�t�
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
        else if (!agent.hasPath)
        {
            // Bazen remainingDistance bugl� kal�yor, garanti i�in ek kontrol
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }

        anim.SetFloat("speed", Mathf.Lerp(anim.GetFloat("speed"), 0.3f, Time.deltaTime * 2f));

        Debug.Log("Devriye geziyor.");

        // Oyuncuyu g�r�rse Chase�e ge�
        SearchForPlayer();
    }

    void SearchForPlayer()
    {
        agent.isStopped = false; 
        Collider[] targets = Physics.OverlapSphere(transform.position, detectionRadius, playerMask);

        foreach (Collider target in targets)
        {
            //g�z pozisyonuyla hedef aras�ndaki y�n� hesapla
            Vector3 targetPoint = target.transform.position + Vector3.up * 1.5f;
            Vector3 dirToTarget = (targetPoint - eyePoint.position).normalized;
            // Hedefe olan a��y� kontrol et
            float angle = Vector3.Angle(eyePoint.forward, dirToTarget);

            Debug.DrawRay(eyePoint.position, dirToTarget * detectionRadius, Color.red);

            if (angle < viewAngle / 1.5f)
            {
                // Engel kontrol� - e�er ray bir engele �arparsa oyuncuyu g�remeyiz
                if (Physics.Raycast(eyePoint.position, dirToTarget, out RaycastHit hit, detectionRadius, obstacleMask))
                {
                    
                    continue; 
                }

                Debug.Log("Oyuncu bulundu! Takip ba�lat�l�yor.");
                targetPlayer = target.transform;
                currentState = AIState.Chase;
                return;
            }
        }
    }

    void ChasePlayer()
    {
        if (targetPlayer == null)
        {
            currentState = AIState.Patrol;
            return;
        }

        Debug.Log("Takip ediliyor.");
        
        agent.SetDestination(targetPlayer.position);

        float distance = Vector3.Distance(transform.position, targetPlayer.position);

        if(distance <= detectionRadius/2)
        {
            if (gameObject.name == "Zombie")
            {
                anim.SetFloat("speed", Mathf.Lerp(anim.GetFloat("speed"), 0.5f, Time.deltaTime * 1f));
                agent.speed = 1.2f;
            }
            else
            {
                agent.speed = 1.5f; // H�z artt�r�l�yor
                anim.SetFloat("speed", Mathf.Lerp(anim.GetFloat("speed"), 1f, Time.deltaTime * 2f));
            }
            
        }
        else
        {
            anim.SetFloat("speed", Mathf.Lerp(anim.GetFloat("speed"), 0.2f, Time.deltaTime * 5f));
            agent.speed = currentSpeed; // Normal h�za d�n
        }

        if (distance <= attackDistance)
        {
            currentState = AIState.Attack;
        }

        // Oyuncu �ok uzakla�t�ysa takibi b�rak
        if (distance > detectionRadius * 1.5f)
        {
            agent.isStopped = true;
            agent.ResetPath();
            Debug.Log("Oyuncu �ok uzakla�t�, devriye ba�lat�l�yor.");
            targetPlayer = null;
            currentState = AIState.Patrol;
            anim.SetFloat("speed", 0);
        }
    }

    void AttackPlayer()
    {
        if (targetPlayer == null)
        {
            currentState = AIState.Patrol;
            return;
        }
        isAttacking = true;
        anim.SetFloat("speed", 0);
        anim.SetBool("attack", true);
        
        Vector3 lookDirection = targetPlayer.position - transform.position;
        lookDirection.y = 0; // canavarin geriye yatmasi engellendi

        if (lookDirection != Vector3.zero) 
        {
            transform.rotation = Quaternion.LookRotation(lookDirection.normalized);
        }

        Debug.Log("Sald�r�yor!");

    }

    public void ChangeRandomAttack()
    {
        int randomIndex = Random.Range(0, blendValues.Length);
        float randomBlend = blendValues[randomIndex];
        anim.SetFloat("rand", randomBlend);
        Debug.Log($"Animation Event - Yeni rastgele de�er: {randomBlend}");
    }

    public void AttackHit()
    {
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackDistance, playerMask);

        foreach (Collider enemy in hitEnemies)
        {
            
            enemy.GetComponent<CharacterHealth>().TakeDamageRPC(5);
            if (anim.GetFloat("rand") == 0.75f || anim.GetFloat("rand") == 1f)
            {
                Vector3 kuvvetVector = (enemy.transform.position - transform.position).normalized * 15f;
                enemy.GetComponent<Animator>().applyRootMotion = false; // Animasyon k�k hareketini devre d��� b�rak
                enemy.GetComponent<Hareket>().ApplyExternalForceRPC(kuvvetVector);
            }
            else
            {
                bloodEffect.SetActive(true);
                biteAudio.PlayOneShot(biteClip);
            }
        }
    }


    public void FinishAttack()
    {
        isAttacking = false; // art�k sald�r� bitti animation event
        // Sald�r� tamamland�, tekrar chase durumuna ge�ebilir
        if (targetPlayer != null)
            currentState = AIState.Chase;

        anim.SetBool("attack", false);
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(attackPoint.position, attackDistance);

        // G�r�� a��s�n� g�ster
        if (eyePoint != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 leftBoundary = Quaternion.AngleAxis(-viewAngle / 2, Vector3.up) * eyePoint.forward * detectionRadius;
            Vector3 rightBoundary = Quaternion.AngleAxis(viewAngle / 2, Vector3.up) * eyePoint.forward * detectionRadius;

            Gizmos.DrawLine(eyePoint.position, eyePoint.position + leftBoundary);
            Gizmos.DrawLine(eyePoint.position, eyePoint.position + rightBoundary);
        }
    }
}