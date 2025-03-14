﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.EventSystems;
using Misc;
using System.Threading.Tasks;

public class ReplayItemRenderer : DynamicInfinityItem
{
    private readonly string _timeFormatPatten = "yyyy/MM/dd HH:mm:ss";
    public class ReplayItemData
    {
       
        public string ReplayFileFullPath;
        public bool IsSelect;

        private byte[] replayBinaryBytes;

        private Src.Replays.ReplayInfo replayInfo;

        public async Task<Src.Replays.ReplayInfo> GetReplayInfoAsync()
        {
            if(replayInfo == null)
            {
                if (replayBinaryBytes == null)
                    replayBinaryBytes = await System.Threading.Tasks.Task.Run(()=>File.ReadAllBytes(ReplayFileFullPath));
                replayInfo = await Src.Replays.ReplayInfo.Read(replayBinaryBytes);
            }
            return replayInfo;
        }
        public FileInfo RepFileInfo;
        public ReplayItemData(string fullPath)
        {
            ReplayFileFullPath = fullPath;
        }
        public string GetFileNameWithoutExtension()
        {
            return System.IO.Path.GetFileNameWithoutExtension(ReplayFileFullPath); 
        }
    }
    public Image m_ImgBg;
    public Text m_TxtReplayName;
    public Text m_TxtReplayCreationDate;

    // Start is called before the first frame update
    void Start()
    {
        EventTriggerListener.Get(m_ImgBg.gameObject).onClick += g => {
            if (OnEventHandler != null)
                OnEventHandler(new Event("Select", GetData()));
        };
    }
    protected override void OnRenderer()
    {
        ReplayItemData replayData = GetData<ReplayItemData>();
        m_ImgBg.color = replayData.IsSelect?new UnityEngine.Color(100/255f,200/255f,1): UnityEngine.Color.white;
        m_TxtReplayName.text = replayData.GetFileNameWithoutExtension();
        if(replayData.RepFileInfo==null)
            replayData.RepFileInfo = new FileInfo(replayData.ReplayFileFullPath);
        m_TxtReplayCreationDate.text = replayData.RepFileInfo.LastWriteTime.ToString(_timeFormatPatten);
    }
}
