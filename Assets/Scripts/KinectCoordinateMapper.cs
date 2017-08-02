using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;


public class KinectCoordinateMapper : MonoBehaviour
{

    private KinectSensor kinectSensor;
    private MultiSourceFrameReader multiSouseFrameReader;
    private CoordinateMapper coordinateMapper;



    // Use this for initialization
    void Start()
    {
        kinectSensor = KinectSensor.GetDefault();
        multiSouseFrameReader = kinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.BodyIndex | FrameSourceTypes.Color | FrameSourceTypes.Depth);
        coordinateMapper = kinectSensor.CoordinateMapper;
    }

    // Update is called once per frame
    void Update()
    {

    }

}
