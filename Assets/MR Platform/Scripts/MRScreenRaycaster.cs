using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace MRPlatform
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(MRScreenInput))]
    [DisallowMultipleComponent]
    public class MRScreenRaycaster : GraphicRaycaster
    {
        private MRScreenInput m_MRScreenInput;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_RayEmitter = new GameObject("Ray Emitter");
            m_RayEmitter.transform.SetParent(this.transform);
            m_RayEmitter.transform.localPosition = new Vector3(10000, 10000, -0.01f);
            m_RayEmitter.transform.localRotation = Quaternion.identity;
             m_RayEmitter.transform.localScale = Vector3.one;
            m_MRScreenInput = GetComponent<MRScreenInput>();

        }

        private GameObject m_RayEmitter;


        struct GraphicHit
        {
            public Graphic graph;
            public Vector3 worldPos;
        }
        private Canvas _canvas = null;
        private Canvas canvas
        {
            get { return _canvas != null ? _canvas : _canvas = GetComponent<Canvas>(); }
        }

        private Camera _eventCamera = null;
        public override Camera eventCamera
        {
            get { return _eventCamera != null ? _eventCamera : _eventCamera = Camera.main; }
        }

        //public GameObject RayEmitter;

        public bool DrawDebugRay = false;
        public float Distance = 2000;

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            m_RayEmitter.transform.localPosition = new Vector3(m_MRScreenInput.x,m_MRScreenInput.y, -0.01f);

            if (enabled == false || m_RayEmitter == null)
                return;
            Ray ray = new Ray(m_RayEmitter.transform.position, m_RayEmitter.transform.forward);

            float hitDistance = float.MaxValue;

            if (blockingObjects != BlockingObjects.None)
            {
                float dist = Distance;
                if (blockingObjects == BlockingObjects.ThreeD || blockingObjects == BlockingObjects.All)
                {
                    var hits = Physics.RaycastAll(ray, dist, m_BlockingMask);
                    if (hits.Length > 0 && hits[0].distance < hitDistance)
                    {
                        hitDistance = hits[0].distance;
                    }
                }

                if (blockingObjects == BlockingObjects.TwoD || blockingObjects == BlockingObjects.All)
                {
                    var hits = Physics2D.GetRayIntersectionAll(ray, dist, m_BlockingMask);

                    if (hits.Length > 0 && hits[0].fraction * dist < hitDistance)
                    {
                        hitDistance = hits[0].fraction * dist;
                    }
                }
            }

            List<GraphicHit> sortedGraphics = new List<GraphicHit>();

            var list = GraphicRegistry.GetGraphicsForCanvas(canvas);
            for (int i = 0; i < list.Count; ++i)
            {
                GraphicHit hit;
                hit.graph = null;
                hit.worldPos = Vector3.zero;

                Graphic g = list[i];

                if (null == g || g.depth == -1 || !g.enabled || !g.raycastTarget || g.canvasRenderer.cull)
                {
                    continue;
                }

                if (!RayGraphicIntersectFlat(ray, g, hitDistance, ref hit))
                {
                    continue;
                }

                sortedGraphics.Add(hit);
            }

            sortedGraphics.Sort((g1, g2) => g2.graph.depth.CompareTo(g1.graph.depth));

            if (sortedGraphics.Count == 0)
            {
                return;
            }
            if (DrawDebugRay)
                Debug.DrawLine(ray.origin, sortedGraphics[0].worldPos, Color.green);

            for (int i = 0; i < sortedGraphics.Count; ++i)
            {
                var castResult = new RaycastResult
                {
                    gameObject = sortedGraphics[i].graph.gameObject,
                    module = this,
                    distance = (sortedGraphics[i].worldPos - ray.origin).magnitude,
                    index = resultAppendList.Count,
                    depth = sortedGraphics[i].graph.depth,
                    worldPosition = sortedGraphics[i].worldPos,
                    sortingLayer = canvas.sortingLayerID,
                    sortingOrder = canvas.sortingOrder,
                };
                resultAppendList.Add(castResult);
                //Debug.Log("raycaster Result�� " + castResult.gameObject.name);
                
            }
        }
        private bool RayGraphicIntersectFlat(Ray ray, Graphic graphic, float dist, ref GraphicHit hit)
        {
            hit.graph = null;

            Ray localRay = ray;

            Matrix4x4 worldToLocal = graphic.transform.worldToLocalMatrix;

            localRay.origin = worldToLocal.MultiplyPoint(ray.origin);
            localRay.direction = worldToLocal.MultiplyVector(ray.direction);

            localRay.direction.Normalize();

            Rect rc = graphic.rectTransform.rect;

            float t = -1;

            if (!RayRectIntersect(localRay, rc, dist, out t))
            {
                return false;
            }

            Matrix4x4 localToWorld = worldToLocal.inverse;

            hit.graph = graphic;
            hit.worldPos = localToWorld.MultiplyPoint(localRay.GetPoint(t));

            //Use Graphic.Raycast to detected whether the hit position has been discard by Mask2D or Mask
            return graphic.Raycast(eventCamera.WorldToScreenPoint(hit.worldPos), eventCamera);
        }

        public bool RayRectIntersect(Ray ray, Rect rc, float dist, out float t)
        {

            Plane plane = new Plane(new Vector3(0, 0, -1), 0);

            if (!plane.Raycast(ray, out t))
            {
                return false;
            }

            if (t < 0 || t > dist)
            {
                return false;
            }

            return rc.Contains(ray.GetPoint(t));
        }
    }

}
