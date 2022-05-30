using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.schiff
{
    public interface ICoverSystemAgent {
        bool CanNavigateTo(Vector3 target);
    }

    public class CoverSystem : MonoBehaviour
    {
        internal int NextCoverId = 1;

        public float MaxAngle = 80;

        public Dictionary<Transform, CoverDictionary> ChildCovers = new Dictionary<Transform, CoverDictionary>();
        public CoverDictionary RootCovers = new CoverDictionary();

        public CoverDictionary AllCovers = new CoverDictionary();

        public CoverDesc GetCover(int id)
        {
            return AllCovers[id];
        }

        public bool IsCoveringEnemyDirection(CoverDesc cover, Vector3 enemyPos)
        {
            Vector3 enemyPosInCoverGroup;
            if (cover.Parent != null)
            {
                enemyPosInCoverGroup = cover.Parent.InverseTransformPoint(enemyPos);
            }
            else
            {
                enemyPosInCoverGroup = enemyPos;
            }

            return Vector3.Angle(cover.Forward, (enemyPosInCoverGroup - cover.Position).normalized) < MaxAngle;
        }

        public bool IsInCover(int coverId, Transform self, Transform enemy, float selfHeight, float selfRadius, float tolerance)
        {
            var cover = GetCover(coverId);

            Vector3 playerPosInCoverGroup;
            if (cover.Parent != null)
            {
                playerPosInCoverGroup = cover.Parent.InverseTransformPoint(self.position);
            }
            else
            {
                playerPosInCoverGroup = self.position;
            }

            if (IsCoveringEnemyDirection(cover, enemy.position))
            {
                var relativePlayerPos = playerPosInCoverGroup - cover.Position;

                var distUp = relativePlayerPos.y;
                if (distUp + tolerance > 0 && distUp + selfHeight - tolerance < cover.Scale.y)
                {
                    var distBackward = -Vector2.Dot(relativePlayerPos, cover.Forward);
                    if (distBackward + tolerance > 0 && distBackward - tolerance < selfRadius)
                    {
                        var distRight = Vector2.Dot(relativePlayerPos, cover.Right);
                        if (distRight + tolerance > -cover.Scale.x / 2 && distRight - tolerance < cover.Scale.x / 2)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public bool FindNearestCover(GameObject self, Transform enemy, float selfRadius, out int coverId, out Vector3 positionInCover)
        {
            var coversByDistance = new SortedList<float, NearestCoverData>();

            FindNearestCover(selfRadius, self.transform.position, enemy.position, RootCovers, coversByDistance);

            foreach (var coverGroup in ChildCovers)
            {
                var playerPosInCoverGroup = coverGroup.Key.InverseTransformPoint(self.transform.position);
                var enemyPosInCoverGroup = coverGroup.Key.InverseTransformPoint(enemy.position);
                FindNearestCover(selfRadius, playerPosInCoverGroup, enemyPosInCoverGroup, coverGroup.Value, coversByDistance);
            }

            foreach (var item in coversByDistance.Values)
            {
                if (CheckCover(self.GetComponent<ICoverSystemAgent>(), item))
                {
                    coverId = item.Cover.Id;
                    positionInCover = item.Cover.Parent != null ? item.Cover.Parent.TransformPoint(item.PositionInCover) : item.PositionInCover;
                    return true;
                }
            }

            coverId = -1;
            positionInCover = Vector3.zero;
            return false;
        }

        private bool CheckCover(ICoverSystemAgent self, NearestCoverData item)
        {
            return self.CanNavigateTo(item.PositionInCover);
        }

        private void FindNearestCover(float selfRadius, Vector3 thisPosInCoverGroup, Vector3 enemyPosInCoverGroup, CoverDictionary covers, SortedList<float, NearestCoverData> coversByDistance)
        {
            foreach (var cover in covers.Values)
            {
                if (IsCoveringEnemyDirection(cover, enemyPosInCoverGroup))
                {
                    var left = new NearestCoverData
                    {
                        Distance = (thisPosInCoverGroup - cover.LeftEdge).sqrMagnitude,
                        PositionInCover = cover.LeftEdge + selfRadius * cover.Right + (-selfRadius) * cover.Forward,
                        Cover = cover
                    };
                    coversByDistance.Add(left.Distance, left);

                    var right = new NearestCoverData
                    {
                        Distance = (thisPosInCoverGroup - cover.RightEdge).sqrMagnitude,
                        PositionInCover = cover.RightEdge + (-selfRadius) * cover.Right + (-selfRadius) * cover.Forward,
                        Cover = cover
                    };
                    coversByDistance.Add(right.Distance, right);
                }
            }
        }

        private class NearestCoverData
        {
            public float Distance;
            public Vector3 PositionInCover;
            public CoverDesc Cover;
        }
    }
}