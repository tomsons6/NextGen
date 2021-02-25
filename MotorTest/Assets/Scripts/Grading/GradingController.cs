﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ModelOutline;
using Outline = ModelOutline.Outline;

public class GradingController : MonoBehaviour
{
    [SerializeField]
    ControllerScript m_ControllerScript;

    [SerializeField]
    Canvas m_ControllerUI;

    [SerializeField]
    Canvas m_ResultUI;

    [SerializeField]
    GameObject m_ListContent;
    Button m_Refresh;

    [SerializeField]
    GameObject m_ResultRow;

    Object[] m_Thumbnails;

    public List<PlacedOrder> PlacedOrder;

    [SerializeField]
    public Results m_CurrPickUpObjRes;

    public List<Results> m_Results;

    [SerializeField]
    bool MethodExecuted;

    public int PlacedOrderID;

    bool ShowOutlineHint;

    private static GradingController _instance;
    public static GradingController Instance { get { return _instance; } }


    //---Total Timer---
    float time;
    float sec;
    float min;

    Text m_TotalTimerText;
    private void Awake()
    {
        m_ControllerScript = FindObjectOfType<ControllerScript>();
        m_TotalTimerText = m_ControllerUI.GetComponentInChildren<Text>();
        m_Refresh = m_ResultUI.GetComponentInChildren<Button>();
        m_Refresh.onClick.AddListener(DisplayList);
        StartCoroutine(TotalTimer());
        m_Results = new List<Results>();

        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }
    private void Update()
    {
        if (m_ControllerScript != null)
        {
            if (m_ControllerScript.objectinhand != null)
            {
                if (m_CurrPickUpObjRes == null)
                {
                    m_CurrPickUpObjRes = m_Results.Find(x => x.partName == m_ControllerScript.objectinhand.name);
                }
                if (!MethodExecuted)
                {
                    AddResToList(m_ControllerScript.objectinhand);
                    TimerCountControl(m_ControllerScript.objectinhand);
                }
            }
            else
            {
                if (m_CurrPickUpObjRes != null)
                {
                    m_CurrPickUpObjRes.isPaused = true;
                }
                m_CurrPickUpObjRes = null;
                MethodExecuted = false;
            }
        }
    }
    public void AddResToList(GameObject go)
    {
        if (MethodExecuted != true)
        {
            MethodExecuted = true;
        }
        if (m_Results.Count == 0)
        {
            Results TempRes = new Results
            {
                partName = go.name,
                PickUpCount = 0,
                isPaused = false,
                CorrectPlacementOrder = go.GetComponent<Placeable>().m_ID,
                ActualPlacementID = 0
            };
            m_Results.Add(TempRes);
        }
        else
        {
            bool Contains = m_Results.Any(x => x.partName == go.name);

            if (Contains)
            {
                m_CurrPickUpObjRes.isPaused = false;
            }
            else
            {
                Results TempRes = new Results
                {
                    partName = go.name,
                    PickUpCount = 0,
                    isPaused = false,
                    CorrectPlacementOrder = go.GetComponent<Placeable>().m_ID,
                    ActualPlacementID = 0
                };
                m_Results.Add(TempRes);
                return;
            }
        }
    }

    void TimerCountControl(GameObject go)
    {
        if (MethodExecuted != true)
        {
            MethodExecuted = true;
        }
        if (m_Results.Count >= 1)
        {
            bool Contains = m_Results.Any(x => x.partName == go.name);
            Results TempRes = m_Results.Find(x => x.partName == go.name);

            if (Contains)
            {
                TempRes.PickUpCount++;
                TempRes.StartTimer(this);
                TempRes.isPaused = false;
            }
            else
            {
                TempRes.StopTimer(this);
                TempRes.isPaused = true;
            }
            TempRes = null;
        }
    }
    public void DisplayList()
    {
        List<Results> SortedList = new List<Results>();
        if (FindObjectsOfType<ListEntryValues>() != null)
        {
            ListEntryValues[] TempListItems = FindObjectsOfType<ListEntryValues>();
            for (int i = 0; i < TempListItems.Length; i++)
            {
                Destroy(TempListItems[i].gameObject);
            }
        }
        m_Thumbnails = Resources.LoadAll("Thumbnails", typeof(Sprite));

        PlacedOrderIdUpdate();

        if (m_Results != null)
        {
            SortedList = m_Results.OrderBy(x => x.CorrectPlacementOrder).ToList();
            foreach (Results res in SortedList)
            {
                if (m_Thumbnails != null)
                {
                    foreach (Sprite img in m_Thumbnails)
                    {
                        if (img.name == res.partName + "_Thumbnail")
                        {
                            m_ResultRow.GetComponent<ListEntryValues>().UpdateTextsAndImage(img, res.partName, res.PickUpCount.ToString(), res.PartPickTime.ToString(), res.CorrectPlacementOrder.ToString(), res.ActualPlacementID.ToString());
                        }
                    }
                }
                else
                {
                    m_ResultRow.GetComponent<ListEntryValues>().UpdateTexts(res.partName, res.PickUpCount.ToString(), res.PartPickTime.ToString(), res.CorrectPlacementOrder.ToString(), res.ActualPlacementID.ToString());
                }
                Instantiate(m_ResultRow, m_ListContent.transform);
            }
            SortedList.Clear();
        }
    }
    public void PlacedOrderIdUpdate()
    {
        for (int i = 0; i < m_Results.Count; i++)
        {
            for (int j = 0; j < PlacedOrder.Count; j++)
            {
                if (m_Results[i].partName == PlacedOrder[j].Partname)
                {
                    m_Results[i].ActualPlacementID = PlacedOrder[j].PlacedId;
                }
            }
        }
    }

    public void ObjectPlaced(GameObject Go, int ObjID)
    {
        Debug.Log("izpildaas kkad");
        if (Instance.PlacedOrder.Count == 0)
        {
            Instance.PlacedOrder.Add(new PlacedOrder { Partname = Go.name, PlacedId = 0, ObjID = ObjID });
            Instance.PlacedOrderID = 1;
        }
        else
        {
            for (int i = 0; i < Instance.PlacedOrder.Count; i++)
            {
                if (!Instance.PlacedOrder.Any(x => x.Partname == Go.name))
                {
                    if (Instance.PlacedOrder.Any(x => x.ObjID == ObjID))
                    {
                        Debug.Log("same id ");
                        int TempInt = Instance.PlacedOrder.Find(x => x.ObjID == ObjID).PlacedId;
                        Instance.PlacedOrder.Add(new PlacedOrder { Partname = Go.name, PlacedId = TempInt, ObjID = ObjID });
                        return;
                    }
                    else
                    {
                        Instance.PlacedOrder.Add(new PlacedOrder { Partname = Go.name, PlacedId = Instance.PlacedOrderID, ObjID = ObjID });
                        Instance.PlacedOrderID++;
                    }
                }
            }
        }
    }
    public void ShowHintOutline(GameObject GO, int ObjID)
    {
        StartCoroutine(HintOutlineCorutine(GO, ObjID));
    }
    IEnumerator HintOutlineCorutine(GameObject GO,int ObjID)
    {

        int PlacedID = Instance.PlacedOrder.Find(x => x.Partname == GO.name).PlacedId;
        if (GO.GetComponent<Outline>() != null)
        {

                if (GO.GetComponent<Outline>().enabled != true)
                {
                    if (PlacedID == ObjID)
                    {
                        GO.GetComponent<Outline>().OutlineWidth = 4f;
                        GO.GetComponent<Outline>().OutlineColor = Color.green;
                        GO.GetComponent<Outline>().enabled = true;
                    }
                    else
                    {
                        GO.GetComponent<Outline>().OutlineWidth = 4f;
                        GO.GetComponent<Outline>().OutlineColor = Color.red;
                        GO.GetComponent<Outline>().enabled = true;
                    }
                }
            yield return new WaitForSeconds(1f);
                GO.GetComponent<Outline>().enabled = false;
                GO.GetComponent<Outline>().OutlineWidth = 2f;
                GO.GetComponent<Outline>().OutlineColor = Color.white;
            
        }
        yield return null;
    }
    public void ObjectRemoved(GameObject Go)
    {
        for (int i = 0; i < Instance.PlacedOrder.Count; i++)
        {
            if (Instance.PlacedOrder[i].Partname == Go.name)
            {
                Instance.m_Results.Find(x => x.partName == Go.name).ActualPlacementID = 0;
                Instance.PlacedOrder.Remove(Instance.PlacedOrder[i]);
                if (Instance.PlacedOrderID > 0)
                {
                    Instance.PlacedOrderID--;
                }
            }
        }
    }
    public IEnumerator TotalTimer()
    {

        while (true)
        {
            time += Time.deltaTime;
            sec = (int)(time % 60);
            min = (int)(time / 60);

            m_TotalTimerText.text = "Time elapsed - " + string.Format("{0:00}:{1:00}", min, sec);

            yield return m_TotalTimerText;
        }
    }

}
[System.Serializable]
public class PlacedOrder
{
    public string Partname;
    public int PlacedId;
    public int ObjID;

}
