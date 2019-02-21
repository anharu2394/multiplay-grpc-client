using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using UnityEngine;

public class SceneScript : MonoBehaviour
{
    private static void SetGuiColorAlpha(float alpha)
    {
        var color = GUI.color;
        color.a = alpha;
        GUI.color = color;
    }

    public GameObject animTest;

    public GUISkin guiSkin;
    public ViewScript viewCam;
    public string boneName = "Hips";
    public Rect camGuiRootRect = new Rect(870, 25, 93, 420);
    public Rect camGuiBodyRect = new Rect(870, 25, 93, 420);
    public Rect guiOnBodyRect = new Rect(50, 75, 300, 70);
    public Rect sliderTextBodyRect = new Rect(0, 0, 0, 0);
    public Rect sliderGuiBodyRect = new Rect(0, 0, 0, 0);

    public Rect textGuiBodyRect = new Rect(20, 510, 300, 70);
    public Rect modelBodyRect = new Rect(20, 40, 300, 500);
    public string FBXListFile = "fbx_list_a";
    public string AnimationListFile = "animation_list_a";

    public string TitleTextFile = "title_text_a";
    public bool guiOn = true;

    public Material facialMaterial_M_org;
    public Material facialMaterial_L_org;

    public string FacialTexListFile = "facial_texture_list_a";
    public string ParticleListFile = "particle_list_a";
    public string ParticleAnimationListFile = "particle_animation_list_a";

    public string facialMatName = "f01_face_00";

    private bool guiShowFlg = false;
    private const string viewerResourcesPath = "Taichi";
    private string viewerSettingPath = viewerResourcesPath + "/Viewer Settings";
    private string viewerMaterialPath = viewerResourcesPath + "/Viewer Materials";
    private string viewerBackGroundPath = viewerResourcesPath + "/Viewer BackGrounds";
    private string texturePath = viewerResourcesPath + "/Textures";

    private Dictionary<string, bool> functionList = new Dictionary<string, bool>();

    private int curBG = 1;
    private int curAnim = 1;
    private int curModel = 1;
    private int curFacial = 1;
    private int curMode = 1;
    private int curLOD = 0;
    private int curParticle = 1;
    private float animSpeed = 1;

    private string curModelName = "";
    private string curAnimName = "";
    private string curModeName = "";
    private string curBgName = "";
    private string curFacialName = "";
    private string curParticleName = "";

    private int facialCount = 0;
    private float positionY = 0;

    private string[] animationList;
    private string[] animationCommonList;
    private string[] facialTexList;
    private string[] particleAnimationList;
    private string[] particleList;

    private string[] modelList;

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
    private string titleText = "";
    private XmlDocument xDoc;
    private XmlNodeList xNodeList;

    private Material faceMat_L;
    private Material faceMat_M;

    private GameObject BGObject;
    private GameObject BGEff;
    private GameObject BGPlane;
    private GameObject planeObj;

    //Popup
    private Vector3 oldMousePosition;
    private float popupWaitingTime = 2;
    private float popupWaitingTimeNow = 0;

    void Start()
    {

        functionList["particle"] = false;
        functionList["facial"] = false;
        functionList["model"] = true;
        functionList["animation"] = true;
        functionList["background"] = true;
        functionList["lod"] = true;

        functionList["position_x"] = false;
        functionList["position_y"] = false;
        functionList["position_z"] = false;
        functionList["rotate"] = false;
        functionList["animation_speed"] = true;




        viewCam = GameObject.Find("Main Camera").GetComponent<ViewScript>();
        planeObj = GameObject.Find("Plane") as GameObject;

        txt = Resources.Load<TextAsset>(viewerSettingPath + "/background_list");
        backGroundList = txt.text.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        txt = Resources.Load<TextAsset>(viewerSettingPath + "/" + FBXListFile);
        modelList = txt.text.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        txt = Resources.Load<TextAsset>(viewerSettingPath + "/stage_texture_list");
        stageTexList = txt.text.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        if (functionList["particle"])
        {
            txt = Resources.Load<TextAsset>(viewerSettingPath + "/" + ParticleListFile);
            particleList = txt.text.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            txt = Resources.Load<TextAsset>(viewerSettingPath + "/" + ParticleAnimationListFile);
            particleAnimationList = txt.text.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        }

        txt = Resources.Load<TextAsset>(viewerSettingPath + "/" + AnimationListFile);
        animationCommonList = txt.text.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        if (functionList["facial"])
        {
            txt = Resources.Load<TextAsset>(viewerSettingPath + "/" + FacialTexListFile);
            facialTexList = txt.text.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        }

        txt = Resources.Load<TextAsset>(viewerSettingPath + "/" + TitleTextFile);
        titleText = txt.text;

        txt = Resources.Load<TextAsset>(viewerSettingPath + "/fbx_ctrl");
        xDoc = new XmlDocument();
        xDoc.LoadXml(txt.text);

        //FaceMaterial

        /*

        if(resourcesPath == "Arum"){
            faceObjName = "succubus_a";
            faceMat_L = Resources.Load( viewerMaterialPath + "/succubus_a_face_l", Material);
            faceMat_M = Resources.Load( viewerMaterialPath + "/succubus_a_face_m", Material);

        }else if(resourcesPath == "Asphodel"){
            faceObjName = "succubus_b";
            faceMat_L = Resources.Load( viewerMaterialPath + "/succubus_b_face_l", Material);
            faceMat_M = Resources.Load( viewerMaterialPath + "/succubus_b_face_m", Material);
        }
        */

        faceMat_L = facialMaterial_L_org;
        faceMat_M = facialMaterial_M_org;


        if (curMode == 0)
        {
            animationList = particleAnimationList;
        }
        else if (curMode == 1)
        {
            animationList = animationCommonList;
        }

        curModeName = modeTextList[curMode];
        /* Succubus Pack
        BGObject = GameObject.Find("obj01_succubus_pedestal_00");
        BGEff    = GameObject.Find("eff_obj01_00");
        BGPlane  = GameObject.Find("Plane");
        */

        SetInitBackGround();
        SetInitModel();
        SetInitMotion();
        SetAnimationSpeed(animSpeed);

        // Succubus
        // SetInitFacial();

        if (curMode == 0)
        {
            this.SetInitParticle();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown("1")) SetNextModel(-1);
        if (Input.GetKeyDown("2")) SetNextModel(1);

        if (Input.GetKeyDown("q")) SetNextMotion(-1);
        if (Input.GetKeyDown("w")) SetNextMotion(1);

        if (Input.GetKeyDown("a")) SetNextBackGround(-1);
        if (Input.GetKeyDown("s")) SetNextBackGround(1);

        if (Input.GetKeyDown("z")) SetNextLOD(-1);
        if (Input.GetKeyDown("x")) SetNextLOD(1);


    }
    private Vector3 scale;
    private bool test;
    private bool ParticleMode;
    void OnGUI()
    {

        if (guiSkin)
        {
            GUI.skin = guiSkin;
        }

        if (!guiOn)
        {
            GUILayout.BeginArea(guiOnBodyRect);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            var guiVal = GUILayout.Toggle(guiOn, "", "GUIOn");
            if (guiOn != guiVal)
            {
                guiOn = guiVal;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndArea();
            this.popUp();
            return;
        }
        if (
            curMode == 0 &&
            curParticle != 4
        )
        {

            /*
            if ( GUI.Button( new Rect(200, 100, 100, 20), "Particle!" ) )
            {
                this.particleExec();
            }
            */
        }

        //print(Screen.height+" "+Screen.width);
        scale.x = Screen.width / 960.0f;
        scale.y = Screen.height / 600.0f;
        scale.x = 1;
        scale.y = 1;
        scale.z = 1.0f;

        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);

        float sw = Screen.width;
        float sh = Screen.height;

        // GUI Parts Adjust
        textGuiBodyRect.y = sh - (textGuiBodyRect.height + 20);

        //animSpeedGuiBodyRect.y = sh-(animSpeedGuiBodyRect.height+20);
        //animSpeedGuiBodyRect.x = sw-(animSpeedGuiBodyRect.width+20);

        camGuiRootRect.x = sw - camGuiRootRect.width * 0.9f;
        camGuiBodyRect.x = sw - (camGuiBodyRect.width * 0.9f - 15);


        sliderTextBodyRect.x = sw - (sliderTextBodyRect.width + 20);
        sliderTextBodyRect.y = sh - (sliderTextBodyRect.height + 20);

        sliderGuiBodyRect.x = sw - (sliderTextBodyRect.width + sliderGuiBodyRect.width + 25);
        sliderGuiBodyRect.y = sh - (sliderGuiBodyRect.height + 20);

        GUI.Label(new Rect(20, 20, 500, 50), titleText, "Title");

        float buttonSpace = 10;
        bool newVal;

        GUILayout.BeginArea(modelBodyRect);
        //GUILayout.BeginHorizontal();
        //GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        {
            var guiVal = GUILayout.Toggle(guiOn, "", "GUIOn");
            if (guiOn != guiVal)
            {
                guiOn = guiVal;
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(buttonSpace);

        if (functionList["particle"])
        {
            GUILayout.BeginHorizontal();
            newVal = GUILayout.Toggle(ParticleMode, "", "ParticleMode");
            if (ParticleMode != newVal)
            {
                ParticleMode = newVal;
                //if(newVal){
                if (curMode == 0)
                {
                    SetNextMode(1);
                }
                else
                {
                    SetNextMode(-1);
                }
                //}

            }
            GUILayout.EndHorizontal();
            GUILayout.Space(buttonSpace);
        }
        else
        {
            SetGuiColorAlpha(0.5f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("", "ParticleMode");
            GUILayout.EndHorizontal();
            GUILayout.Space(buttonSpace);
            SetGuiColorAlpha(1.0f);

        }

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
            if (GUILayout.Button("", "LeftGlayOut")) { }
            GUILayout.Label("", "Costume");
            if (GUILayout.Button("", "RightGlayOut")) { }
            GUILayout.EndHorizontal();
            GUILayout.Space(buttonSpace);
            SetGuiColorAlpha(1.0f);
        }


        if (functionList["animation"])
        {
            //Motion
            if (curMode == 1)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("", "Left")) SetNextMotion(-1);
                GUILayout.Label("", "Anim");
                if (GUILayout.Button("", "Right")) SetNextMotion(1);
                GUILayout.EndHorizontal();
                GUILayout.Space(buttonSpace);

                //Motion & Particle
            }
            else if (curMode == 0)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("", "Left")) SetNextParticle(-1);
                if (GUILayout.Button("", "ParticleShot"))
                {
                    this.particleExec();
                }

                if (GUILayout.Button("", "Right")) SetNextParticle(1);
                GUILayout.EndHorizontal();
                GUILayout.Space(buttonSpace);
            }
        }
        else
        {
            SetGuiColorAlpha(0.5f);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("", "LeftGlayOut")) { }
            GUILayout.Label("", "Anim");
            if (GUILayout.Button("", "RightGlayOut")) { }
            GUILayout.EndHorizontal();
            GUILayout.Space(buttonSpace);
            SetGuiColorAlpha(1.0f);
        }

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
            if (GUILayout.Button("", "LeftGlayOut")) { }
            GUILayout.Label("", "Facial");
            if (GUILayout.Button("", "RightGlayOut")) { }
            GUILayout.EndHorizontal();
            GUILayout.Space(buttonSpace);
            SetGuiColorAlpha(1.0f);
        }


        if (functionList["background"])
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("", "Left")) SetNextBackGround(-1);
            GUILayout.Label("", "BG");
            if (GUILayout.Button("", "Right")) SetNextBackGround(1);
            GUILayout.EndHorizontal();
            GUILayout.Space(buttonSpace);
        }
        else
        {
            SetGuiColorAlpha(0.5f);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("", "LeftGlayOut")) { }
            GUILayout.Label("", "BG");
            if (GUILayout.Button("", "RightGlayOut")) { }
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
        }
        else
        {
            SetGuiColorAlpha(0.5f);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("", "LeftGlayOut")) { }
            GUILayout.Label("", "LOD");
            if (GUILayout.Button("", "RightGlayOut")) { }
            GUILayout.EndHorizontal();
            GUILayout.Space(buttonSpace);
            SetGuiColorAlpha(1.0f);
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
        GUILayout.EndArea();


        //Slider Text

        GUILayout.BeginArea(sliderTextBodyRect);
        GUILayout.FlexibleSpace();

        if (functionList["position_x"])
        {
            SetGuiColorAlpha(1.0f);
        }
        else
        {
            SetGuiColorAlpha(0.4f);
        }

        string positionXText = "Position X : " + String.Format("{0:F1}", obj.transform.position.x);

        GUILayout.Box(positionXText);
        GUILayout.FlexibleSpace();

        if (functionList["position_y"])
        {
            SetGuiColorAlpha(1.0f);
        }
        else
        {
            SetGuiColorAlpha(0.4f);
        }

        string positionYText = "Position Y : " + String.Format("{0:F1}", obj.transform.position.y);
        GUILayout.Box(positionYText);
        GUILayout.FlexibleSpace();

        if (functionList["position_z"])
        {
            SetGuiColorAlpha(1.0f);
        }
        else
        {
            SetGuiColorAlpha(0.4f);
        }

        string positionZText = "Position Z : " + String.Format("{0:F1}", obj.transform.position.z);
        GUILayout.Box(positionZText);
        GUILayout.FlexibleSpace();

        // if (functionList["rotate_y"])
        // {
        //     SetGuiColorAlpha(1.0f);
        // }
        // else
        // {
        //     SetGuiColorAlpha(0.4f);
        // }

        string rotateText = "Rotate : " + String.Format("{0:F1}", obj.transform.eulerAngles.y);
        GUILayout.Box(rotateText);
        GUILayout.FlexibleSpace();
        if (functionList["animation_speed"])
        {
            SetGuiColorAlpha(1.0f);
        }
        else
        {
            SetGuiColorAlpha(0.4f);
        }
        string animSpeedText = "Animation\nSpeed : " + String.Format("{0:F1}", animSpeed);
        GUILayout.Box(animSpeedText);
        GUILayout.FlexibleSpace();

        GUILayout.EndArea();

        //Slider GUI
        GUILayout.BeginArea(sliderGuiBodyRect);

        if (functionList["position_x"])
        {
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
        }
        else
        {
            //Position X
            SetGuiColorAlpha(0.4f);
            GUILayout.HorizontalSlider(obj.transform.position.x, 0, 0);
            GUILayout.Space(0);
        }


        //Position Y
        if (functionList["position_y"])
        {
            if (onSliderFlg == 2)
            {
                SetGuiColorAlpha(1.0f);
            }
            else
            {
                SetGuiColorAlpha(0.4f);
            }
            //float posYVal = GUILayout.HorizontalSlider(obj.transform.position.y, 0, 3);
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
        }
        else
        {
            SetGuiColorAlpha(0.4f);
            GUILayout.HorizontalSlider(obj.transform.position.y, 0, 0);
            GUILayout.Space(0);
        }



        //Position Z
        if (functionList["position_z"])
        {

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
            GUILayout.Space(0);
        }
        else
        {
            SetGuiColorAlpha(0.4f);
            GUILayout.HorizontalSlider(obj.transform.position.z, 0, 0);
            GUILayout.Space(0);
        }

        //Rotate
        if (functionList["rotate"])
        {
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
            GUILayout.Space(5);
        }
        else
        {
            SetGuiColorAlpha(0.4f);
            GUILayout.HorizontalSlider(obj.transform.eulerAngles.y, 0, 0);
            GUILayout.Space(5);
        }


        if (functionList["animation_speed"])
        {
            if (onSliderFlg == 5)
            {
                SetGuiColorAlpha(1.0f);
            }
            else
            {
                SetGuiColorAlpha(0.4f);
            }
            SetGuiColorAlpha(1.0f);
            //Motion Speed
            float Val = GUILayout.HorizontalSlider(animSpeed, 0, 2);
            if (animSpeed != Val)
            {
                animSpeed = Val;
                SetAnimationSpeed(animSpeed);
                viewCam.MouseLock(true);
            }
            else
            {
                viewCam.MouseLock(false);
            }
        }
        else
        {
            SetGuiColorAlpha(0.4f);
            GUILayout.HorizontalSlider(animSpeed, 0, 0);
        }
        GUILayout.EndArea();
        SetGuiColorAlpha(1.0f);
        /*
            if(curMode == 1){
            GUILayout.BeginArea (animSpeedGuiBodyRect);
                GUILayout.FlexibleSpace();
                float val = GUILayout.HorizontalSlider(animSpeed, 0, 2);
                if(animSpeed != val){ 
                    animSpeed = val;
                    SetAnimationSpeed(animSpeed);
                    viewCam.MouseLock(true);
                }else{
                    viewCam.MouseLock(false);
                }
                GUILayout.FlexibleSpace();
                string animSpeedText = "Animation Speed : " + String.Format("{0:F1}", animSpeed);
                GUILayout.Box(animSpeedText);
                GUILayout.FlexibleSpace();
            GUILayout.EndArea ();

            GUILayout.BeginArea (positionYGuiBodyRect);
                GUILayout.FlexibleSpace();
                float pos = GUILayout.HorizontalSlider(positionY, 0, 3);
                if(positionY != pos){ 
                    positionY = pos;
                    obj.transform.position.y = positionY;
                    viewCam.MouseLock(true);
                }else{
                    viewCam.MouseLock(false);
                }

                GUILayout.FlexibleSpace();
                string positionYText = "positionY : " + String.Format("{0:F1}", positionY);
                GUILayout.Box(positionYText);
                GUILayout.FlexibleSpace();
            GUILayout.EndArea ();
            }
        */

        GUI.Label(camGuiRootRect, "", "CamBG");
        GUILayout.BeginArea(camGuiBodyRect);
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        //GUILayout.FlexibleSpace();
        //bool  newVal;
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
            viewCam.Reset();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        //GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);

        string text = "";

        if (functionList["particle"])
        {
            text += "Mode : " + curModeName + "\n";
        }
        if (functionList["model"])
        {
            text += "Costume : " + (curModel + 1) + " / " + modelList.Length + " : " + curModelName + "\n";
        }

        if (functionList["animation"])
        {
            if (curMode == 0)
            {
                text += "Particle  : " + (curAnim + 1) + " / " + (animationList.Length) + " : " + curParticleName + "\n";
            }
            else
            {
                text += "Animation : " + (curAnim + 1) + " / " + (animationList.Length) + " : " + curAnimName + "\n";
            }
        }
        if (functionList["facial"])
        {
            text += "Facial : " + (curFacial + 1) + " / " + (facialCount) + " : " + curFacialName + "\n";
        }

        if (functionList["background"])
        {
            text += "BackGround : " + (curBG + 1) + " / " + backGroundList.Length + " : " + curBgName + "\n";
        }
        if (functionList["lod"])
        {
            text += "Quality : " + lodTextList[curLOD] + "\n";
        }
        text += "Animation Speed : " + String.Format("{0:F2}", animSpeed);
        GUI.Box(textGuiBodyRect, text);


        this.popUp();

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

        float topMargin = 60;
        float leftMargin = 20;

        float topPosY = sh - topMargin;
        float iconHeight = 50f;
        float iconWidth = 100;
        float iconMargin = 12;

        float minX;
        float minY;

        float maxX;
        float maxY;

        float popUpY;
        float popUpX;
        if (guiOn)
        {
            ///////////////////////////
            // Left Menu
            ///////////////////////////

            // GUI On/Off.
            maxY = topPosY;
            minY = topPosY - iconHeight;

            maxX = iconWidth + leftMargin;
            minX = leftMargin;

            popUpY = sh - maxY;
            popUpX = iconWidth + leftMargin + 10;

            minPos[0] = new Vector2(minX, minY);
            maxPos[0] = new Vector2(maxX, maxY);
            popupRect[0] = new Rect(popUpX - 20, popUpY, 120, 23);
            popupText[0] = "GUI On/Off.";
            // Mode Change.
            maxY -= iconHeight + iconMargin;
            minY -= iconHeight + iconMargin;

            popUpY = sh - maxY;
            minPos[1] = new Vector2(minX, minY);
            maxPos[1] = new Vector2(maxX, maxY);
            popupRect[1] = new Rect(popUpX - 20, popUpY, 120, 23);
            popupText[1] = "Mode Change.";

            // Model Change.
            maxY -= iconHeight + iconMargin;
            minY -= iconHeight + iconMargin;

            popUpY = sh - maxY;
            minPos[2] = new Vector2(minX, minY);
            maxPos[2] = new Vector2(maxX, maxY);
            popupRect[2] = new Rect(popUpX, popUpY, 120, 23);
            popupText[2] = "Model Change.";

            // Motion Change.
            maxY -= iconHeight + iconMargin;
            minY -= iconHeight + iconMargin;

            popUpY = sh - maxY;
            minPos[3] = new Vector2(minX, minY);
            maxPos[3] = new Vector2(maxX, maxY);
            popupRect[3] = new Rect(popUpX, popUpY, 120, 23);
            popupText[3] = "Motion Change.";

            // Facial Change.
            maxY -= iconHeight + iconMargin;
            minY -= iconHeight + iconMargin;

            popUpY = sh - maxY;
            minPos[4] = new Vector2(minX, minY);
            maxPos[4] = new Vector2(maxX, maxY);
            popupRect[4] = new Rect(popUpX, popUpY, 120, 23);
            popupText[4] = "Facial Change.";

            // BackGround Change.
            maxY -= iconHeight + iconMargin;
            minY -= iconHeight + iconMargin;

            popUpY = sh - maxY;
            minPos[5] = new Vector2(minX, minY);
            maxPos[5] = new Vector2(maxX, maxY);
            popupRect[5] = new Rect(popUpX, popUpY, 150, 23);
            popupText[5] = "BackGround Change.";

            // Lod Change.
            maxY -= iconHeight + iconMargin;
            minY -= iconHeight + iconMargin;

            popUpY = sh - maxY;
            minPos[6] = new Vector2(minX, minY);
            maxPos[6] = new Vector2(maxX, maxY);
            popupRect[6] = new Rect(popUpX, popUpY, 120, 23);
            popupText[6] = "Lod Change.";
            ///////////////////////////
            // Right Menu
            ///////////////////////////

            topMargin = 43;

            topPosY = sh - topMargin;
            iconHeight = 57.6f;
            iconWidth = 57.6f;
            iconMargin = 11.5f;



            float rightPopupMargin = 220;
            float rightPopupX;

            rightPopupX = Screen.width - rightPopupMargin;

            // Camera Rotate.
            maxY = topPosY;
            minY = topPosY - iconHeight;

            maxX = sw - 10;
            minX = sw - 10 - iconWidth;
            popUpY = sh - maxY;
            minPos[7] = new Vector2(minX, minY);
            maxPos[7] = new Vector2(maxX, maxY);
            popupRect[7] = new Rect(rightPopupX, popUpY, 120, 23);
            popupText[7] = "Camera Rotate.";

            // Camera Move.
            maxY -= iconHeight + iconMargin;
            minY -= iconHeight + iconMargin;
            popUpY = sh - maxY;
            minPos[8] = new Vector2(minX, minY);
            maxPos[8] = new Vector2(maxX, maxY);

            popupRect[8] = new Rect(rightPopupX, popUpY, 120, 23);
            popupText[8] = "Camera Move.";

            // Camera Zoom.
            maxY -= iconHeight + iconMargin;
            minY -= iconHeight + iconMargin;
            popUpY = sh - maxY;
            minPos[9] = new Vector2(minX, minY);
            maxPos[9] = new Vector2(maxX, maxY);

            popupRect[9] = new Rect(rightPopupX, popUpY, 120, 23);
            popupText[9] = "Camera Zoom.";

            // Camera Target Lock.
            maxY -= iconHeight + iconMargin;
            minY -= iconHeight + iconMargin;
            popUpY = sh - maxY;
            minPos[10] = new Vector2(minX, minY);
            maxPos[10] = new Vector2(maxX, maxY);
            rightPopupX -= 30;
            popupRect[10] = new Rect(rightPopupX, popUpY, 150, 23);
            popupText[10] = "Camera Target Lock.";

            // Camera Reset.
            maxY -= iconHeight + iconMargin;
            minY -= iconHeight + iconMargin;
            popUpY = sh - maxY;
            rightPopupX += 30;
            minPos[11] = new Vector2(minX, minY);
            maxPos[11] = new Vector2(maxX, maxY);

            popupRect[11] = new Rect(rightPopupX, popUpY, 120, 23);
            popupText[11] = "Camera Reset.";
        }
        else
        {
            // GUI On/Off.
            maxY = topPosY;
            minY = topPosY - iconHeight;

            maxX = iconWidth + leftMargin;
            minX = leftMargin;

            popUpY = sh - maxY;
            popUpX = iconWidth + leftMargin + 10;

            minPos[0] = new Vector2(minX, minY);
            maxPos[0] = new Vector2(maxX, maxY);
            popupRect[0] = new Rect(popUpX, popUpY, 120, 23);
            popupText[0] = "GUI On/Off.";

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

        //Left Slider
        minPos[0] = new Vector2(20, 270);
        maxPos[0] = new Vector2(280, 300);
        minPos[1] = new Vector2(20, 240);
        maxPos[1] = new Vector2(280, 270);

        minPos[2] = new Vector2(20, 210);
        maxPos[2] = new Vector2(280, 240);

        minPos[3] = new Vector2(20, 180);
        maxPos[3] = new Vector2(280, 210);

        minPos[4] = new Vector2(20, 140);
        maxPos[4] = new Vector2(280, 180);

        minPos[5] = new Vector2(20, 100);
        maxPos[5] = new Vector2(280, 140);

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

    void SetNextMode(int _add)
    {
        curMode += _add;

        if (curMode > 1)
        {
            curMode = 0;
        }
        else if (curMode < 0)
        {
            curMode = 1;
        }

        if (curMode == 0)
        {
            animationList = particleAnimationList;
        }
        else if (curMode == 1)
        {
            animationList = animationCommonList;
        }
        curModeName = modeTextList[curMode];

        curAnim = 0;
        curParticle = 0;
        curLOD = 0;

        curParticleName = particleList[curParticle];
        this.SetInitModel();
        this.SetInitMotion();
        this.SetAnimationSpeed(animSpeed);
        this.SetInitFacial();
        if (curMode == 0)
        {
            this.SetInitParticle();
        }
    }

    void SetInitParticle()
    {
        if (curMode != 0)
        {
            return;
        }
    }

    void SetNextParticle(int _add)
    {
        curAnim += _add;
        curParticle += _add;

        if (animationList.Length <= curAnim)
        {
            curAnim = 0;
            curParticle = 0;
        }
        else if (curAnim < 0)
        {
            curAnim = animationList.Length - 1;
            curParticle = curAnim;
        }

        curParticleName = particleList[curParticle];
        this.SetParticle();
    }

    void particleExec()
    {
    }

    void SetParticle()
    {

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
        this.ModelChange(modelList[curModel] + lodList[curLOD]);
        this.SetInitParticle();

        if (functionList["facial"])
        {
            if (curLOD == 0)
            {
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
            print("ModelChange : " + _name);
            curModelName = Path.GetFileNameWithoutExtension(_name);
            var loaded = Resources.Load<GameObject>(_name);
            Destroy(obj);

            obj = Instantiate(loaded) as GameObject;

            SM = obj.GetComponentInChildren(typeof(SkinnedMeshRenderer)) as SkinnedMeshRenderer;
            SM.quality = SkinQuality.Bone4;
            SM.updateWhenOffscreen = true;
            viewCam.ModelTarget(GetBone(obj, boneName));


            int i = 0;
            foreach (Material mat in SM.GetComponent<Renderer>().sharedMaterials)
            {
                if (mat.name == facialMatName + "_m")
                {
                    SM.GetComponent<Renderer>().materials[i] = faceMat_M;
                }
                else if (mat.name == facialMatName + "_l")
                {
                    SM.GetComponent<Renderer>().materials[i] = faceMat_L;
                }
                i++;
            }

            foreach (AnimationState anim in animTest.GetComponent<Animation>())
            {
                obj.GetComponent<Animation>().AddClip(anim.clip, anim.name);
            }
            //this.facialMaterialSet();
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
        SetAnimation(animationList[curAnim]);
        SetAnimationSpeed(animSpeed);
    }

    void SetInitFacial()
    {
        curFacial = 0;
        curFacialName = "Default";

        var mesh = GameObject.Find(curModelName + "_face").GetComponent<SkinnedMeshRenderer>().sharedMesh;
        facialCount = mesh.blendShapeCount + 1;
    }

    void SetFacialBlendShape(int _i)
    {
        SkinnedMeshRenderer renderer = GameObject.Find(curModelName + "_face").GetComponent<SkinnedMeshRenderer>();
        var mesh = GameObject.Find(curModelName + "_face").GetComponent<SkinnedMeshRenderer>().sharedMesh;
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

    void facialMaterialSet()
    {
        if (curLOD != 0)
        {
            var fName = faceObjName + lodList[curLOD] + "_face";

            int i = 0;
            var faceObj = GameObject.Find(fName);
            //faceSM = faceObj.GetComponentInChildren(typeof(SkinnedMeshRenderer)) as SkinnedMeshRenderer;
            faceSM = faceObj.GetComponent<SkinnedMeshRenderer>();

            foreach (Material mat in faceSM.GetComponent<Renderer>().sharedMaterials)
            {
                if (mat.name == facialMatName + "_m")
                {
                    faceSM.GetComponent<Renderer>().materials[i] = faceMat_M;
                }
                else if (mat.name == facialMatName + "_l")
                {
                    faceSM.GetComponent<Renderer>().materials[i] = faceMat_L;
                }
                i++;
            }
        }
    }
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
    void SetFacialTex(int _i)
    {
        this.facialMaterialSet();

        string file = texturePath + "/" + facialTexList[_i] + lodList[curLOD];
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

    void SetNextMotion(int _add)
    {
        curAnim += _add;
        if (animationList.Length <= curAnim)
        {
            curAnim = 0;
        }
        else if (curAnim < 0)
        {
            curAnim = animationList.Length - 1;
        }
        SetAnimation(animationList[curAnim]);
        SetAnimationSpeed(animSpeed);
    }

    void SetAnimation(string _name)
    {
        if (!string.IsNullOrEmpty(_name))
        {
            print("SetAnimation : " + _name);
            curAnimName = "" + _name;
            obj.GetComponent<Animation>().Play(curAnimName);

            SetFixedFbx(xDoc, obj, curModelName, curAnimName, curLOD);
        }
    }

    void SetInitBackGround()
    {
        //BGObject.SetActive (false);
        //BGEff.SetActive (false);
        //BGPlane.SetActive (false);

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
        var objBill = GameObject.Find("BillBoard") as GameObject;
        /* model set
            if(curBG == 0){
                objBill.renderer.material.mainTexture = Resources.Load( viewerBackGroundPath + "/bg1" ,Texture2D);
                BGObject.SetActive (true);
                BGEff.SetActive (true);
                BGPlane.SetActive (false);
            }else{
                objBill.renderer.material.mainTexture = Resources.Load( viewerBackGroundPath + "/bg0" ,Texture2D);
                BGObject.SetActive (false);
                BGEff.SetActive (false);
                BGPlane.SetActive (true);
            }
        */
        //Textures
        if (!string.IsNullOrEmpty(_name))
        {
            print("SetBackGround : " + _name);
            curBgName = Path.GetFileNameWithoutExtension(_name);
            var loaded = Resources.Load<Texture2D>(_name);
            var obj = GameObject.Find("BillBoard") as GameObject;
            obj.GetComponent<Renderer>().material.mainTexture = loaded;
        }

        //StageTex
        {
            var loaded = Resources.Load<Texture2D>(stageTexList[curBG]);
            //obj = GameObject.Find("Plane") as GameObject;
            planeObj.GetComponent<Renderer>().material.mainTexture = loaded;
        }

        if (curBG == 0)
        {
            planeObj.SetActive(false);
        }
        else
        {
            planeObj.SetActive(true);
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

        /*
        t = "Root/Texture[@Lod=''or@Lod='" + _lod + "'][Info[@Model=''or@Model='" + _model + "'][@Ani=''or@Ani='" + _anim + "']]" ;
        xNodeTex = _xDoc.SelectSingleNode(t);

        if(xNodeTex){ 
            string matname = xNodeTex.Attributes["Material"].InnerText;
            string property = xNodeTex.Attributes["Property"].InnerText;
            string file = xNodeTex.Attributes["File"].InnerText;
            print("Change Texture To "+matname+" : " + property +" : "+file);
            for each( Material mat in SM.renderer.sharedMaterials){
                if(mat){
                if(mat.name ==  matname){
                    Texture2D tex = Resources.Load(file ,Texture2D);
                    mat.SetTexture( property, tex);
                }
                }
            }
        } 
        */

        t = "Root/Animation[@Lod=''or@Lod='" + _lod + "'][Info[@Model=''or@Model='" + _model + "'][@Ani=''or@Ani='" + _anim + "']]";
        xNodeAni = _xDoc.SelectSingleNode(t);

        if (xNodeAni != null)
        {
            string ani = xNodeAni.Attributes["File"].InnerText;
            curAnimName = ani;
            print("Change Animation To " + curAnimName);
            _obj.GetComponent<Animation>().Play(curAnimName);
        }
        t = "Root/Texture[@Lod=''or@Lod='" + _lod + "'][Info[@Model=''or@Model='" + _model + "'][@Ani=''or@Ani='" + _anim + "']]";
        xNodeTex = _xDoc.SelectSingleNode(t);

        if (xNodeTex != null)
        {
            string matname = xNodeTex.Attributes["Material"].InnerText;
            string property = xNodeTex.Attributes["Property"].InnerText;
            string file = xNodeTex.Attributes["File"].InnerText;
            print("Change Texture To " + matname + " : " + property + " : " + file);
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

            obj.transform.position = pos;
            obj.transform.eulerAngles = rot;

            positionY = pos.y;
        }

    }
}