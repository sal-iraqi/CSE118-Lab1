using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static T[] SubArray<T>(this T[] array, int offset, int length)
    {
        return new List<T>(array)
                    .GetRange(offset, length)
                    .ToArray();
    }
}

public class Line : MonoBehaviour
{
    [SerializeField] private LineRenderer _renderer;
    private Vector3 _currentPlaneTransform;
    private Vector3 _previousPlaneTransform;

    void Start()
    {
        _currentPlaneTransform = GameObject.FindGameObjectWithTag("Plane").GetComponent<Transform>().position;
        _previousPlaneTransform = _currentPlaneTransform;
    }

    void Update()
    {
        //Vector3[] positions = new Vector3[_renderer.positionCount];
        //_renderer.GetPositions(positions);
        //_currentPlaneTransform += new Vector3(0, 0, 0.01f);
        /*Debug.Log("_currentPlaneTransform: " + _currentPlaneTransform);
        Debug.Log("_previousPlaneTransform: " + _previousPlaneTransform);

        for (int i = 0; i < _renderer.positionCount; i++)
        {
            _renderer.SetPosition(i,_renderer.GetPosition(i) + (_currentPlaneTransform - _previousPlaneTransform));
        }

        _previousPlaneTransform = _currentPlaneTransform;*/
    }

    // Adds a point to the line renderers
    public void SetPosition(Vector3 pos)
    {
        // if cannot append, don't do anything
        if (!CanAppend(pos))
            return;

        // add a point to the line renderer
        _renderer.positionCount++;
        _renderer.SetPosition(_renderer.positionCount - 1, pos);
    }

    // Checks if a point can be appended to the line renderer
    private bool CanAppend(Vector3 pos)
    {
        if (_renderer.positionCount == 0)
            return true;

        // if the distance between previous point and new position is greater than the threshold, then it can append
        if (Vector3.Distance(_renderer.GetPosition(_renderer.positionCount - 1), pos) > DrawManager.POINT_TO_POINT_THRESHOLD)
        {
            Debug.Log("Can append");
            return true;
        }
        // otherwise not enough distance between previous and new point, so don't append
        else
        {
            return false;
        }
    }

    // Draw the whole line
    public void SetPositions(Vector3[] positions)
    {
        _renderer.positionCount = positions.Length;
        _renderer.SetPositions(positions);
    }

    // Removes a point and splits the line into two parts
    public bool RemovePosition(out Vector3[] linePositions0, out Vector3[] linePositions1, Vector3 pos)
    {
        linePositions0 = null;
        linePositions1 = null;
        int pointIndex;

        // If no point to remove, then don't do anything
        if (!CanRemove(out pointIndex, pos))
            return false;

        Vector3[] positions = new Vector3[_renderer.positionCount];
        _renderer.GetPositions(positions);

        // Get first part of the to-be-split line excluding the removed point (only if there are at least 2 points)
        if(pointIndex > 1)
            linePositions0 = positions.SubArray(0, pointIndex);

        // Get second part of the to-be-split line excluding the removed point (only if there are at least 2 points)
        if (pointIndex < positions.Length - 2)
            linePositions1 = positions.SubArray(pointIndex + 1, positions.Length - pointIndex - 1);

        // Return true for removing the point
        return true;
    }

    // Checks if a point is close enough from the passed position to be removed
    private bool CanRemove(out int pointIndex, Vector3 pos)
    {
        Vector3[] positions = new Vector3[_renderer.positionCount];
        int numPoints = _renderer.GetPositions(positions);

        // Iterate through all the points in the line
        for (int i = 0; i < numPoints; i++)
        {
            // If distance between point and the erasing pos is less than the threshold
            if (Vector3.Distance(positions[i], pos) < DrawManager.REMOVE_THRESHOLD)
            {
                // return which point to be removed
                pointIndex = i;
                return true;
            }
        }

        // otherwise, no point can be removed
        pointIndex = 0;
        return false;
    }

    public void UpdatePositions(Vector3 pPos, Vector3 pNormal, Vector3 cPos, Vector3 cNormal)
    {
        Vector3 deltaFromTranslation = cPos - pPos;
        float angleDelta = Vector3.Angle(pNormal, pNormal);
        Vector3 rotAxis = Vector3.Cross(pNormal, cNormal);
        
        for (int i = 0; i < _renderer.positionCount; i++)
        {
            Vector3 centerToPoint = _renderer.GetPosition(i) - pPos;
            Vector3 deltaFromRotation = pPos + Quaternion.AngleAxis(angleDelta, rotAxis) * centerToPoint;

            //Vector3 delta = deltaFromTranslation + deltaFromRotation;
            Vector3 delta = deltaFromTranslation;
            _renderer.SetPosition(i, _renderer.GetPosition(i) + delta);
        }
    }
}
