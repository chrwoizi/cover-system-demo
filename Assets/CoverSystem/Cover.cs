using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.schiff
{
    public class Cover : MonoBehaviour
    {
        void Start()
        {
            var system = FindObjectOfType<CoverSystem>();
            CoverDictionary covers;
            if (transform.parent != null)
            {
                if (!system.ChildCovers.TryGetValue(transform.parent, out covers))
                {
                    covers = new CoverDictionary();
                    system.ChildCovers.Add(transform.parent, covers);
                }
            }
            else
            {
                covers = system.RootCovers;
            }

            var forward = transform.parent != null ? transform.parent.InverseTransformDirection(transform.forward) : transform.forward;
            var right = Vector3.Cross(Vector3.up, transform.forward);

            var id = system.NextCoverId++;
            var desc = new CoverDesc
            {
                Id = id,
                Parent = transform.parent,
                Position = transform.localPosition,
                Forward = forward,
                Right = right,
                LeftEdge = transform.localPosition - right * transform.localScale.x / 2,
                RightEdge = transform.localPosition + right * transform.localScale.x / 2,
                Scale = transform.localScale
            };

            system.AllCovers.Add(id, desc);
            covers.Add(id, desc);

            Destroy(gameObject);
        }
    }
}