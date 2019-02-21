using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using UnityEngine;

public class Twin2ndSceneScript : MonoBehaviour
{
    private static void SetGuiColorAlpha(float alpha)
    {
        var color = GUI.color;
        color.a = alpha;
        GUI.color = color;
    }

    public GUISkin guiSkin;
    public TwinViewScript viewCam;
    public string boneName = "Hips";
    public Rect camGuiBodyRect = new Rect(870, 25, 93, 420);
    public Rect modelBodyRect = new Rect(20, 40, 300, 500);
    public Rect textGuiBodyRect = new Rect(20, 510, 300, 70);
    public Rect sliderGuiBodyRect = new Rect(770, 520, 170, 150);
    public Rect sliderTextBodyRect = new Rect(770, 520, 170, 150);

    private string segmentCode = "_B";

    private string CharacterCode = "M01/";
    private string FBXListFile = "fbx_list";
    private string AnimationListFile = "animation_list";
    private string AnimationListFileAll = "animation_list_all";

    private string FbxCtrlFile = "fbx_ctrl";

    private string ParticleListFile = "ParticleList";
    private string ParticleAnimationListFile = "ParticleAnimationList";
    private string FacialTexListFile = "facial_texture_list";
    private string facialMatName = "succubus_a_face";

    private float curParticle = 1;
    private string curCharacterName = "";
    //string TitleTextFile = "TitleText";
    public bool guiOn = true;
    private float initPosX = -0.3f;
    private bool autoResourceMode = true;

    private string settingFileDir = "Taichi/TwinViewer Settings/";

    private int curBG = 0;
    private int curAnim = 0;
    private int curModel = 0;
    private int curCharacter = 0;
    private int curFacial = 1;

    private bool animReplay = true;
    private bool playOnceFlg = true;

    private string resourcesPathFull = "Assets/Taichi Character Pack/Resources/Taichi";
    private string resourcesPath = "";

    private float animSpeed = 1;
    private float motionDelay = 0;

    private int curLOD = 0;
    private string curModelName = "";
    private string curAnimName = "";
    private string curBgName = "";
    private string curFacialName = "";
    private string curParticleName = "";

    private int facialCount = 0;
    private float positionY = 0;
    private string animationPath;

    private string[] animationList;
    private string[] animationListAll;
    private string[] animationNameList;
    private string[] modelList;
    private string[] modelNameList;
    private string[] facialTexList;
    private string[] particleAnimationList;
    private string[] particleList;

    private float animSpeedSet;
    private string[] backGroundList;
    private string[] stageTexList;
    private string[] lodList = { "_h", "_m", "_l" };
    private string[] lodTextList = { "Hi", "Mid", "Low" };
    private string[] modeTextList = { "AddPerticle", "Original" };
    private GameObject obj;
    private GameObject loaded;
    private SkinnedMeshRenderer SM;
    private SkinnedMeshRenderer faceSM;
    private string faceObjName;

    private TextAsset txt;

    private bool CamModeRote = true;
    private bool CamModeMove = false;
    private bool CamModeZoom = false;
    private bool CamModeFix = true;
    private int CamMode = 0;

    //private string titleText = "";
    private XmlDocument xDoc;
    private XmlNodeList xNodeList;

    private float nowTime = 0;
    private bool playFlg = true;

    private Material faceMat_L;
    private Material faceMat_M;

    private GameObject BGObject;
    private GameObject BGEff;
    private GameObject BGPlane;

    private Dictionary<string, bool> functionList = new Dictionary<string, bool>();
    private GameObject planeObj;

    void Start()
    {
        viewCam = GameObject.Find("Main Camera").GetComponent<TwinViewScript>();
        nowTime += Time.deltaTime;

        //	txt = Resources.Load(settingFileDir+"BackGroundList", TextAsset);
        //	backGroundList =txt.text.Split(["\r","\n"],1);

        //	txt = Resources.Load(settingFileDir+"StageTexList", TextAsset);
        //	stageTexList =txt.text.Split(["\r","\n"],1);

        this.SetSettings(0);
        //	this.SetInitBackGround();
        this.SetInitModel();
        this.SetInitMotion();
        this.SetAnimationSpeed(animSpeed);

        var pos = obj.transform.position;
        pos.x = initPosX;
        obj.transform.position = pos;
    }

    private bool animationPlayFlgOld;
    void Update()
    {

        if (
            !obj.GetComponent<Animation>().IsPlaying(curAnimName) &&
            animationPlayFlgOld
        )
        {
            nowTime = 0;
            playFlg = true;
        }

        animationPlayFlgOld = obj.GetComponent<Animation>().IsPlaying(curAnimName);

        nowTime += Time.deltaTime;
        if (nowTime > motionDelay)
        {
            SetAnimationSpeed(animSpeedSet);
            this.playAnimation();
        }
        else
        {
            SetAnimationSpeed(0);
            this.playAnimation();
        }
        if (Input.GetKeyDown("1")) SetNextModel(-1);
        if (Input.GetKeyDown("2")) SetNextModel(-1);

        if (Input.GetKeyDown("q")) SetNextMotion(-1);
        if (Input.GetKeyDown("w")) SetNextMotion(1);

        if (Input.GetKeyDown("a")) SetNextBackGround(-1);
        if (Input.GetKeyDown("s")) SetNextBackGround(1);

        if (Input.GetKeyDown("z")) SetNextLOD(-1);
        if (Input.GetKeyDown("x")) SetNextLOD(1);
    }
    private int onSliderFlg;
    void scrollBarPos()
    {
        Vector2[] minPos;
        Vector2[] maxPos;
        int popupCount = 10;

        onSliderFlg = 0;

        minPos = new Vector2[popupCount];
        maxPos = new Vector2[popupCount];

        var maxX = Screen.width - 10;
        var minX = Screen.width - (sliderGuiBodyRect.width + sliderTextBodyRect.width + 10);

        //Right Slider

        minPos[0] = new Vector2(minX, 267);
        maxPos[0] = new Vector2(maxX, 295);

        minPos[1] = new Vector2(minX, 235);
        maxPos[1] = new Vector2(maxX, 267);

        minPos[2] = new Vector2(minX, 205);
        maxPos[2] = new Vector2(maxX, 235);

        minPos[3] = new Vector2(minX, 175);
        maxPos[3] = new Vector2(maxX, 205);

        minPos[4] = new Vector2(minX, 130);
        maxPos[4] = new Vector2(maxX, 175);

        minPos[5] = new Vector2(minX, 85);
        maxPos[5] = new Vector2(maxX, 130);
        for (int i = 0; i < popupCount; i++)
        {
            if (
                Input.mousePosition.x > minPos[i].x &&
                Input.mousePosition.x < maxPos[i].x &&
                Input.mousePosition.y > minPos[i].y &&
                Input.mousePosition.y < maxPos[i].y
            )
            {
                onSliderFlg = i + 1;
            }
        }

    }

    private Vector3 scale;
    void OnGUI()
    {

        if (guiSkin)
        {
            GUI.skin = guiSkin;
        }

        if (!guiOn) return;
        scale.x = 1;
        scale.y = 1;
        scale.z = 1.0f;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);

        float sw = Screen.width;
        float sh = Screen.height;

        //GUI Parts Adjust
        textGuiBodyRect.y = sh - (textGuiBodyRect.height + 10);
        textGuiBodyRect.x = sw - (textGuiBodyRect.width + 10);
        sliderGuiBodyRect.y = sh - (sliderGuiBodyRect.height + textGuiBodyRect.height + 28);
        sliderTextBodyRect.y = sh - (sliderTextBodyRect.height + textGuiBodyRect.height + 15);

        sliderGuiBodyRect.x = sw - (sliderGuiBodyRect.width + sliderTextBodyRect.width + 15);
        sliderTextBodyRect.x = sw - (sliderTextBodyRect.width + 10);

        modelBodyRect.x = sw - (modelBodyRect.width + 10);
        this.scrollBarPos();

        float buttonSpace = 8;

        GUILayout.BeginArea(modelBodyRect);
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("", "Left")) SetNextCharacter(-1);
        GUILayout.Label("", "Chara");
        if (GUILayout.Button("", "Right")) SetNextCharacter(1);
        GUILayout.EndHorizontal();
        GUILayout.Space(buttonSpace);

        if (functionList["model"])
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("", "Left")) SetNextModel(-1);
            GUILayout.Label("", "Costume");
            if (GUILayout.Button("", "Right")) SetNextModel(1);
            GUILayout.EndHorizontal();
            GUILayout.Space(buttonSpace);
        }
        else
        {
            SetGuiColorAlpha(0.5f);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("", "LeftGrayOut")) { }
            GUILayout.Label("", "Costume");
            if (GUILayout.Button("", "RightGrayOut")) { }
            GUILayout.EndHorizontal();
            GUILayout.Space(buttonSpace);
            SetGuiColorAlpha(1.0f);
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("", "Left")) SetNextMotion(-1);

        var animReplayVal = GUILayout.Toggle(animReplay, "", "AnimReplay");
        if (animReplay != animReplayVal)
        {
            animReplay = animReplayVal;
        }
        if (animReplay)
        {
            playOnceFlg = true;
        }
        if (GUILayout.Button("", "Right")) SetNextMotion(1);
        GUILayout.EndHorizontal();
        GUILayout.Space(buttonSpace);

        if (functionList["facial"])
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("", "Left")) SetNextFacial(-1);
            GUILayout.Label("", "Facial");
            if (GUILayout.Button("", "Right")) SetNextFacial(1);
            GUILayout.EndHorizontal();
            GUILayout.Space(buttonSpace);
        }
        else
        {
            SetGuiColorAlpha(0.5f);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("", "LeftGrayOut")) { }
            GUILayout.Label("", "Facial");
            if (GUILayout.Button("", "RightGrayOut")) { }
            GUILayout.EndHorizontal();
            GUILayout.Space(buttonSpace);
            SetGuiColorAlpha(1.0f);
        }

        if (functionList["lod"])
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("", "Left")) SetNextLOD(-1);
            GUILayout.Label("", "LOD");
            if (GUILayout.Button("", "Right")) SetNextLOD(1);
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        }
        else
        {
            SetGuiColorAlpha(0.5f);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("", "LeftGrayOut")) { }
            GUILayout.Label("", "LOD");
            if (GUILayout.Button("", "RightGrayOut")) { }
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            SetGuiColorAlpha(1.0f);
        }
        GUILayout.EndVertical();
        GUILayout.EndArea();
        //Slider Text

        GUILayout.BeginArea(sliderTextBodyRect);
        GUILayout.FlexibleSpace();

        string positionXText = "Position X : " + String.Format("{0:F1}", obj.transform.position.x);
        GUILayout.Box(positionXText);
        GUILayout.FlexibleSpace();

        string positionYText = "Position Y : " + String.Format("{0:F1}", obj.transform.position.y);
        GUILayout.Box(positionYText);
        GUILayout.FlexibleSpace();

        string positionZText = "Position Z : " + String.Format("{0:F1}", obj.transform.position.z);
        GUILayout.Box(positionZText);
        GUILayout.FlexibleSpace();

        string rotateText = "Rotate : " + String.Format("{0:F1}", obj.transform.eulerAngles.y);
        GUILayout.Box(rotateText);
        GUILayout.FlexibleSpace();

        string animSpeedText = "Animation\nSpeed : " + String.Format("{0:F1}", animSpeed);
        GUILayout.Box(animSpeedText);
        GUILayout.FlexibleSpace();

        string motionTimingText = "Motion\nDelay : " + String.Format("{0:F1}", motionDelay);
        GUILayout.Box(motionTimingText);
        GUILayout.FlexibleSpace();

        GUILayout.EndArea();
        //Slider GUI
        GUILayout.BeginArea(sliderGuiBodyRect);
        if (onSliderFlg == 1)
        {
            SetGuiColorAlpha(1.0f);
        }
        else
        {
            SetGuiColorAlpha(0.4f);
        }
        //Position X
        float posXVal = GUILayout.HorizontalSlider(obj.transform.position.x, 1, -1);
        if (obj.transform.position.x != posXVal)
        {
            var pos = obj.transform.position;
            pos.x = posXVal;
            obj.transform.position = pos;
            viewCam.MouseLock(true);
        }
        else
        {
            viewCam.MouseLock(false);
        }
        GUILayout.Space(0);

        //Position Y
        if (onSliderFlg == 2)
        {
            SetGuiColorAlpha(1.0f);
        }
        else
        {
            SetGuiColorAlpha(0.4f);
        }
        float posYVal = GUILayout.HorizontalSlider(obj.transform.position.y, 0, 3);
        if (obj.transform.position.y != posYVal)
        {
            var pos = obj.transform.position;
            pos.y = posYVal;
            obj.transform.position = pos;
            viewCam.MouseLock(true);
        }
        else
        {
            viewCam.MouseLock(false);
        }
        GUILayout.Space(0);

        //Position Z
        if (onSliderFlg == 3)
        {
            SetGuiColorAlpha(1.0f);
        }
        else
        {
            SetGuiColorAlpha(0.4f);
        }
        float posZVal = GUILayout.HorizontalSlider(obj.transform.position.z, 1, -1);
        if (obj.transform.position.z != posZVal)
        {
            var pos = obj.transform.position;
            pos.z = posZVal;
            obj.transform.position = pos;
            viewCam.MouseLock(true);
        }
        else
        {
            viewCam.MouseLock(false);
        }
        GUILayout.Space(-3);

        //Rotate Y
        if (onSliderFlg == 4)
        {
            SetGuiColorAlpha(1.0f);
        }
        else
        {
            SetGuiColorAlpha(0.4f);
        }
        float rotVal = GUILayout.HorizontalSlider(obj.transform.eulerAngles.y, 0, 359.9f);
        if (obj.transform.eulerAngles.y != rotVal)
        {
            var eulerAngles = obj.transform.eulerAngles;
            eulerAngles.y = rotVal;
            obj.transform.eulerAngles = eulerAngles;
            viewCam.MouseLock(true);
        }
        else
        {
            viewCam.MouseLock(false);
        }
        GUILayout.Space(6);

        if (onSliderFlg == 5)
        {
            SetGuiColorAlpha(1.0f);
        }
        else
        {
            SetGuiColorAlpha(0.4f);
        }
        //Motion Speed
        animSpeedSet = GUILayout.HorizontalSlider(animSpeed, 0, 2);
        if (animSpeed != animSpeedSet)
        {
            animSpeed = animSpeedSet;
            SetAnimationSpeed(animSpeed);
            viewCam.MouseLock(true);
        }
        else
        {
            viewCam.MouseLock(false);
        }
        GUILayout.Space(13);

        if (onSliderFlg == 6)
        {
            SetGuiColorAlpha(1.0f);
        }
        else
        {
            SetGuiColorAlpha(0.4f);
        }
        //Motion timing
        float delayVal = GUILayout.HorizontalSlider(motionDelay, 0, 5);
        if (motionDelay != delayVal)
        {
            motionDelay = delayVal;
            viewCam.MouseLock(true);
        }
        else
        {
            viewCam.MouseLock(false);
        }
        GUILayout.EndArea();
        SetGuiColorAlpha(1.0f);

        string text = "";
        text += "Character : " + curCharacterName + "\n";
        if (functionList["model"]) text += "Costume : " + (curModel + 1) + " / " + modelList.Length + " : " + curModelName + "\n";
        text += "Animation : " + (curAnim + 1) + " / " + (animationList.Length) + " : " + curAnimName + "\n";
        if (functionList["facial"]) text += "Facial : " + (curFacial + 1) + " / " + (facialCount) + " : " + curFacialName + "\n";
        //text += "BackGround : " + (curBG+1)+" / " +backGroundList.length + " : " + curBgName + "\n";
        if (functionList["lod"]) text += "Quality : " + lodTextList[curLOD] + "\n";
        GUI.Box(textGuiBodyRect, text);
    }

    public void SetInit()
    {
        SetInitModel();
        SetInitMotion();
        SetAnimationSpeed(animSpeed);
        if (functionList["facial"])
        {
            SetInitFacial();
        }
    }

    public void timerReset()
    {
        nowTime = 0;
        obj.GetComponent<Animation>().Stop();
        playOnceFlg = true;

    }

    public void sliderReset()
    {
        var eulerAngles = obj.transform.eulerAngles;
        eulerAngles.y = 0.0f;
        obj.transform.position = new Vector3(-0.3f, 0.0f, 0.0f);
        animSpeed = 1;
        motionDelay = 0;
        SetAnimationSpeed(animSpeed);
    }

    public void SetSettings(int _i)
    {
        string fbxSetting;
        string animSetting;
        string fbxCtrlSetting = string.Empty;
        curAnim = 0;
        curModel = 0;
        curLOD = 0;

        switch (_i)
        {
            // Taichi Hayami
            case 0:
                resourcesPathFull = "Assets/Taichi Character Pack/Resources/Taichi";
                resourcesPath = "Taichi/";
                animationPath = "Taichi/Animations Legacy/m01@";
                CharacterCode = "M01/";
                AnimationListFile = "animation_list";
                AnimationListFileAll = "animation_list";

                fbxCtrlSetting = "Taichi/TwinViewer Settings/" + CharacterCode + "fbx_ctrl";
                faceMat_L = Resources.Load<Material>("Taichi/Materials/m01_face_00_l");
                faceMat_M = Resources.Load<Material>("Taichi/Materials/m01_face_00_m");
                functionList["model"] = true;
                functionList["facial"] = false;
                functionList["lod"] = true;

                curCharacterName = "Taichi Hayami";
                break;

            // puppet
            case 1:
                resourcesPathFull = "Assets/Taichi Character Pack/Resources/Puppet";
                resourcesPath = "Puppet/";
                animationPath = "Taichi/Animations Legacy/m01@";
                AnimationListFile = "animation_list";
                AnimationListFileAll = "animation_list";

                CharacterCode = "Puppet/";

                //animTest          = animTestM01;
                fbxCtrlSetting = settingFileDir + CharacterCode + "fbx_ctrl";
                functionList["model"] = false;
                functionList["facial"] = false;
                functionList["lod"] = false;

                curCharacterName = "Puppet";
                break;
            // HonokaFutaba
            case 2:
                resourcesPathFull = "Assets/HonokaFutabaBasicSet/Resources/Honoka";
                resourcesPath = "Honoka/";
                animationPath = "Honoka/Animations Legacy/f01@";
                AnimationListFile = "animation_list";
                AnimationListFileAll = "animation_list_all";

                CharacterCode = "F01/";
                //animTest = animTestF01;
                /*
                            fbxSetting     = "Viewer Settings/HonokaFutaba/FBXList";
                            animSetting    = "Viewer Settings/HonokaFutaba/AnimationList";
                            animTest = animHonoka;
                */

                fbxCtrlSetting = settingFileDir + CharacterCode + "fbx_ctrl";
                functionList["model"] = true;
                functionList["facial"] = false;
                functionList["lod"] = true;
                faceMat_L = Resources.Load<Material>("Honoka/Materials/f01_face_00_l");
                faceMat_M = Resources.Load<Material>("Honoka/Materials/f01_face_00_m");

                curCharacterName = "Honoka Futaba";

                break;

            // AoiKiryu
            case 3:
                resourcesPathFull = "Assets/Aoi Character Pack/Resources/Aoi";
                resourcesPath = "Aoi/";
                animationPath = "Aoi/Animations Legacy/f02@";
                AnimationListFile = "animation_list";
                AnimationListFileAll = "animation_list_all";

                CharacterCode = "F02/";
                //animTest = animTestF02;

                /*
                            fbxSetting     = "Viewer Settings/AoiKiryu/FBXList";
                            animSetting    = "Viewer Settings/AoiKiryu/AnimationList";
                            animTest = animAoi;
                */
                fbxCtrlSetting = settingFileDir + CharacterCode + "fbx_ctrl";
                functionList["model"] = true;
                functionList["facial"] = false;
                functionList["lod"] = true;
                faceMat_L = Resources.Load<Material>("Aoi/Materials/f02_face_00_l");
                faceMat_M = Resources.Load<Material>("Aoi/Materials/f02_face_00_m");

                curCharacterName = "Aoi Kiryu";

                break;

            // Succubus Arum
            case 4:
                resourcesPathFull = "Assets/Succubus Twins Character Pack Ver1.10f/Resources/Arum/";
                resourcesPath = "Arum/";
                animationPath = "Arum/Animations Legacy/animation@";
                AnimationListFile = "animation_list_a";
                AnimationListFileAll = "animation_list_a";

                FacialTexListFile = settingFileDir + CharacterCode + "facial_texture_list_a";

                CharacterCode = "F03/Arum/";
                //animTest = animTestF03_0;
                /*
                            fbxSetting     = "Viewer Settings/Arum/FBXList";
                            animSetting    = "Viewer Settings/Arum/AnimationList";
                            animTest = animArum;
                */
                fbxCtrlSetting = settingFileDir + CharacterCode + "fbx_ctrl";
                functionList["model"] = false;
                functionList["facial"] = true;
                functionList["lod"] = true;

                txt = Resources.Load<TextAsset>(FacialTexListFile);
                facialTexList = txt.text.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);



                faceObjName = "succubus_a";
                facialMatName = "succubus_a_face";
                faceMat_L = Resources.Load<Material>("Arum/Materials/succubus_a_face_l");
                faceMat_M = Resources.Load<Material>("Arum/Materials/succubus_a_face_m");
                FacialTexListFile = settingFileDir + CharacterCode + "facial_texture_list_a";

                curCharacterName = "Succubus Arum";
                break;

            // Succubus Asphodel
            case 5:
                resourcesPathFull = "Assets/Succubus Twins Character Pack Ver1.10f/Resources/Asphodel/";
                resourcesPath = "Asphodel/";
                animationPath = "Asphodel/Animations Legacy/animation@";
                CharacterCode = "F03/Asphodel/";
                AnimationListFile = "animation_list_b";
                AnimationListFileAll = "animation_list_b";

                FacialTexListFile = settingFileDir + CharacterCode + "facial_texture_list_b";

                //animTest          = animTestF03_1;
                fbxCtrlSetting = settingFileDir + CharacterCode + "fbx_ctrl";
                /*
                            fbxSetting     = "Viewer Settings/Asphodel/FBXList";
                            animSetting    = "Viewer Settings/Asphodel/AnimationList";
                            animTest = animAsphodel;
                */
                functionList["model"] = false;
                functionList["facial"] = true;
                functionList["lod"] = true;

                txt = Resources.Load<TextAsset>(FacialTexListFile);
                facialTexList = txt.text.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                faceObjName = "succubus_b";
                facialMatName = "succubus_b_face";

                faceMat_L = Resources.Load<Material>("Asphodel/Materials/succubus_b_face_l");
                faceMat_M = Resources.Load<Material>("Asphodel/Materials/succubus_b_face_m");
                curCharacterName = "Succubus Asphodel";
                break;

            // Satomi Makise
            case 6:
                resourcesPathFull = "Assets/Satomi Character Pack/Resources/Satomi";
                resourcesPath = "Satomi/";
                animationPath = "Satomi/Animations Legacy/f05@";
                AnimationListFile = "animation_list";
                AnimationListFileAll = "animation_list_all";

                CharacterCode = "F05/";
                //animTest = animTestM01;
                fbxCtrlSetting = settingFileDir + CharacterCode + "fbx_ctrl";
                faceMat_L = Resources.Load<Material>("Satomi/Materials/f05_face_00_l");
                faceMat_M = Resources.Load<Material>("Satomi/Materials/f05_face_00_m");
                functionList["model"] = true;
                functionList["facial"] = false;
                functionList["lod"] = true;
                curCharacterName = "Satomi Makise";

                break;
        }
        txt = Resources.Load<TextAsset>(settingFileDir + CharacterCode + FBXListFile);
        modelList = txt.text.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        txt = Resources.Load<TextAsset>(settingFileDir + CharacterCode + AnimationListFile);
        animationList = txt.text.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        txt = Resources.Load<TextAsset>(settingFileDir + CharacterCode + AnimationListFileAll);
        animationListAll = txt.text.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        //	txt = Resources.Load<TextAsset>(fbxSetting);
        //	modelList =txt.text.Split(["\r","\n"],1);
        //	this.setModelList();

        //	txt = Resources.Load<TextAsset>(animSetting);
        //	animationList =txt.text.Split(["\r","\n"],1);
        //	this.setAnimationList();

        txt = Resources.Load<TextAsset>(fbxCtrlSetting);
        xDoc = new XmlDocument();
        xDoc.LoadXml(txt.text);

        this.SetInit();
    }

    public void SetNextCharacter(int _add)
    {
        curCharacter += _add;

        if (curCharacter > 6)
        {
            curCharacter = 0;
        }
        else if (curCharacter < 0)
        {
            curCharacter = 6;
        }

        switch (curCharacter)
        {
            case 0:
                CharacterCode = "M01/";
                break;
            case 1:
                CharacterCode = "Puppet/";
                break;
            case 2:
                CharacterCode = "F01/";
                break;
            case 3:
                CharacterCode = "F02/";
                break;
            case 4:
                CharacterCode = "F03/Arum/";
                break;
            case 5:
                CharacterCode = "F03/Asphodel/";
                break;
            case 6:
                CharacterCode = "F05/";
                break;
        }

        txt = Resources.Load<TextAsset>(settingFileDir + CharacterCode + FBXListFile);
        modelList = txt.text.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        var loaded = Resources.Load<GameObject>(modelList[0] + "_h");

        if (loaded == null)
        {
            this.SetNextCharacter(_add);
        }

        SetSettings(curCharacter);
    }
    void characterExistCheck()
    {
    }

    //-----------------------
    // setAnimationList
    //-----------------------
    /*
    void setAnimationList (){
        //Search to Animation Directory
        FIXME_VAR_TYPE info= new DirectoryInfo(resourcesPathFull+"Animations Legacy");
        FIXME_VAR_TYPE fileInfo= info.GetFiles();
        string[] fileName;

        animationList = new string[fileInfo.Length/2-1];
        animationNameList = new string[fileInfo.Length/2-1];

        int i = 0;
        foreach(file in fileInfo)
        {
            if(
                file.Extension == ".fbx" && 
                file.Name != "animation.fbx"
            ){
                string fileNameAll = Regex.Replace(file.Name, ".fbx", "");
                fileName = fileNameAll.Split(["@"],1);

                animationList[i]     = fileNameAll;
                animationNameList[i] = fileName[1];
                i++;
            }
        }
    }
    */

    //-----------------------
    // setAnimationList
    //-----------------------
    void setAnimationList_old()
    {
        var AnimationClipAll = Resources.LoadAll<AnimationClip>("Animations Legacy");

        foreach (var file in AnimationClipAll)
        {
            AnimationClip clip = file;
        }
    }
    //-----------------------
    // setModelList
    //-----------------------
    /*
    void setModelList (){
        //Search to Models Directory
        FIXME_VAR_TYPE info= new DirectoryInfo(resourcesPathFull+"Models Legacy");
        FIXME_VAR_TYPE fileInfo= info.GetFiles();
        string[] fileNameArray;
        string fileName;
        string fileNameOld;

        int fileCount = fileInfo.Length/2/3;

        modelList     = new string[fileCount];
        modelNameList = new string[fileCount];

        int i = 0;
        foreach(file in fileInfo)
        {
            if(file.Extension == ".fbx")
            {
                string fileNameAll = Regex.Replace(file.Name, ".fbx", "");
                fileNameArray = fileNameAll.Split(["_"],1);
                fileName = "";
                for(float fi = 0;fi < fileNameArray.Length-1;fi++){
                    if(fi == 0){
                        fileName += fileNameArray[fi];
                    }else{
                        fileName += "_"+fileNameArray[fi];
                    }
                }
                if(fileNameOld != fileName){
                    modelList[i] = fileName;
                    fileNameOld  = fileName;
                    i++;
                }
            }
        }
    }
    */

    void SetNextFacial(int _add)
    {
        curFacial += _add;


        if (facialCount <= curFacial)
        {
            curFacial = 0;
        }
        else if (curFacial < 0)
        {
            curFacial = facialCount - 1;
        }

        if (curLOD == 0)
        {
            this.SetFacialBlendShape(curFacial);
        }
        else
        {
            this.SetFacialTex(curFacial);
        }
    }
    void SetInitFacial()
    {

        curFacial = 0;
        curFacialName = "Default";

        var Obj = GameObject.Find(curModelName + "_face");

        var mesh = Obj.GetComponent<SkinnedMeshRenderer>().sharedMesh;
        facialCount = mesh.blendShapeCount + 1;

        Obj.name = Obj.name + segmentCode;
    }

    void SetFacialBlendShape(int _i)
    {
        SkinnedMeshRenderer renderer = GameObject.Find(curModelName + "_face" + segmentCode).GetComponent<SkinnedMeshRenderer>();
        var mesh = GameObject.Find(curModelName + "_face" + segmentCode).GetComponent<SkinnedMeshRenderer>().sharedMesh;
        int count = facialCount - 1;

        int facialNum = _i - 1;

        //facial reset
        for (int i = 0; i < facialCount - 1; i++)
        {
            renderer.SetBlendShapeWeight(i, 0);
        }
        //facial set
        if (facialNum >= 0)
        {
            curFacialName = mesh.GetBlendShapeName(facialNum);
            renderer.SetBlendShapeWeight(facialNum, 100);
        }
        else
        {
            curFacialName = "default";
        }
    }

    void SetFacialTex(int _i)
    {
        string file = resourcesPath + "Textures/" + facialTexList[_i] + lodList[curLOD];
        string matName = facialMatName + lodList[curLOD] + " (Instance)";
        Texture2D tex = Resources.Load<Texture2D>(file);

        curFacialName = facialTexList[_i];
        foreach (Material mat in faceSM.GetComponent<Renderer>().sharedMaterials)
        {
            if (mat)
            {
                if (mat.name == matName)
                {
                    mat.SetTexture("_MainTex", tex);
                }
            }
        }
    }

    void facialMaterialSet()
    {
        if (curLOD != 0)
        {
            var fName = faceObjName + lodList[curLOD] + "_face";
            var faceObj = GameObject.Find(fName);

            //faceSM = faceObj.GetComponentInChildren(typeof(SkinnedMeshRenderer)) as SkinnedMeshRenderer;
            faceSM = faceObj.GetComponent<SkinnedMeshRenderer>();

            int i = 0;
            foreach (Material mat in faceSM.GetComponent<Renderer>().sharedMaterials)
            {
                if (mat.name == faceMat_M.name)
                {
                    faceSM.GetComponent<Renderer>().materials[i] = faceMat_M;
                }
                else if (mat.name == faceMat_L.name)
                {
                    faceSM.GetComponent<Renderer>().materials[i] = faceMat_L;
                }
                i++;
            }
        }
    }


    void SetInitModel()
    {
        curModel = 0;
        ModelChange(modelList[curModel] + lodList[curLOD]);
    }

    void SetNextModel(int _add)
    {
        curModel += _add;

        if (modelList.Length <= curModel)
        {
            curModel = 0;
        }
        else if (curModel < 0)
        {
            curModel = modelList.Length - 1;
        }
        ModelChange(modelList[curModel] + lodList[curLOD]);

    }

    void SetNextLOD(int _add)
    {
        curLOD += _add;

        if (lodList.Length <= curLOD)
        {
            curLOD = 0;
        }
        else if (curLOD < 0)
        {
            curLOD = lodList.Length - 1;
        }
        ModelChange(modelList[curModel] + lodList[curLOD]);

        if (functionList["facial"])
        {
            if (curLOD == 0)
            {
                var Obj = GameObject.Find(curModelName + "_face");
                if (Obj) Obj.name = Obj.name + segmentCode;
                this.SetFacialBlendShape(curFacial);
            }
            else
            {
                this.SetFacialTex(curFacial);
            }
        }

    }

    void ModelChange(string _name)
    {
        if (!string.IsNullOrEmpty(_name))
        {
            //print("ModelChange : "+_name);
            curModelName = Path.GetFileNameWithoutExtension(_name);
            //FIXME_VAR_TYPE loaded= Resources.Load(resourcesPath+"Models Legacy/"+_name ,GameObject);
            var loaded = Resources.Load<GameObject>(_name);
            var pos = Vector3.zero;
            float rotY = 0.0f;
            if (obj)
            {
                pos = obj.transform.position;
                rotY = obj.transform.eulerAngles.y;
            }
            Destroy(obj);

            obj = Instantiate(loaded) as GameObject;
            obj.transform.position = pos;
            var eulerAngles = obj.transform.eulerAngles;
            eulerAngles.y = rotY;
            obj.transform.eulerAngles = eulerAngles;

            SM = obj.GetComponentInChildren(typeof(SkinnedMeshRenderer)) as SkinnedMeshRenderer;
            SM.quality = SkinQuality.Bone4;
            SM.updateWhenOffscreen = false;

            /*
            foreach(f in animationList){
                AnimationClip animClip = Resources.Load(resourcesPath+"Animations Legacy/"+f, AnimationClip);
                obj.animation.AddClip(animClip,animClip.name);
            }

            SetAnimation(""+animationNameList[curAnim]);
            SetAnimationSpeed( animSpeed );
            */


            //FacialMaterialCopy puppet reject
            if (curCharacter != 1)
            {
                int i = 0;
                foreach (Material mat in SM.GetComponent<Renderer>().sharedMaterials)
                {
                    if (mat.name == faceMat_M.name)
                    {
                        SM.GetComponent<Renderer>().materials[i] = faceMat_M;
                    }
                    else if (mat.name == faceMat_L.name)
                    {
                        SM.GetComponent<Renderer>().materials[i] = faceMat_L;
                    }
                    i++;
                }
            }

            //AnimationClip Add
            foreach (string animName in animationListAll)
            {
                GameObject animObj = Resources.Load<GameObject>(animationPath + animName);

                if (animObj)
                {
                    var anim = animObj.GetComponent<Animation>().clip;
                    obj.GetComponent<Animation>().AddClip(anim, animName);
                }

            }

            /*
                    for each ( AnimationState anim in animTest.animation) {
                        obj.animation.AddClip(anim.clip,anim.name);
                    }
            */
            viewCam.ModelTarget(GetBone(obj, boneName));

            if (functionList["facial"])
            {
                this.facialMaterialSet();
            }
            this.SetAnimation("" + animationList[curAnim]);
            this.SetAnimationSpeed(animSpeed);

        }
    }

    void SetAnimationSpeed(float _speed)
    {
        foreach (AnimationState state in obj.GetComponent<Animation>())
        {
            state.speed = _speed;
        }
    }

    void SetInitMotion()
    {
        curAnim = 0;
        //SetAnimation(animationNameList[curAnim]);
        SetAnimation(animationList[curAnim]);
        SetAnimationSpeed(animSpeed);
    }

    void SetNextMotion(int _add)
    {
        curAnim += _add;
        playOnceFlg = true;

        if (animationList.Length <= curAnim)
        {
            curAnim = 0;
        }
        else if (curAnim < 0)
        {
            curAnim = animationList.Length - 1;
        }

        //SetAnimation(animationNameList[curAnim]);
        SetAnimation(animationList[curAnim]);
        SetAnimationSpeed(animSpeed);
    }

    void playAnimation()
    {
        obj.GetComponent<Animation>().wrapMode = WrapMode.Once;

        GameObject animObj = Resources.Load<GameObject>(animationPath + curAnimName);
        if (animObj)
        {
            if (playOnceFlg) obj.GetComponent<Animation>().Play(curAnimName);
        }
        else
        {
            SetNextModel(-1);
            SetNextModel(1);
            print(curAnimName + " animation clip does not exist");
        }


        if (animReplay && playOnceFlg)
        {
            playOnceFlg = true;
        }
        else
        {
            playOnceFlg = false;
        }
    }

    void SetAnimation(string _name)
    {
        if (!string.IsNullOrEmpty(_name))
        {
            //print("SetAnimation : "+_name);
            curAnimName = "" + _name;

            //obj.animation.wrapMode = WrapMode.Once;
            //obj.animation.Play(curAnimName);
            this.timerReset();
            SetFixedFbx(xDoc, obj, curModelName, curAnimName, curLOD);
        }
    }

    void SetInitBackGround()
    {
        curBG = 0;
        SetBackGround(backGroundList[curBG]);
    }

    void SetNextBackGround(int _add)
    {
        curBG += _add;
        if (backGroundList.Length <= curBG)
        {
            curBG = 0;
        }
        else if (curBG < 0)
        {
            curBG = backGroundList.Length - 1;
        }
        SetBackGround(backGroundList[curBG]);
    }

    void SetBackGround(string _name)
    {
        return;
        if (!string.IsNullOrEmpty(_name))
        {
            //print("SetBackGround : "+_name);

            //BackGround		
            curBgName = Path.GetFileNameWithoutExtension(_name);
            var loaded = Resources.Load<Texture2D>(_name);
            var obj = GameObject.Find("BillBoard") as GameObject;
            obj.GetComponent<Renderer>().material.mainTexture = loaded;


            //StageTex
            loaded = Resources.Load<Texture2D>(stageTexList[curBG]);
            //obj = GameObject.Find("Plane") as GameObject;
            planeObj.GetComponent<Renderer>().material.mainTexture = loaded;

            if (curBG == 0)
            {
                planeObj.SetActive(false);
            }
            else
            {
                planeObj.SetActive(true);
            }


        }
    }

    Transform GetBone(GameObject _obj, string _bone)
    {
        SkinnedMeshRenderer SM = _obj.GetComponentInChildren(typeof(SkinnedMeshRenderer)) as SkinnedMeshRenderer;
        if (SM)
        {
            foreach (Transform t in SM.bones)
            {
                if (t.name == _bone)
                {
                    return t;
                }
            }
        }

        return null;
    }
    void SetFixedFbx(XmlDocument _xDoc, GameObject _obj, string _model, string _anim, int _lod)
    {
        if (_xDoc == null) return;
        if (_obj == null) return;


        XmlNode xNode;
        XmlNode xNodeTex;
        XmlNode xNodeAni;
        string t;

        t = "Root/Texture[@Lod=''or@Lod='" + _lod + "'][Info[@Model=''or@Model='" + _model + "'][@Ani=''or@Ani='" + _anim + "']]";
        xNodeTex = _xDoc.SelectSingleNode(t);

        if (xNodeTex != null)
        {
            string matname = xNodeTex.Attributes["Material"].InnerText;
            string property = xNodeTex.Attributes["Property"].InnerText;
            string file = xNodeTex.Attributes["File"].InnerText;
            //print("Change Texture To "+matname+" : " + property +" : "+file);
            foreach (Material mat in SM.GetComponent<Renderer>().sharedMaterials)
            {
                if (mat)
                {
                    if (mat.name == matname)
                    {

                        Texture2D tex = Resources.Load<Texture2D>(file);
                        mat.SetTexture(property, tex);
                    }
                }
            }
        }

        t = "Root/Animation[@Lod=''or@Lod='" + _lod + "'][Info[@Model=''or@Model='" + _model + "'][@Ani=''or@Ani='" + _anim + "']]";
        xNodeAni = _xDoc.SelectSingleNode(t);

        if (xNodeAni != null)
        {
            string ani = xNodeAni.Attributes["File"].InnerText;
            curAnimName = ani;
            //print("Change Animation To "+curAnimName);
            _obj.GetComponent<Animation>().Play(curAnimName);
        }


        //init Position
        Vector3 pos;
        Vector3 rot;
        t = "Root/Position[@Ani=''or@Ani='" + _anim + "']";
        xNodeAni = _xDoc.SelectSingleNode(t);
        if (xNodeAni != null)
        {
            pos.x = float.Parse(xNodeAni.Attributes["PosX"].InnerText);
            pos.y = float.Parse(xNodeAni.Attributes["PosY"].InnerText);
            pos.z = float.Parse(xNodeAni.Attributes["PosZ"].InnerText);
            rot.x = float.Parse(xNodeAni.Attributes["RotX"].InnerText);
            rot.y = float.Parse(xNodeAni.Attributes["RotY"].InnerText);
            rot.z = float.Parse(xNodeAni.Attributes["RotZ"].InnerText);

            //obj.transform.position = pos;
            //obj.transform.eulerAngles = rot;

            var tmpPos = obj.transform.position;
            tmpPos.y = pos.y;
            obj.transform.position = tmpPos;
        }



    }
}