using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RedBjorn.ProtoTiles;
using RedBjorn.ProtoTiles.Example;
using RedBjorn.Utils;
using TMPro;
using UnityEngine;

/// <summary>
/// Handles asynchronous task execution of unit. Responsible for reference to unit components, Unity-side communication and synchronization of unit.
/// </summary>
abstract public class UnitTaskHandler : TaskHandler
{
    protected float _walkSpeed = 1f;
    protected float _walkRange = 100f;
    protected Quaternion TargetRotation; // To implement smooth rotation
    protected float RotateSpeed = 5f; // To control speed of smooth rotation

    public Transform RotationNode;
    public SpriteRenderer HighlightCircle;
    public Outline HighlightOutline;
    public List<TMP_Text> TextLabels;
    public ParticleSystem HealParticle;
    public PathDrawer PathPrefab;
    
    private PathDrawer PathDrawer;
    protected Unit Unit { get; private set; }

    public void Init(Unit unit, MapEntity map)
    {
        base.Init(map);
        Unit = unit;
        TargetRotation = RotationNode.rotation;
        PathCreate();
    }

    public override void Update()
    {
        if (RotationNode.rotation != TargetRotation) 
        {
            RotationNode.rotation = Quaternion.Lerp(RotationNode.rotation, TargetRotation, RotateSpeed * Time.deltaTime);
        }
    }

    private void PathCreate()
    {
        if (PathPrefab != null)
        {
            PathDrawer = Spawner.Spawn(PathPrefab, Vector3.zero, Quaternion.identity);
            PathDrawer.Show(new List<Vector3>() { }, Map);
            PathDrawer.InactiveState();
            PathDrawer.IsEnabled = true;
        }
    }

    private void PathUpdate(List<TileEntity> path)
    {
        if (PathDrawer != null)
        {
            if (path != null && path.Any())
            {
                var pathPts = path.Select(t => Map.WorldPosition(t.Position)).ToList();

                PathDrawer.Show(pathPts, Map);
                PathDrawer.ActiveState();
            }
            else
            {
                PathDrawer.Hide();
                PathDrawer.InactiveState();
            }
        }
    }

    protected IEnumerator Moving(List<TileEntity> path, Func<TileEntity, bool> onCompleteStep, Action onCompletePath)
    {
        var nextIndex = 0;
        transform.position = Map.Settings.Projection(transform.position);

        Unit.Animate("TriggerWalk");
        PathUpdate(path);
        while (nextIndex < path.Count)
        {
            var nextTile = path[nextIndex];
            var targetPoint = Map.WorldPosition(nextTile);
            var stepDir = (targetPoint - transform.position) * _walkSpeed;

            var displayPath = path.GetRange(nextIndex, path.Count - nextIndex);
            PathUpdate(displayPath);
            if (Map.RotationType == RotationType.LookAt && stepDir != Vector3.zero)
            {
                TargetRotation = Quaternion.LookRotation(stepDir, Vector3.up) * Quaternion.Euler(0f, 90f, 0f);
            }
            else if(Map.RotationType == RotationType.Flip)
            {
                TargetRotation = Map.Settings.Flip(stepDir);
            }
            var reached = stepDir.sqrMagnitude < 0.01f;
            while (!reached)
            {
                transform.position += stepDir * Time.deltaTime;
                reached = Vector3.Dot(stepDir, targetPoint - transform.position) < 0f;
                yield return null;
            }
            transform.position = targetPoint;
            nextIndex++;

            if (onCompleteStep.SafeInvoke(nextTile)) 
            {
                PathUpdate(null);
                yield break;
            }
        }

        Unit.Animate("TriggerIdle");
        PathUpdate(null);
        onCompletePath.SafeInvoke();
    }

    /// <summary>
    /// Handle unit movement to a certain tile
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="onCompleteStep">Callback function fired when a step is traversed. Return true to terminate ongoing movement</param>
    /// <param name="onCompletePath">Callback function fired when the entire path is traversed</param>
    public void HandleMoveToTile(TileEntity tile, List<TileOccupant> ignoreOccupantSet = null, Func<TileEntity, bool> onCompleteStep = null, Action onCompletePath = null) 
    {
        if (tile != null)
        {
            var path = Map.PathTiles(transform.position, Map.WorldPosition(tile), _walkRange, ignoreOccupantSet);

            if (path != null && path.Any())
            {
                if (CurrentTask != "despawn" && TaskCoroutine != null) 
                {
                    StopCoroutine(TaskCoroutine);
                }
                CurrentTask = "move";
                TaskCoroutine = StartCoroutine(
                    Moving(path, onCompleteStep, 
                        () => {
                            onCompletePath.SafeInvoke();
                            CurrentTask = "idle";
                        }
                    )
                );
            }
            else
            {
                onCompletePath.SafeInvoke();
            }
        }
    }
}