using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawManager : MonoBehaviour
{
    private Camera _cam;
    [SerializeField] private GameObject _linePrefab;
    [SerializeField] private GameObject _stylusTip;
    [SerializeField] private GameObject _plane;
    public const float POINT_TO_POINT_THRESHOLD = 0.005f;
    public const float REMOVE_THRESHOLD = 0.01f;
    public const float STYLUS_TO_PAD_THRESHOLD = 0.03f;

    private List<GameObject> _lines;
    private GameObject _currentLine;

    private Vector3 _previousPlanePos;
    private Vector3 _previousPlaneNormal;
    [SerializeField] GameObject _testLine;

    // Start is called before the first frame update
    void Start()
    {
        _cam = Camera.main;
        _lines = new List<GameObject>();
        _previousPlanePos = _plane.GetComponent<Transform>().position;
        _previousPlaneNormal = _plane.GetComponent<Transform>().up;
        _lines.Add(_testLine);
    } 

    // Update is called once per frame
    void Update()
    {
        //_plane.GetComponent<Transform>().position += new Vector3(0, 0, 0.01f);
        Vector3 currentPlanePos = _plane.GetComponent<Transform>().position;
        Vector3 currentPlaneNormal = _plane.GetComponent<Transform>().up;

        for (int i = 0; i < _lines.Count; i++)
        {
            _lines[i].GetComponent<Line>().UpdatePositions(_previousPlanePos, _previousPlaneNormal, currentPlanePos, currentPlaneNormal);
        }

        _previousPlanePos = new Vector3(currentPlanePos.x, currentPlanePos.y, currentPlanePos.z);

        Vector3 stylusTipPos = _stylusTip.transform.position;
        Vector3 stylusTipFwd = _stylusTip.transform.forward;
        bool hit;
        RaycastHit hitInfo;

        // Starting a new line
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            hit = Physics.Raycast(_stylusTip.transform.position, stylusTipFwd, out hitInfo, STYLUS_TO_PAD_THRESHOLD);
            if (hit)
            {
                Vector3 hitPadPos = hitInfo.point + hitInfo.normal*0.001f;
                _currentLine = Instantiate(_linePrefab, hitPadPos, Quaternion.identity);
                _lines.Add(_currentLine);
            }
            
        }

        // Continuing the started line
        else if (OVRInput.Get(OVRInput.Button.One))
        {
            hit = Physics.Raycast(_stylusTip.transform.position, stylusTipFwd, out hitInfo, STYLUS_TO_PAD_THRESHOLD);
            if (hit)
            {
                Vector3 hitPadPos = hitInfo.point + hitInfo.normal*0.001f;
                _currentLine.GetComponent<Line>().SetPosition(hitPadPos);
            }
        }

        // Erasing a line
        else if (OVRInput.Get(OVRInput.Button.Two))
        {
            // iterate through all the lines
            for (int i = 0; i < _lines.Count; i++)
            {
                GameObject currLine = _lines[i];
                bool isRemoved = false;
                Vector3[] splitLinePositions0;  // stores the first half of the split line
                Vector3[] splitLinePositions1;  // stores the second half of the split line

                // try erasing the line at the stylus tip position
                isRemoved = currLine.GetComponent<Line>().RemovePosition(out splitLinePositions0, out splitLinePositions1, stylusTipPos);

                // if a point on the line is erased
                if (isRemoved)
                {
                    GameObject splitLine;

                    // Create a new line for the first half of the split line
                    if (splitLinePositions0 != null)
                    {
                        splitLine = Instantiate(_linePrefab, stylusTipPos, Quaternion.identity);
                        splitLine.GetComponent<Line>().SetPositions(splitLinePositions0);
                        _lines.Add(splitLine);
                    }

                    // Create a new line for the second half of the split line
                    if (splitLinePositions1 != null)
                    {
                        splitLine = Instantiate(_linePrefab, stylusTipPos, Quaternion.identity);
                        splitLine.GetComponent<Line>().SetPositions(splitLinePositions1);
                        _lines.Add(splitLine);
                    }

                    // Destroy the line object that was erased
                    _lines.RemoveAt(i);
                    Destroy(currLine);
                }
            }
        }

        // Clear all
        else if (OVRInput.Get(OVRInput.Button.Three))
        {
            for (int i = 0; i < _lines.Count; i++)
            {
                Destroy(_lines[i]);
                _lines.RemoveAt(i);
            }
        }
    }
}
