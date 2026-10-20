using UnityEngine;
using UnityEngine.InputSystem;

public class JiaoxiOnsen : MonoBehaviour
{
    [Header("遊戲設定（遊戲數值，非實際泡湯建議）")]
    public float safeMin = 38, safeMax = 42, requiredSeconds = 8;
    public float temperature = 30;
    public float coldForce, hotForce, progress;
    public bool completed, failed;
    public int round;
    public float timeLimit = 90, elapsed, thermalVelocity, disturbance, dangerSeconds;
    public string FailureReason { get; private set; }
    public float Remaining => Mathf.Max(0, timeLimit - elapsed);
    public string Weather => elapsed % 18 < 6 ? "山風降溫" : elapsed % 18 < 12 ? "地熱湧升" : "泉壓波動";
    public Renderer water;
    public Transform coldStream, hotStream;
    float externalCold, externalHot; bool externalInput;
    Font font; Material waterInstance;
    public void SetPedals(float leftBrake, float accelerator) { externalInput = true; externalCold = Mathf.Clamp01(leftBrake); externalHot = Mathf.Clamp01(accelerator); }
    public void ReleasePedals() { externalInput = false; }
    public void Restart() { temperature = 30; progress = 0; completed = failed = false; round = 0; elapsed = thermalVelocity = disturbance = dangerSeconds = coldForce = hotForce = 0; FailureReason = ""; SetRound(); }
    void SetRound() { safeMin = round == 0 ? 38 : round == 1 ? 39 : 40; safeMax = round == 0 ? 42 : round == 1 ? 42 : 42; requiredSeconds = 5 + round * 2; }
    void Start() { Restart(); font = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft JhengHei", "Arial" }, 24); if (water) waterInstance = water.material; }
    void Update()
    {
        var k = Keyboard.current; var g = Gamepad.current;
        if (k != null && k.rKey.wasPressedThisFrame) Restart();
        float c = externalInput ? externalCold : Mathf.Max(k != null && (k.aKey.isPressed || k.leftArrowKey.isPressed) ? 1 : 0, g != null ? g.leftTrigger.ReadValue() : 0);
        float h = externalInput ? externalHot : Mathf.Max(k != null && (k.dKey.isPressed || k.rightArrowKey.isPressed) ? 1 : 0, g != null ? g.rightTrigger.ReadValue() : 0);
        coldForce = Mathf.MoveTowards(coldForce, completed || failed ? 0 : c, Time.deltaTime * 1.8f);
        hotForce = Mathf.MoveTowards(hotForce, completed || failed ? 0 : h, Time.deltaTime * 1.8f);
        if (!completed && !failed) Step(Time.deltaTime, coldForce, hotForce);
        if (coldStream) coldStream.localScale = new Vector3(.08f * coldForce, .7f, .08f * coldForce);
        if (hotStream) hotStream.localScale = new Vector3(.08f * hotForce, .7f, .08f * hotForce);
        if (waterInstance) waterInstance.color = completed ? new Color(.78f,.62f,.22f) : Color.Lerp(new Color(.12f,.49f,.55f), new Color(.43f,.72f,.62f), Mathf.InverseLerp(20,60,temperature));
    }
    public void Step(float dt, float cold, float hot)
    {
        if (completed || failed || dt <= 0) return;
        // Fixed substeps keep thermal inertia and challenge timing stable across frame rates.
        while(dt > .00001f && !completed && !failed) { float step = Mathf.Min(dt, .02f); Tick(step, Mathf.Clamp01(cold), Mathf.Clamp01(hot)); dt -= step; }
    }
    void Tick(float dt, float cold, float hot)
    {
        elapsed += dt;
        float phase = elapsed % 18;
        disturbance = (phase < 6 ? -1.1f : phase < 12 ? 1.3f : Mathf.Sin(elapsed * 2.4f) * 1.1f) * (1 + round * .25f);
        float targetVelocity = hot * 7.5f - cold * 7.5f + disturbance - .15f;
        thermalVelocity = Mathf.Lerp(thermalVelocity, targetVelocity, 1 - Mathf.Exp(-dt / .8f));
        temperature = Mathf.Clamp(temperature + thermalVelocity * dt, 20, 60);
        bool stable = temperature >= safeMin && temperature <= safeMax && cold + hot >= .55f;
        progress = stable ? progress + dt : 0;
        if(progress >= requiredSeconds) { if(round == 2) { progress = requiredSeconds; completed = true; } else { round++; progress = 0; SetRound(); } }
        dangerSeconds = temperature < 25 || temperature > 55 ? dangerSeconds + dt : 0;
        if(!completed && (elapsed >= timeLimit || dangerSeconds >= 3)) { failed = true; FailureReason = dangerSeconds >= 3 ? "水溫失控超過 3 秒" : "調配時間已結束"; }
    }
    void Box(Rect r, Color c) { GUI.color = c; GUI.DrawTexture(r, Texture2D.whiteTexture); GUI.color = Color.white; }
    void Label(Rect r, string s, int size, Color c, TextAnchor anchor = TextAnchor.UpperLeft)
    { GUI.Label(r, s, new GUIStyle(GUI.skin.label) { font = font, fontSize = size, normal = { textColor = c }, alignment = anchor }); }
    void OnGUI()
    {
        var old = GUI.matrix; GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width/1440f, Screen.height/900f,1));
        var cream = new Color(.96f,.93f,.81f); var green = new Color(.43f,.78f,.57f);
        Box(new Rect(35,32,490,145), new Color(.035f,.12f,.12f,.92f));
        Label(new Rect(60,47,440,25), "YILAN  /  SLOW JOURNEY     •     01", 15, green);
        Label(new Rect(58,78,450,52), "礁溪溫泉公園", 38, cream);
        Label(new Rect(60,135,440,30), "森林湯池  /  調配一池黃金溫泉", 18, cream);
        Box(new Rect(380,655,680,218), new Color(.035f,.12f,.12f,.96f));
        Label(new Rect(407,673,410,34), completed ? "黃金溫泉調配成功" : failed ? "挑戰失敗 · " + FailureReason : "第 " + (round+1) + " / 3 階段 · 溫泉調配", failed ? 18 : 24, cream);
        Label(new Rect(830,671,200,40), temperature.ToString("F1") + " °C", 30, completed ? green : cream, TextAnchor.UpperRight);
        float x = 412, y = 731, w = 616;
        Box(new Rect(x,y,w,22), new Color(.23f,.47f,.63f));
        float min = Mathf.InverseLerp(20,60,safeMin), max = Mathf.InverseLerp(20,60,safeMax);
        Box(new Rect(x+w*min,y,w*(max-min),22), green);
        Box(new Rect(x+w*max,y,w*(1-max),22), new Color(.77f,.38f,.25f));
        float needle = x + Mathf.InverseLerp(20,60,temperature)*w;
        Box(new Rect(needle-2,y-8,4,39), cream);
        Label(new Rect(x,y+31,616,27), "20°C                 綠色安全區 " + safeMin + "–" + safeMax + "°C                 60°C", 16, cream);
        Box(new Rect(x,802,w,6), new Color(.18f,.28f,.26f)); Box(new Rect(x,802,w*progress/requiredSeconds,6),green);
        Label(new Rect(x,821,616,30), completed ? "三階段完成！  [ R ] 再玩一次" : failed ? "[ R ] 重新挑戰 · 提早鬆踏板，避免水溫慣性超調" : "連續穩定 " + progress.ToString("F1") + " / " + requiredSeconds.ToString("F0") + " 秒 · 需持續注水 ≥55%", 17,cream);
        Pedal(35,"冷水 / 左煞車", "A / ←  ·  LT",coldForce,new Color(.39f,.72f,.9f));
        Pedal(1100,"熱水 / 油門", "D / →  ·  RT",hotForce,new Color(.94f,.54f,.32f));
        Label(new Rect(1040,42,360,30), "JIAOXI  /  溫泉慢活體驗", 18,cream,TextAnchor.UpperRight);
        Box(new Rect(1090,87,310,183),new Color(.035f,.12f,.12f,.92f));
        Label(new Rect(1110,101,270,37),"剩餘 " + Remaining.ToString("F0") + " 秒",28,Remaining<20?new Color(1,.5f,.3f):cream);
        Label(new Rect(1110,147,280,28), Weather + "  " + disturbance.ToString("+0.0;-0.0;0.0") + "°/s",18,green);
        Label(new Rect(1110,183,280,27),"溫度趨勢 " + thermalVelocity.ToString("+0.0;-0.0;0.0") + "°/s",18,cream);
        Label(new Rect(1110,222,280,30), dangerSeconds>0 ? "水溫失控！ " + dangerSeconds.ToString("F1") + " / 3 秒" : "[ R ] 重玩  ·  三階段挑戰",16, dangerSeconds>0?Color.red:cream);
        Box(new Rect(35,191,490,86),new Color(.035f,.12f,.12f,.9f));
        Label(new Rect(53,201,462,61),"90 秒完成三階段，綠區逐漸縮小。\n水溫有慣性；離開綠區或注水不足會重計。",18,cream);
        GUI.matrix = old;
    }
    void Pedal(float x,string title,string key,float force,Color color)
    {
        Box(new Rect(x,710,305,163),new Color(.035f,.12f,.12f,.94f));
        Label(new Rect(x+22,726,270,35),title,24,color); Label(new Rect(x+22,768,270,25),key,18,Color.white);
        Box(new Rect(x+22,816,260,12),new Color(.18f,.28f,.26f)); Box(new Rect(x+22,816,260*force,12),color);
        Label(new Rect(x+22,840,260,22),"注水力道  " + Mathf.RoundToInt(force*100)+"%",14,Color.white);
    }
}
