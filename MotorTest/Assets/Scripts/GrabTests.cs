﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class GrabTests : MonoBehaviour
{
    public SteamVR_Input_Sources handType;
    public SteamVR_Action_Boolean teleportAction;
    public SteamVR_Action_Boolean grabAction;

    public SteamVR_Behaviour_Pose controllerPose;

    private GameObject collidingObject;
    private GameObject objectinhand;
    private GameObject ThrownObject;

    private void SetCollidiongObject(Collider col)
    {
        if(collidingObject|| !col.GetComponent<Rigidbody>()){
            return;
        }
        collidingObject = col.gameObject;
    }
    // Update is called once per frame
    void Update()
    {
        //if (GetTeleportDown())
        //{
        //    print("Teleport" + handType);
        //}
        //if (GetGrab())
        //{
        //    print("Grab" + handType);
        //}
        if (grabAction.GetLastStateDown(handType))
        {
            if (collidingObject.tag == "Grab")
            {
                print("i collided with "+collidingObject.name);
                GrabObject();
            }
        }
        if (grabAction.GetLastStateUp(handType))
        {
            if (objectinhand)
            {
                ReleaseObject();
            }
        }
        if (teleportAction.GetLastState(handType))
        {
            if(collidingObject.tag == "Button")
            {
                //SceneChange();
            }
        }
        if (teleportAction.GetStateUp(handType))
        {
            if (collidingObject.tag == "Spawn")
            {
                //SpawnBall();
            }
        }
    }
    public bool GetTeleportDown()
    {
        return teleportAction.GetStateDown(handType);
    }
    public bool GetGrab()
    {
        return grabAction.GetState(handType);
    }
    public void OnTriggerEnter(Collider other)
    {
        SetCollidiongObject(other);
    }
    public void OnTriggerStay(Collider other)
    {
        SetCollidiongObject(other);
    }
    public void OnTriggerExit(Collider other)
    {   
        if(!collidingObject){
            return;
        }
        collidingObject = null;
    }
    private void GrabObject()
    {
        objectinhand = collidingObject;
        collidingObject = null;
        var joint = AddFixedJoint();
        joint.connectedBody = objectinhand.GetComponent<Rigidbody>();
    }
    private FixedJoint AddFixedJoint()
    {
        FixedJoint fx = gameObject.AddComponent<FixedJoint>();
        fx.breakForce = 20000;
        fx.breakTorque = 20000;
        return fx;
    }
    private void ReleaseObject()
    {
        if (GetComponent<FixedJoint>())
        {
            GetComponent<FixedJoint>().connectedBody = null;
            Destroy(GetComponent<FixedJoint>());
            //objectinhand.GetComponent<Rigidbody>().velocity = controllerPose.GetVelocity();
            //objectinhand.GetComponent<Rigidbody>().angularVelocity = controllerPose.GetAngularVelocity();

        }
        objectinhand = null;
    }


}
