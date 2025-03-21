﻿using UnityEngine;
using System.Collections;
using Managers.UI;
using Localization;
using System.Collections.Generic;
using UnityEngine.UI;
using NetServiceImpl.OnlineMode.Gate;
using Misc;
using Net.Pt;
using NetServiceImpl;
using Proxy;
using Managers;

public class GatePanel : UIView, ILanguageApplicable
{
    private List<GateAddressVO> m_Hosts;
    public Button m_BtnBack;
    public Text m_TxtTitle;
    public ToggleGroup m_ToggleGroup;
    public List<Toggle> m_Toggles;
    public DynamicInfinityListRenderer m_DynRoomList;
    public Button m_BtnRefreshGate;
    public Button m_BtnRefreshRoom;
    public Button m_BtnJoin;
    public Button m_BtnCreate;
    private Toggle currentSelectedToggle;
    public override void OnInit()
    {
        base.OnInit();

        foreach (Toggle toggle in m_Toggles)
        {
            toggle.isOn = false;
            toggle.onValueChanged.AddListener((select) => OnToggleSelect(toggle, select));
            toggle.gameObject.SetActive(false);
        }
        Evt.EventMgr<EvtGate, List<GateAddressVO>>.AddListener(EvtGate.UpdateWANGateServerList, OnWANGateServerList);
        Evt.EventMgr<EvtGate, List<GateAddressVO>>.AddListener(EvtGate.UpdateLANGateServerList, OnLANGateServerList);
        Evt.EventMgr<EvtGate, object>.AddListener(EvtGate.GateServerConnected, OnGateServerConnected);
        Evt.EventMgr<EvtGate, PtRoomList>.AddListener(EvtGate.UpdateRoomList, OnUpdateRoomList);

        Evt.EventMgr<EvtGate, object>.AddListener(EvtGate.OpenRoomPanel, OnOpenRoomPanel);
        m_BtnBack.onClick.AddListener(() =>
        {
            ModuleManager.GetModule<UIModule>().Pop(Layer.Bottom);
            foreach (Toggle toggle in m_Toggles)
            {
                toggle.isOn = false;
                toggle.gameObject.SetActive(false);
            }
            currentSelectedToggle = null;
            GameClientNetwork.Instance.CloseClient();
        });

        m_BtnRefreshGate.onClick.AddListener(OnClickRefreshGate);
        m_BtnRefreshRoom.onClick.AddListener(OnClickRefreshRoom);
        m_BtnCreate.onClick.AddListener(OnClickCreate);
        m_DynRoomList.InitRendererList(OnSelectDynRoomListItem, null);
    }
    IEnumerator _RefreshGateInfo()
    {
        while (true)
        {
            OnClickRefreshRoom();
            yield return new WaitForSeconds(1);
        }
    }
    void OnEnable()
    {
        StartCoroutine(_RefreshGateInfo());
    }
    void OnDisable()
    {
        StopCoroutine(_RefreshGateInfo());
    }
    public override void OnResume()
    {
        base.OnResume();
    }
    public override void OnClose()
    {
        base.OnClose();
        m_BtnRefreshGate.onClick.RemoveAllListeners();
        m_BtnRefreshRoom.onClick.RemoveAllListeners();
        m_BtnBack.onClick.RemoveAllListeners();
        m_BtnCreate.onClick.RemoveAllListeners();
        foreach (Toggle toggle in m_Toggles)
        {
            toggle.onValueChanged.RemoveAllListeners();// ((select) => OnToggleSelect(toggle, select));            
        }
        Evt.EventMgr<EvtGate, List<GateAddressVO>>.RemoveListener(EvtGate.UpdateWANGateServerList, OnWANGateServerList);
        Evt.EventMgr<EvtGate, List<GateAddressVO>>.RemoveListener(EvtGate.UpdateLANGateServerList, OnLANGateServerList);
        Evt.EventMgr<EvtGate, object>.RemoveListener(EvtGate.GateServerConnected, OnGateServerConnected);
        Evt.EventMgr<EvtGate, PtRoomList>.RemoveListener(EvtGate.UpdateRoomList, OnUpdateRoomList);

        Evt.EventMgr<EvtGate, object>.RemoveListener(EvtGate.OpenRoomPanel, OnOpenRoomPanel);
    }
    void OnClickCreate()
    {
        //AllUI.Instance.Show("PanelRoomPage", new RoomPageVO(pageVO.PageType));
        ModuleManager.GetModule<UIModule>().Push(UITypes.RoomPanel, Layer.Bottom, null);
        ClientService.Get<GateService>().RequestCreateRoom(1);
    }
    void OnOpenRoomPanel(object obj)
    {
        ModuleManager.GetModule<UIModule>().Push(UITypes.RoomPanel, Layer.Bottom, null);
    }

    /// <summary>
    /// 房间加入按钮的事件
    /// </summary>
    /// <param name="selectedItem"></param>
    void OnSelectDynRoomListItem(DynamicInfinityItem selectedItem)
    {
        var ptRoom = selectedItem.GetData<PtRoom>();
        string selfName = DataProxy.Get<UserDataProxy>().GetUserName();
        if (ptRoom.Players.Exists(p => p.NickName == selfName))
        {
            //以断开 重新进入的方式加入房间，会以不同的方式进入游戏
            DialogBox.Show(Language.GetText(22),Language.GetText(40), DialogBox.SelectType.All, selection =>
            {
                if (selection == DialogBox.SelectType.Confirm)
                {
                    ClientService.Get<GateService>().RequestJoinRoom(ptRoom);
                }
            });
        }
        else
        {
            //在组队时刻加入房间
            ClientService.Get<GateService>().RequestJoinRoom(ptRoom);
        }

    }
    #region NetMessageHandler
    void OnGateServerConnected(object obj)
    {
        print("connected gate");
    }
    void OnUpdateRoomList(PtRoomList roomList)
    {
        m_DynRoomList.SetDataProvider(roomList.Rooms);
    }
    void OnWANGateServerList(List<GateAddressVO> hosts)
    {
        UpdateServerList(hosts);
    }
    void OnLANGateServerList(List<GateAddressVO> hosts)
    {
        UpdateServerList(hosts);
    }
    #endregion
    void UpdateServerList(List<GateAddressVO> hosts)
    {
        m_Hosts = hosts;
        for (int i = 0; i < m_Toggles.Count; ++i)
        {
            if (i < m_Hosts.Count)
            {
                m_Toggles[i].gameObject.SetActive(true);
                m_Toggles[i].SetToggleText(m_Hosts[i].ConnectKey);
            }
            else
            {
                m_Toggles[i].gameObject.SetActive(false);
            }
        }
        if (m_Hosts.Count > 0)
        {
            m_Toggles[0].isOn = false;
            m_Toggles[0].isOn = true;
            //OnClickRefreshGate();
        }

    }

    void ConnectToGateServer()
    {
        Debug.LogWarning("ConnectTo GateServer");
        ClientService.Get<GateService>().Connect2GateServer( );
    }
    void OnClickRefreshGate()
    {
        ClientService.Get<GateService>().RefreshLanGates(9030);
    }
    void OnClickRefreshRoom()
    {
        ClientService.Get<GateService>().RequestRoomList();
    }
    void OnToggleSelect(Toggle toggle, bool select)
    {
        Debug.LogWarning("OnToggleSelect "+select);
        if (select && currentSelectedToggle != toggle)
        {
            currentSelectedToggle = toggle;
            toggle.SetToggleTextColor(Color.black);
            foreach (var t in m_Toggles)
            {
                if (t != toggle)
                {
                    t.SetToggleTextColor(Color.white);
                }
            }
            int selectIndex = m_Toggles.IndexOf(toggle);
            ClientService.Get<GateService>().UpdateCurrentGateAddress( m_Hosts[selectIndex]);
            ConnectToGateServer();
        }
    }
    public override void OnShow(object paramObject)
    {
        base.OnShow(paramObject);
        ApplyLocalizedLanguage();
            //wan mode
        GateAddressVO gateAddressVO = DataProxy.Get<UserDataProxy>().GetGateAddressVO();
        if (gateAddressVO != null)
        {
            Evt.EventMgr<EvtGate, List<GateAddressVO>>.TriggerEvent(
               EvtGate.UpdateWANGateServerList, new List<GateAddressVO>() { gateAddressVO });
        }
        
    }


    // Update is called once per frame
    void Update()
    {

    }

    public void ApplyLocalizedLanguage()
    {
        m_BtnBack.SetButtonText(Language.GetText(5));
        m_BtnCreate.SetButtonText(Language.GetText(8));
        m_BtnRefreshGate.SetButtonText(Language.GetText(6));
        m_BtnRefreshRoom.SetButtonText(Language.GetText(6));
        m_BtnJoin.SetButtonText(Language.GetText(10));
        m_TxtTitle.text = Language.GetText(1);
    }

}
