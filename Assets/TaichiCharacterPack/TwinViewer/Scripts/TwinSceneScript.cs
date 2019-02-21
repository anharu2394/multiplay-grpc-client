using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using UnityEngine;

public class TwinSceneScript : MonoBehaviour
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

    public Rect guiOnButtonRect = new Rect(20, 40, 300, 500);
    public Rect guiOnBGRect = new Rect(20, 40, 300, 500);

    private bool guiShowFlg = false;
    private float guiShowTime = 0f;
    private float guiShowAlpha = 1f;
    private string segmentCode = "_A";

    private string CharacterCode = "M01/";
    private string FBXListFile = "fbx_list";
    private string AnimationListFile = "animation_list";
    private string AnimationListFileAll = "animation_list_all";
    private string FbxCtrlFile = "fbx_ctrl";

    private string ParticleListFile = "ParticleList";
    private string ParticleAnimationListFile = "ParticleAnimationList";
    private string FacialTexListFile = "facial_texture_list";
    private string facialMatName = "chr01_F03_face";

    private float curParticle = 1;

    private Vector3 oldMousePosition;
    private float popupWaitingTime = 2;
    private float popupWaitingTimeNow = 0;

    //string TitleTextFile = "TitleText";
    private bool guiOn = true;
    private float initPosX = 0.3f;
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

    private string curCharacterName = "";

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

        planeObj = GameObject.Find("Plane") as GameObject;
        txt = Resources.Load<TextAsset>(settingFileDir + "background_list");
        backGroundList = txt.text.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        txt = Resources.Load<TextAsset>(settingFileDir + "stage_texture_list");
        stageTexList = txt.text.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        this.SetSettings(0);
        this.SetInitBackGround();
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
        if (Input.GetKeyDown("2")) SetNextModel(1);

        if (Input.GetKeyDown("q")) SetNextMotion(-1);
        if (Input.GetKeyDown("w")) SetNextMotion(1);

        if (Input.GetKeyDown("a")) SetNextBackGround(-1);
        if (Input.GetKeyDown("s")) SetNextBackGround(1);

        if (Input.GetKeyDown("z")) SetNextLOD(-1);
        if (Input.GetKeyDown("x")) SetNextLOD(1);
    }



    void popUp()
    {
        if (Input.GetMouseButton(0))
        {
            popupWaitingTimeNow = 0;
        }
        if (oldMousePosition == Input.mousePosition)
        {
            popupWaitingTimeNow += Time.deltaTime;
        }
        else
        {
            popupWaitingTimeNow = 0;
        }

        oldMousePosition = Input.mousePosition;

        if (popupWaitingTime > popupWaitingTimeNow)
        {
            return;
        }

        float sw = Screen.width;
        float sh = Screen.height;

        Vector2[] minPos;
        Vector2[] maxPos;
        Rect[] popupRect;
        string[] popupText;
        int popupCount = 17;

        minPos = new Vector2[popupCount];
        maxPos = new Vector2[popupCount];
        popupRect = new Rect[popupCount];
        popupText = new string[popupCount];
        float topPosY = sh - 10;
        float iconHeight = 50;
        float iconWidth = 100;
        float iconMargin = 9.5f;

        float minX;
        float minY;

        float maxX;
        float maxY;

        if (guiOn)
        {
            ///////////////////////////
            // Left Menu
            ///////////////////////////
            // Character Change.

            maxY = topPosY;
            minY = topPosY - iconHeight;
            minX = 10;
            maxX = iconWidth + minX;

            minPos[0] = new Vector2(minX, minY);
            maxPos[0] = new Vector2(maxX, maxY);
            popupRect[0] = new Rect(123, 10, 120, 23);
            popupText[0] = "Character Change.";

            // Model Change.

            maxY = topPosY - iconHeight - iconMargin;
            minY = topPosY - iconHeight * 2 - iconMargin;

            //minPos[1]    = Vector2(20,469);
            //maxPos[1]    = Vector2(120,520.5f);
            minPos[1] = new Vector2(minX, minY);
            maxPos[1] = new Vector2(maxX, maxY);
            popupRect[1] = new Rect(123, 69, 120, 23);
            popupText[1] = "Model Change.";

            // Motion Change.

            maxY = topPosY - iconHeight * 2 - iconMargin * 2;
            minY = topPosY - iconHeight * 3 - iconMargin * 2;

            //minPos[2]    = Vector2(20,409.5f);
            //maxPos[2]    = Vector2(120,461);
            minPos[2] = new Vector2(minX, minY);
            maxPos[2] = new Vector2(maxX, maxY);
            popupRect[2] = new Rect(123, 129, 120, 40);
            popupText[2] = "Motion Change.\nRepeat Setting.";

            // Facial Change.
            maxY = topPosY - iconHeight * 3 - iconMargin * 3;
            minY = topPosY - iconHeight * 4 - iconMargin * 3;

            //minPos[3]    = Vector2(20,350);
            //maxPos[3]    = Vector2(120,401.5f);
            minPos[3] = new Vector2(minX, minY);
            maxPos[3] = new Vector2(maxX, maxY);
            popupRect[3] = new Rect(123, 190, 120, 23);
            popupText[3] = "Facial Change.";

            // Lod Change.

            maxY = topPosY - iconHeight * 4 - iconMargin * 4;
            minY = topPosY - iconHeight * 5 - iconMargin * 4;

            //minPos[4]    = Vector2(20,290.5f);
            //maxPos[4]    = Vector2(120,342);
            minPos[4] = new Vector2(minX, minY);
            maxPos[4] = new Vector2(120, maxY);
            popupRect[4] = new Rect(123, 250, 120, 23);
            popupText[4] = "Lod Change.";
            ///////////////////////////
            // Right Menu
            ///////////////////////////


            float rightPopupMargin = 245;
            float rightPopupX;

            rightPopupX = Screen.width - rightPopupMargin;

            // Character Change.

            maxY = topPosY;
            minY = topPosY - iconHeight;

            maxX = sw - 10;
            minX = sw - 10 - iconWidth;


            minPos[5] = new Vector2(minX, minY);
            maxPos[5] = new Vector2(maxX, maxY);
            popupRect[5] = new Rect(rightPopupX, 10, 120, 23);
            popupText[5] = "Character Change.";

            // Model Change.
            maxY = topPosY - iconHeight - iconMargin;
            minY = topPosY - iconHeight * 2 - iconMargin;

            minPos[6] = new Vector2(minX, minY);
            maxPos[6] = new Vector2(maxX, maxY);

            popupRect[6] = new Rect(rightPopupX, 69, 120, 23);
            popupText[6] = "Model Change.";

            // Motion Change.
            maxY = topPosY - iconHeight * 2 - iconMargin * 2;
            minY = topPosY - iconHeight * 3 - iconMargin * 2;

            minPos[7] = new Vector2(minX, minY);
            maxPos[7] = new Vector2(maxX, maxY);

            popupRect[7] = new Rect(rightPopupX, 129, 120, 40);
            popupText[7] = "Motion Change.\nRepeat Setting.";

            // Facial Change.
            maxY = topPosY - iconHeight * 3 - iconMargin * 3;
            minY = topPosY - iconHeight * 4 - iconMargin * 3;

            minPos[8] = new Vector2(minX, minY);
            maxPos[8] = new Vector2(maxX, maxY);

            popupRect[8] = new Rect(rightPopupX, 190, 120, 23);
            popupText[8] = "Facial Change.";

            // Lod Change.
            maxY = topPosY - iconHeight * 4 - iconMargin * 4;
            minY = topPosY - iconHeight * 5 - iconMargin * 4;

            minPos[9] = new Vector2(minX, minY);
            maxPos[9] = new Vector2(maxX, maxY);

            popupRect[9] = new Rect(rightPopupX, 250, 120, 23);
            popupText[9] = "Lod Change.";
        }

        if (guiShowFlg)
        {
            // Top Menu

            maxY = sh - 50;
            minY = maxY - 50;

            float topMenuX = camGuiBodyRect.x;
            iconWidth = 50;
            iconMargin = 8;

            // Camera Rotate
            maxX = topMenuX + iconWidth;
            minX = topMenuX;

            minPos[10] = new Vector2(minX, minY);
            maxPos[10] = new Vector2(maxX, maxY);
            popupRect[10] = new Rect(minX, 25, 150, 20);
            popupText[10] = "Camera Rotate.";

            // Camera Move
            maxX += iconWidth + iconMargin;
            minX += iconWidth + iconMargin;

            minPos[11] = new Vector2(minX, minY);
            maxPos[11] = new Vector2(maxX, maxY);
            popupRect[11] = new Rect(minX, 25, 150, 20);
            popupText[11] = "Camera Move.";

            // Camera Zoom
            maxX += iconWidth + iconMargin;
            minX += iconWidth + iconMargin;

            minPos[12] = new Vector2(minX, minY);
            maxPos[12] = new Vector2(maxX, maxY);
            popupRect[12] = new Rect(minX, 25, 150, 20);
            popupText[12] = "Camera Zoom.";

            // Camera Fix
            maxX += iconWidth + iconMargin;
            minX += iconWidth + iconMargin;

            minPos[13] = new Vector2(minX, minY);
            maxPos[13] = new Vector2(maxX, maxY);
            popupRect[13] = new Rect(minX - 100, 25, 150, 20);
            popupText[13] = "Camera Target Lock.";

            // Camera Reset
            maxX += iconWidth + iconMargin;
            minX += iconWidth + iconMargin;

            minPos[14] = new Vector2(minX, minY);
            maxPos[14] = new Vector2(maxX, maxY);
            popupRect[14] = new Rect(minX - 100, 25, 150, 20);
            popupText[14] = "Camera Reset.";

            /////////////////

            minX = guiOnButtonRect.x;
            maxX = minX + 100;

            // Background Change
            minPos[15] = new Vector2(minX, minY);
            maxPos[15] = new Vector2(maxX, maxY);
            popupRect[15] = new Rect(minX + 50 - 73, 25, 150, 20);
            popupText[15] = "BackGround:" + (curBG + 1) + "/" + backGroundList.Length + ":" + curBgName + "\n";

            // GUI On/Off Change
            minX = guiOnButtonRect.x + iconWidth * 2 + 70;
            maxX = minX + iconWidth;

            minPos[16] = new Vector2(minX, minY);
            maxPos[16] = new Vector2(maxX, maxY);
            popupRect[16] = new Rect(minX + (iconWidth / 2) - 50, 25, 100, 20);

            if (guiOn)
            {
                popupText[16] = "Viewer UI:On";
            }
            else
            {
                popupText[16] = "Viewer UI:Off";
            }
        }

        for (int i = 0; i < popupCount; i++)
        {
            if (
                Input.mousePosition.x > minPos[i].x &&
                Input.mousePosition.x < maxPos[i].x &&
                Input.mousePosition.y > minPos[i].y &&
                Input.mousePosition.y < maxPos[i].y
            )
            {
                GUI.Box(popupRect[i], popupText[i]);
            }
        }
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

        float minX;
        float maxX;
        float minY;
        float maxY;

        minX = 10;
        maxX = 270;
        //Left Slider
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

        float sw = Screen.width;
        float sh = Screen.height;

        float guiShowHandleWidth = 40;
        float guiShowHandleHeight = 20;

        float guiShowHandleXmin = (Screen.width / 2) - (guiShowHandleWidth / 2);
        float guiShowHandleXmax = guiShowHandleXmin + guiShowHandleWidth;

        float guiShowHandleYmin = Screen.height - guiShowHandleHeight;
        float guiShowHandleYmax = Screen.height;

        //adjust guiOn
        guiOnBGRect.x = (sw / 2) - (guiOnBGRect.width / 2);
        guiOnButtonRect.x = (sw / 2) + 40;
        camGuiBodyRect.x = (sw / 2) - (guiOnBGRect.width / 2) + 20;
        if (
            Input.mousePosition.x > guiShowHandleXmin &&
            Input.mousePosition.x < guiShowHandleXmax &&
            Input.mousePosition.y > guiShowHandleYmin &&
            Input.mousePosition.y < guiShowHandleYmax
        )
        {
            guiShowFlg = true;
            guiShowAlpha = 1;
            guiShowTime = 0;
        }

        float guiHideHandleYmin = Screen.height - guiOnBGRect.height + guiShowHandleHeight;
        float guiHideHandleXmax = (Screen.width / 2) - (guiOnBGRect.width / 2);
        float guiHideHandleXmin = (Screen.width / 2) + (guiOnBGRect.width / 2);

        if (
            Input.mousePosition.x < guiHideHandleXmax ||
            Input.mousePosition.x > guiHideHandleXmin ||
            Input.mousePosition.y < guiHideHandleYmin
        )
        {
            guiShowFlg = false;
        }

        if (!guiShowFlg) guiShowTime += Time.deltaTime;

        if (
            !guiShowFlg &&
            guiShowTime > 5
        )
        {
            if (guiShowAlpha > 0.5f)
            {
                guiShowAlpha -= Time.deltaTime * 0.5f;
            }
        }
        SetGuiColorAlpha(guiShowAlpha);

        //GUI ON OFF
        Twin2ndSceneScript _Second_SceneScript = gameObject.GetComponent<Twin2ndSceneScript>();

        if (guiSkin)
        {
            GUI.skin = guiSkin;
        }

        /*
        GUILayout.BeginArea (guiOnShowRect);
            GUILayout.BeginVertical();
                guiShowFlg = GUILayout.Toggle(guiShowFlg,"","guiShow");
            GUILayout.EndVertical();
        GUILayout.EndArea ();
        */

        float guiShowSpeed = 200;

        if (guiShowFlg)
        {
            if (guiOnBGRect.y < -30)
            {
                guiOnBGRect.y += Time.deltaTime * guiShowSpeed;
                guiOnButtonRect.y += Time.deltaTime * guiShowSpeed;
                camGuiBodyRect.y += Time.deltaTime * guiShowSpeed;
            }
            else
            {
                guiOnBGRect.y = -30;
                guiOnButtonRect.y = 50;
                camGuiBodyRect.y = 50;
            }

        }
        else
        {
            if (guiOnBGRect.y > -158)
            {
                guiOnBGRect.y -= Time.deltaTime * guiShowSpeed;
                guiOnButtonRect.y -= Time.deltaTime * guiShowSpeed;
                camGuiBodyRect.y -= Time.deltaTime * guiShowSpeed;
            }
            else
            {
                guiOnBGRect.y = -158;
                guiOnButtonRect.y = -80;
                camGuiBodyRect.y = -80;
            }
        }

        GUI.Label(guiOnBGRect, "", "GUIOnBG");
        GUILayout.BeginArea(guiOnButtonRect);
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("", "Left")) SetNextBackGround(-1);
        GUILayout.Label("", "BG");
        if (GUILayout.Button("", "Right")) SetNextBackGround(1);

        GUILayout.FlexibleSpace();

        var guiVal = GUILayout.Toggle(guiOn, "", "GUIOn");
        if (guiOn != guiVal)
        {
            guiOn = guiVal;
            _Second_SceneScript.guiOn = guiVal;
        }

        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        SetGuiColorAlpha(1.0f);

        GUILayout.BeginArea(camGuiBodyRect);
        GUILayout.BeginHorizontal();
        //GUILayout.BeginVertical(); 
        //GUILayout.FlexibleSpace();
        bool newVal;
        newVal = GUILayout.Toggle(CamMode == 0, "", "Rote");
        if (CamMode != 0 && newVal)
        {
            CamMode = 0;
            viewCam.ModeRote();
        }
        GUILayout.FlexibleSpace();
        newVal = GUILayout.Toggle(CamMode == 1, "", "Move");
        if (CamMode != 1 && newVal)
        {
            CamMode = 1;
            viewCam.ModeMove();
        }
        GUILayout.FlexibleSpace();
        newVal = GUILayout.Toggle(CamMode == 2, "", "Zoom");
        if (CamMode != 2 && newVal)
        {
            CamMode = 2;
            viewCam.ModeZoom();
        }
        GUILayout.FlexibleSpace();
        CamModeFix = viewCam.isFixTarget;
        newVal = GUILayout.Toggle(CamModeFix, "", "Fix");
        if (CamModeFix != newVal)
        {
            CamModeFix = newVal;
            viewCam.FixTarget(CamModeFix);
        }
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("", "Reset"))
        {
            viewCam.ModelTarget(GetBone(obj, boneName));
            viewCam.Reset();
        }
        GUILayout.FlexibleSpace();
        //GUILayout.EndVertical();  
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        if (!guiOn)
        {
            this.popUp();
            return;
        }
        scale.x = 1;
        scale.y = 1;
        scale.z = 1.0f;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);

        //GUI Parts Adjust
        textGuiBodyRect.y = sh - (textGuiBodyRect.height + 10);

        sliderGuiBodyRect.y = sh - (sliderGuiBodyRect.height + textGuiBodyRect.height + 28);

        sliderTextBodyRect.y = sh - (sliderTextBodyRect.height + textGuiBodyRect.height + 15);


        this.scrollBarPos();

        if (
            GUI.Button(new Rect(Screen.width / 2 - 150, Screen.height - 80, 300, 30), "MotionSyncro") ||
            Input.GetKeyDown("space")
        )
        {
            this.timerReset();
            _Second_SceneScript.timerReset();
        }

        if (GUI.Button(new Rect(Screen.width / 2 - 150, Screen.height - 40, 300, 30), "Reset"))
        {
            var eulerAngles = obj.transform.eulerAngles;
            eulerAngles.y = 0.0f;
            obj.transform.eulerAngles = eulerAngles;
            var pos = obj.transform.position;
            obj.transform.position = new Vector3(0.3f, 0.0f, 0.0f);
            animSpeed = 1;
            motionDelay = 0;
            SetAnimationSpeed(animSpeed);
            _Second_SceneScript.sliderReset();
        }



        //GUI.Label( new Rect(20,20,500,50), titleText, "Title");

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

        if (onSliderFlg == 4)
        {
            SetGuiColorAlpha(1.0f);
        }
        else
        {
            SetGuiColorAlpha(0.4f);
        }
        //		}
        //Rotate Y
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


        this.popUp();
    }

    void SetInit()
    {
        SetInitModel();
        SetInitMotion();
        SetAnimationSpeed(animSpeed);
        if (functionList["facial"])
        {
            SetInitFacial();
        }
    }

    void timerReset()
    {
        nowTime = 0;
        obj.GetComponent<Animation>().Stop();
        playOnceFlg = true;

    }
    void SetSettings(int _i)
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
                resourcesPathFull = "Assets/Succubus Twins Character Pack/Resources/Asphodel/";
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

        //	txt = Resources.Load(fbxSetting, TextAsset);
        //	modelList =txt.text.Split(["\r","\n"],1);
        //	this.setModelList();

        //	txt = Resources.Load(animSetting, TextAsset);
        //	animationList =txt.text.Split(["\r","\n"],1);
        //	this.setAnimationList();

        txt = Resources.Load<TextAsset>(fbxCtrlSetting);
        xDoc = new XmlDocument();
        xDoc.LoadXml(txt.text);

        this.SetInit();
    }
    void SetNextCharacter(int _add)
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
            Vector3 pos = Vector3.zero;
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