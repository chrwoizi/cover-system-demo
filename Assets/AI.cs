using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.schiff;

public class AI : MonoBehaviour, ICoverSystemAgent
{
    public bool HasCover;
    public int CoverId;
    public Vector3 PositionInCover;
    
    public float ModelHeight = 1.85f;
    public float ModelRadius = 0.3f;
    public float NavMeshSearchDistance = 2;

    void Update()
    {
        if(this.HasCover) {
            var nav = GetComponent<UnityEngine.AI.NavMeshAgent>();
            var coverSystem = FindObjectOfType<CoverSystem>();
            var enemy = FindObjectOfType<Enemy>();

            // Validate cover
            var cover = coverSystem.GetCover(this.CoverId);
            if(coverSystem.IsCoveringEnemyDirection(cover, enemy.transform.position)) {
                MoveToCover();
            }
            else {
                FindCover();
            }
        }
        else {
            FindCover();
        }
    }

    void MoveToCover() {
        var nav = GetComponent<UnityEngine.AI.NavMeshAgent>();
        var coverSystem = FindObjectOfType<CoverSystem>();
        var enemy = FindObjectOfType<Enemy>();
        var npc = this;

        if(!coverSystem.IsInCover(this.CoverId, this.transform, enemy.transform, npc.ModelHeight, npc.ModelRadius, 0.2f)){
            npc.Navigate(this.PositionInCover, null);
        }
    }

    void FindCover() {
        var nav = GetComponent<UnityEngine.AI.NavMeshAgent>();
        var coverSystem = FindObjectOfType<CoverSystem>();
        var enemy = FindObjectOfType<Enemy>();
        var npc = this;

        int coverId;
        Vector3 positionInCover;
        this.HasCover = coverSystem.FindNearestCover(npc.gameObject, enemy.transform, npc.ModelRadius, out coverId, out positionInCover);
        this.CoverId = coverId;
        this.PositionInCover = positionInCover;
        if(this.HasCover) {
            if(nav != null) {
                Debug.Log("stop");
                //nav.Stop();
            }
            MoveToCover();
        }
        else {
            // Follow
            npc.Navigate(enemy.transform.position, null);
        }
    }
    
    public bool CanNavigateTo(Vector3 destination)
    {
        var nav = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (!nav.isOnNavMesh)
        {
            return false;
        }

        UnityEngine.AI.NavMeshHit toHit;
        UnityEngine.AI.NavMesh.SamplePosition(destination, out toHit, NavMeshSearchDistance, nav.areaMask);
        if (!toHit.hit)
        {
            return false;
        }

        var path = new UnityEngine.AI.NavMeshPath();
        return nav.CalculatePath(toHit.position, path);
    }

    public bool Navigate(Vector3 destination, float? destinationRotation)
    {
        var nav = GetComponent<UnityEngine.AI.NavMeshAgent>();

        if (Vector3.Distance(this.transform.position, destination) < 0.1f)
        {
            return true;
        }

        if (!this.StopNavigation())
        {
            return false;
        }

        UnityEngine.AI.NavMeshHit fromHit;
        UnityEngine.AI.NavMesh.SamplePosition(transform.position, out fromHit, NavMeshSearchDistance, nav.areaMask);
        if (!fromHit.hit)
        {
            Debug.LogError("Cannot navigate from " + transform.position);
            return false;
        }

        UnityEngine.AI.NavMeshHit toHit;
        UnityEngine.AI.NavMesh.SamplePosition(destination, out toHit, NavMeshSearchDistance, nav.areaMask);
        if (!toHit.hit)
        {
            Debug.LogError("Cannot navigate to " + destination);
            return false;
        }
        
        Debug.Log("navigate to " + toHit.position);
        nav.destination = toHit.position;

        return true;
    }

    bool StopNavigation() {
        var nav = GetComponent<UnityEngine.AI.NavMeshAgent>();
        Debug.Log("stop");
        //nav.Stop();
        return true;
    }
}
