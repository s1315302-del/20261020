using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class JiaoxiGarden
{
    static Transform root;
    static Material M(string name, Color c) { string p="Assets/Jiaoxi/"+name+".mat"; var m=AssetDatabase.LoadAssetAtPath<Material>(p); if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,p);}m.color=c;return m; }
    static GameObject P(string name,Vector3 pos,Vector3 scale,Material mat,PrimitiveType type=PrimitiveType.Cube) { var o=GameObject.CreatePrimitive(type);o.name=name;o.transform.SetParent(root);o.transform.position=pos;o.transform.localScale=scale;o.GetComponent<Renderer>().sharedMaterial=mat;return o; }
    public static void Enrich()
    {
        var old=GameObject.Find("Garden enrichment"); if(old) Object.DestroyImmediate(old);
        root=new GameObject("Garden enrichment").transform;
        var moss=M("GardenMoss",new Color(.25f,.36f,.2f));var rock=M("GardenRock",new Color(.36f,.4f,.35f));var sand=M("GardenSand",new Color(.6f,.57f,.43f));
        var wood=M("GardenCedar",new Color(.35f,.19f,.1f));var red=M("Vermilion",new Color(.65f,.18f,.1f));var bamboo=M("Bamboo",new Color(.35f,.49f,.19f));var leaves=M("BambooLeaves",new Color(.19f,.37f,.16f));
        var pink=M("Azalea",new Color(.83f,.36f,.48f));var cream=M("Plaster",new Color(.82f,.77f,.61f));var tile=M("RoofTiles",new Color(.17f,.24f,.25f));var stream=M("Creek",new Color(.16f,.43f,.46f));
        P("Extended landscape",new Vector3(0,-.52f,20),new Vector3(140,.4f,145),moss);
        // Layered ridges close the horizon, keeping the pool unobstructed.
        for(int layer=0;layer<3;layer++) {
            var mountain=M("Mountain"+layer,Color.Lerp(new Color(.24f,.38f,.31f),new Color(.44f,.54f,.47f),layer*.3f));
            for(int i=0;i<7;i++) {var hill=P("Distant mountain ridge",new Vector3(-57+i*19+layer*5,1,38+layer*17),new Vector3(31,20+(i%3)*7-layer*2,26),mountain,PrimitiveType.Sphere); hill.transform.rotation=Quaternion.Euler(0,i*23,10*(i%2));}
        }
        for(int i=0;i<26;i++) {
            float z=-12+i*1.1f;float x=-9+Mathf.Sin(i*.19f)*1.4f;
            var s=P("Winding stepping stone",new Vector3(x,-.03f,z),new Vector3(1.7f,.2f,.85f),sand,PrimitiveType.Cylinder);s.transform.rotation=Quaternion.Euler(0,i*19,0);
        }
        // Stream and wooden footbridge to the left of the bath.
        for(int i=0;i<20;i++) {float z=-10+i*1.5f;float x=-13+Mathf.Sin(i*.4f);P("Forest creek",new Vector3(x,-.16f,z),new Vector3(2.6f,.08f,1.7f),stream);for(int side=-1;side<=1;side+=2)P("Creek bank",new Vector3(x+side*1.5f,-.02f,z),new Vector3(.8f,.55f,1.4f),rock,PrimitiveType.Sphere);}
        for(int i=0;i<12;i++)P("Bridge plank",new Vector3(-15+i*.36f,.45f,2),new Vector3(.32f,.18f,2.1f),wood);
        for(int side=-1;side<=1;side+=2){P("Bridge handrail",new Vector3(-13,1.3f,2+side),new Vector3(4.6f,.12f,.12f),red);for(int i=0;i<4;i++)P("Bridge post",new Vector3(-15+i*1.4f,.9f,2+side),new Vector3(.15f,1.3f,.15f),red);}
        // Bamboo grove, rocks, flower clusters and low shrubs.
        var rng=new System.Random(314);
        for(int i=0;i<58;i++) {
            float x=i%2==0?-17-(float)rng.NextDouble()*6:12+(float)rng.NextDouble()*9;float z=5+(float)rng.NextDouble()*21;float h=3+(float)rng.NextDouble()*3;
            P("Bamboo stalk",new Vector3(x,h/2,z),new Vector3(.13f,h/2,.13f),bamboo,PrimitiveType.Cylinder);
            for(int j=0;j<3;j++){P("Bamboo node",new Vector3(x,h*.25f+j*.9f,z),new Vector3(.18f,.045f,.18f),cream,PrimitiveType.Cylinder);var leaf=P("Bamboo foliage",new Vector3(x+(j%2==0?.5f:-.5f),h-.5f+j*.4f,z),new Vector3(1.7f,.18f,.7f),leaves,PrimitiveType.Sphere);leaf.transform.rotation=Quaternion.Euler(0,i*31,j%2==0?25:-25);}
        }
        for(int i=0;i<75;i++) {
            float x=-20+(float)rng.NextDouble()*39,z=-8+(float)rng.NextDouble()*33;
            if(Mathf.Abs(x)<8 && z<10)continue;
            float size=.5f+(float)rng.NextDouble()*.8f;
            P("Mossy garden stone",new Vector3(x,.13f,z),new Vector3(size,.5f,size*.8f),rock,PrimitiveType.Sphere);
            P("Azalea shrub",new Vector3(x+.4f,.25f,z+.35f),new Vector3(1,.65f,1),leaves,PrimitiveType.Sphere);
            for(int j=0;j<3;j++)P("Azalea blossom",new Vector3(x+.1f+j*.27f,.55f,z+.35f),new Vector3(.24f,.18f,.25f),pink,PrimitiveType.Sphere);
        }
        // Entrance gate and small tea house behind the pool.
        for(int side=-1;side<=1;side+=2)P("Torii pillar",new Vector3(-10+side*2,2.1f,13),new Vector3(.4f,4.3f,.4f),red,PrimitiveType.Cylinder);
        P("Torii lintel",new Vector3(-10,4.3f,13),new Vector3(5.9f,.35f,.6f),tile);P("Torii lower beam",new Vector3(-10,3.4f,13),new Vector3(5,.22f,.3f),red);
        P("Tea house foundation",new Vector3(-2,.2f,16),new Vector3(8,.6f,4),rock);P("Tea house wall",new Vector3(-2,1.9f,17),new Vector3(7,3.2f,.3f),cream);
        for(int i=0;i<8;i++){P("Shoji frame",new Vector3(-5.5f+i,1.9f,16.75f),new Vector3(.07f,3.2f,.12f),wood);P("Veranda plank",new Vector3(-5.5f+i,.6f,15.4f),new Vector3(.95f,.18f,2.5f),wood);}
        for(int i=0;i<4;i++)P("Shoji crossbar",new Vector3(-2,.7f+i*.85f,16.7f),new Vector3(7,.06f,.12f),wood);
        for(int side=-1;side<=1;side+=2){var roof=P("Tea house roof",new Vector3(-2,4.2f,17+side*1.3f),new Vector3(9,.2f,3),tile);roof.transform.rotation=Quaternion.Euler(side*22,0,0);}
        // Fence gives the midground a clear boundary.
        for(int i=0;i<23;i++){P("Garden fence post",new Vector3(-22+i*2,1,24),new Vector3(.16f,2,.16f),wood);if(i<22)for(int j=0;j<2;j++)P("Garden fence rail",new Vector3(-21+i*2,.7f+j*.8f,24),new Vector3(2,.12f,.12f),wood);}
        var cam=Camera.main;cam.transform.position=new Vector3(13,12,-20);cam.transform.LookAt(new Vector3(0,2,4));cam.fieldOfView=49;cam.backgroundColor=new Color(.46f,.59f,.54f);
        RenderSettings.fogDensity=.012f;RenderSettings.fogColor=new Color(.4f,.52f,.45f);
        var mist=GameObject.Find("Slow thermal mist"); if(mist){var ps=mist.GetComponent<ParticleSystem>();var main=ps.main;main.startColor=new Color(.85f,.95f,.91f,.7f);main.startSize=2.6f;main.prewarm=true;}
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();
    }
}
