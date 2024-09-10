// using RedBjorn.Utils;
// using System;
// using System.Collections;
// using System.Collections.Generic;
// using Unity.VisualScripting;
// using UnityEngine;

// namespace RedBjorn.ProtoTiles.Example
// {
//     public class UnitMove : MonoBehaviour
//     {
//         public float Speed = 5;
//         public float Range = 10f;
//         public Transform RotationNode;
//         public AreaOutline AreaPrefab;
//         public PathDrawer PathPrefab;

//         Quaternion _targetRotation; // To implement smooth rotation
//         float _rotateSpeed = 5f; // To control speed of smooth rotation
//         MapEntity Map;
//         AreaOutline Area;
//         PathDrawer Path;
//         Coroutine MovingCoroutine;
        
//         void Update()
//         {
//             if (RotationNode.rotation != _targetRotation) {
//                 RotationNode.rotation = Quaternion.Lerp(RotationNode.rotation, _targetRotation, _rotateSpeed * Time.deltaTime);
//             }
//             if (MyInput.GetOnWorldUp(Map.Settings.Plane()))
//             {
//                 HandleWorldClick();
//             }
//             PathUpdate();
//         }

//         public void Init(MapEntity map)
//         {
//             Map = map;
//             Area = Spawner.Spawn(AreaPrefab, Vector3.zero, Quaternion.identity);
//             AreaShow();
//             PathCreate();
//             _targetRotation = RotationNode.rotation;
//         }

//         void HandleWorldClick() 
//         {
//             var clickPos = MyInput.GroundPosition(Map.Settings.Plane());
//             var tile = Map.Tile(clickPos);

//             if (tile != null && tile.Vacant)
//             {
//                 Debug.Log("Clicked at " + tile.Data.TilePos);
//                 AreaHide();
//                 Path.IsEnabled = false;
//                 PathHide();
//                 var path = Map.PathTiles(transform.position, clickPos, Range);
//                 Move(path, null, () =>
//                 {
//                     Path.IsEnabled = true;
//                     AreaShow();
//                 });
//             }
//         }

//         /// <summary>
//         /// Handle unit movement to a certain tile
//         /// </summary>
//         /// <param name="tile"></param>
//         /// <param name="onCompleteStep">Callback function fired when a step is traversed. Return true to terminate ongoing movement</param>
//         /// <param name="onCompletePath">Callback function fired when the entire path is traversed</param>
//         public void MoveToTile(TileEntity tile, Func<TileEntity, bool> onCompleteStep, Action onCompletePath) {
//             if (tile != null && tile.Vacant)
//             {
//                 AreaHide();
//                 Path.IsEnabled = false;
//                 PathHide();
//                 var path = Map.PathTiles(transform.position, Map.WorldPosition(tile), Range);
//                 Move(path, onCompleteStep, () =>
//                 {
//                     Path.IsEnabled = true;
//                     AreaShow();
//                     onCompletePath();
//                 });
//             }
//         }


//         public void Move(List<TileEntity> path, Func<TileEntity, bool> onCompleteStep, Action onCompletePath)
//         {
//             if (path != null)
//             {
//                 if (MovingCoroutine != null)
//                 {
//                     StopCoroutine(MovingCoroutine);
//                 }
//                 MovingCoroutine = StartCoroutine(Moving(path, onCompleteStep, onCompletePath));
//             }
//             else
//             {
//                 onCompletePath.SafeInvoke();
//             }
//         }

//         IEnumerator Moving(List<TileEntity> path, Func<TileEntity, bool> onCompleteStep, Action onCompletePath)
//         {
//             var nextIndex = 0;
//             transform.position = Map.Settings.Projection(transform.position);

//             while (nextIndex < path.Count)
//             {
//                 var nextTile = path[nextIndex];
//                 var targetPoint = Map.WorldPosition(nextTile);
//                 var stepDir = (targetPoint - transform.position) * Speed;
//                 if (Map.RotationType == RotationType.LookAt)
//                 {
//                     _targetRotation = Quaternion.LookRotation(stepDir, Vector3.up) * Quaternion.Euler(0f, 90f, 0f);
//                 }
//                 else if(Map.RotationType == RotationType.Flip)
//                 {
//                     _targetRotation = Map.Settings.Flip(stepDir);
//                 }
//                 var reached = stepDir.sqrMagnitude < 0.01f;
//                 while (!reached)
//                 {
//                     transform.position += stepDir * Time.deltaTime;
//                     reached = Vector3.Dot(stepDir, (targetPoint - transform.position)) < 0f;
//                     yield return null;
//                 }
//                 transform.position = targetPoint;
//                 nextIndex++;

//                 if (onCompleteStep.SafeInvoke(nextTile)) {
//                     yield break;
//                 }
//             }
//             onCompletePath.SafeInvoke();
//         }

//         void AreaShow()
//         {
//             AreaHide();
//             Area.Show(Map.WalkableBorder(transform.position, Range), Map);
//         }

//         void AreaHide()
//         {
//             Area.Hide();
//         }

//         void PathCreate()
//         {
//             if (!Path)
//             {
//                 Path = Spawner.Spawn(PathPrefab, Vector3.zero, Quaternion.identity);
//                 Path.Show(new List<Vector3>() { }, Map);
//                 Path.InactiveState();
//                 Path.IsEnabled = true;
//             }
//         }

//         void PathHide()
//         {
//             if (Path)
//             {
//                 Path.Hide();
//             }
//         }

//         void PathUpdate()
//         {
//             if (Path && Path.IsEnabled)
//             {
//                 var tile = Map.Tile(MyInput.GroundPosition(Map.Settings.Plane()));
//                 if (tile != null && tile.Vacant)
//                 {
//                     var path = Map.PathPoints(transform.position, Map.WorldPosition(tile.Position), Range);
//                     Path.Show(path, Map);
//                     Path.ActiveState();
//                     Area.ActiveState();
//                 }
//                 else
//                 {
//                     Path.InactiveState();
//                     Area.InactiveState();
//                 }
//             }
//         }
//     }
// }
