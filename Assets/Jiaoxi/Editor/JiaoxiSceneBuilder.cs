using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class JiaoxiSceneBuilder
{
    static Transform root;
    static Material Mat(string name, Color color)
    {
        string path = "Assets/Jiaoxi/"+name+".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!m) { m = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(m,path); }
        m.color = color; m.SetFloat("_Smoothness",.25f); return m;
    }
    static GameObject P(string n, PrimitiveType type, Vector3 pos, Vector3 scale, Material m)
    {
        var o = GameObject.CreatePrimitive(type); o.name=n; o.transform.SetParent(root); o.transform.position=pos; o.transform.localScale=scale; o.GetComponent<Renderer>().sharedMaterial=m; return o;
    }
    static GameObject Cube(string n, Vector3 p, Vector3 s, Material m) { return P(n,PrimitiveType.Cube,p,s,m); }
    [MenuItem("Jiaoxi/Build Onsen Scene")]
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        root = new GameObject("Jiaoxi Onsen Park").transform;
        var game = root.gameObject.AddComponent<JiaoxiOnsen>();
        var stone=Mat("Basalt",new Color(.24f,.29f,.28f)); var wood=Mat("Cedar",new Color(.32f,.18f,.09f));
        var ground=Mat("Moss",new Color(.14f,.23f,.16f)); var leaf=Mat("Canopy",new Color(.16f,.35f,.22f));
        var leaf2=Mat("Jade",new Color(.26f,.43f,.25f)); var gold=Mat("Lantern",new Color(1,.73f,.32f));
        var water=Mat("OnsenWater",new Color(.18f,.57f,.59f)); water.SetFloat("_Smoothness",.92f);
        var cold=Mat("ColdWater",new Color(.35f,.77f,.91f)); var hot=Mat("HotWater",new Color(.94f,.61f,.33f));
        Cube("Forest floor",new Vector3(0,-.3f,0),new Vector3(40,.4f,38),ground);
        Cube("Pool foundation",new Vector3(0,0,0),new Vector3(11,.55f,7),stone);
        game.water=Cube("Thermal water",new Vector3(0,.35f,0),new Vector3(9.6f,.12f,5.6f),water).GetComponent<Renderer>();
        for(int i=0;i<12;i++) for(int side=-1;side<=1;side+=2) Cube("Stone pool rim",new Vector3(-5.05f+i*.92f,.5f,side*3.1f),new Vector3(.87f,.65f,.7f),stone);
        for(int i=0;i<6;i++) for(int side=-1;side<=1;side+=2) Cube("Stone pool rim",new Vector3(side*5.15f,.5f,-2.5f+i),new Vector3(.7f,.65f,.93f),stone);
        for(int i=0;i<28;i++) Cube("Cedar deck plank",new Vector3(-6.75f+i*.5f,.15f,5.2f),new Vector3(.47f,.24f,3.2f),wood);
        for(int side=-1;side<=1;side+=2) {
            Cube("Footbath bench",new Vector3(side*5.9f,.9f,0),new Vector3(.9f,.22f,6),wood);
            for(int z=-2;z<=2;z+=4) Cube("Bench leg",new Vector3(side*5.9f,.45f,z),new Vector3(.3f,.8f,.3f),wood);
            Cube("Water spout",new Vector3(side*3,1.9f,2.5f),new Vector3(.3f,.3f,1.4f),wood);
            Cube("Spout post",new Vector3(side*3,1.1f,3),new Vector3(.25f,2,.25f),wood);
            var stream=P(side<0?"Cold stream":"Hot stream",PrimitiveType.Cylinder,new Vector3(side*3,1.05f,1.9f),new Vector3(.08f,.7f,.08f),side<0?cold:hot).transform;
            if(side<0) game.coldStream=stream; else game.hotStream=stream;
        }
        var rng=new System.Random(82);
        for(int i=0;i<65;i++) {
            float x=(float)rng.NextDouble()*36-18, z=(float)rng.NextDouble()*30-9;
            if(z < 5 || (Mathf.Abs(x)<10 && z<10)) continue;
            float height=4+(float)rng.NextDouble()*4;
            P("Cedar trunk",PrimitiveType.Cylinder,new Vector3(x,height/2,z),new Vector3(.35f,height/2,.35f),wood);
            for(int j=0;j<3;j++) P("Forest canopy",PrimitiveType.Sphere,new Vector3(x,height+j*.7f,z),new Vector3(3.8f-j*.7f,2.5f,3.8f-j*.7f),i%2==0?leaf:leaf2);
        }
        for(int i=0;i<12;i++) {
            float x=i%2==0?-7.4f:7.4f,z=-3+i/2*2.2f;
            Cube("Lantern pedestal",new Vector3(x,.6f,z),new Vector3(.5f,1.2f,.5f),stone);
            Cube("Warm lantern",new Vector3(x,1.4f,z),new Vector3(.65f,.65f,.65f),gold);
            Cube("Lantern cap",new Vector3(x,1.83f,z),new Vector3(.95f,.18f,.95f),wood);
            var l=new GameObject("Lantern glow").AddComponent<Light>(); l.transform.SetParent(root); l.transform.position=new Vector3(x,1.7f,z-.4f); l.type=LightType.Point;l.color=new Color(1,.65f,.28f);l.intensity=2;l.range=4;
        }
        // Japanese pavilion silhouette at the far end of the pool.
        for(int side=-1;side<=1;side+=2) Cube("Pavilion pillar",new Vector3(side*4,2.4f,7),new Vector3(.35f,4.5f,.35f),wood);
        Cube("Pavilion lintel",new Vector3(0,4.5f,7),new Vector3(9.5f,.4f,.5f),wood);
        for(int side=-1;side<=1;side+=2) {var roof=Cube("Sloped tiled roof",new Vector3(0,5,7+side*.9f),new Vector3(11,.24f,2.2f),stone);roof.transform.rotation=Quaternion.Euler(side*20,0,0);}
        var sign=Cube("Onsen sign",new Vector3(0,3.65f,6.7f),new Vector3(3.7f,.9f,.16f),wood);
        var label=new GameObject("湯 sign").AddComponent<TextMesh>();label.transform.SetParent(root); label.transform.position=new Vector3(0,3.65f,6.58f);label.transform.rotation=Quaternion.identity;label.text="J I A O X I   /   O N S E N";label.fontSize=48;label.characterSize=.065f;label.anchor=TextAnchor.MiddleCenter;label.color=new Color(1,.85f,.55f);
        var steamShader=Shader.Find("Universal Render Pipeline/Particles/Unlit");
        var steamMat=new Material(steamShader); steamMat.name="Steam";steamMat.SetFloat("_Surface",1);steamMat.SetFloat("_Blend",0);steamMat.SetFloat("_SrcBlend",5);steamMat.SetFloat("_DstBlend",10);steamMat.SetFloat("_ZWrite",0);steamMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");steamMat.renderQueue=3000;
        var tex=new Texture2D(64,64);tex.name="SteamSoft";for(int y=0;y<64;y++)for(int x=0;x<64;x++){float d=Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f))/31.5f;tex.SetPixel(x,y,new Color(1,1,1,Mathf.Pow(Mathf.Clamp01(1-d),2)*.25f));}tex.Apply();
        if (!AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Jiaoxi/SteamSoft.asset")) AssetDatabase.CreateAsset(tex,"Assets/Jiaoxi/SteamSoft.asset"); else { Object.DestroyImmediate(tex); tex=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Jiaoxi/SteamSoft.asset"); }steamMat.SetTexture("_BaseMap",tex);if (!AssetDatabase.LoadAssetAtPath<Material>("Assets/Jiaoxi/Steam.mat")) AssetDatabase.CreateAsset(steamMat,"Assets/Jiaoxi/Steam.mat"); else { Object.DestroyImmediate(steamMat); steamMat=AssetDatabase.LoadAssetAtPath<Material>("Assets/Jiaoxi/Steam.mat"); }
        var ps=new GameObject("Slow thermal mist").AddComponent<ParticleSystem>();ps.transform.SetParent(root);ps.transform.position=new Vector3(0,.45f,0);ps.transform.rotation=Quaternion.Euler(-90,0,0);
        var main=ps.main;main.startLifetime=5;main.startSpeed=.3f;main.startSize=2.2f;main.startColor=new Color(.83f,.92f,.87f,.35f);main.maxParticles=90;main.simulationSpace=ParticleSystemSimulationSpace.World;
        var emission=ps.emission;emission.rateOverTime=13;var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(8,4,.1f);
        ps.GetComponent<ParticleSystemRenderer>().sharedMaterial=steamMat;
        var cam=new GameObject("Main Camera").AddComponent<Camera>();cam.tag="MainCamera";cam.transform.position=new Vector3(12,10,-16);cam.transform.LookAt(new Vector3(0,1.5f,1.5f));cam.fieldOfView=48;cam.backgroundColor=new Color(.12f,.22f,.2f);cam.clearFlags=CameraClearFlags.SolidColor;cam.gameObject.AddComponent<AudioListener>();
        var sun=new GameObject("Sun").AddComponent<Light>();sun.type=LightType.Directional;sun.transform.rotation=Quaternion.Euler(42,-25,0);sun.color=new Color(1,.87f,.65f);sun.intensity=1.5f;sun.shadows=LightShadows.Soft;
        RenderSettings.ambientLight=new Color(.4f,.53f,.47f);RenderSettings.fog=true;RenderSettings.fogColor=new Color(.21f,.34f,.29f);RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.021f;
        EditorSceneManager.SaveScene(scene,"Assets/Jiaoxi/JiaoxiOnsen.unity");JiaoxiGarden.Enrich();AssetDatabase.SaveAssets();Selection.activeGameObject=root.gameObject;
    }
}


