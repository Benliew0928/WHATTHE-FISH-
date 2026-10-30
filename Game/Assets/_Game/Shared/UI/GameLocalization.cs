using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace WhatTheFish {
 public enum GameLanguage { English, SimplifiedChinese, Malay }

 // Presentation is local to this device. Network values and user-authored text are never rewritten.
 public static class GameLocalization {
  [Serializable] public sealed class Entry {public string en,zh,ms;public int[] terms;}
  [Serializable] sealed class Catalog {public Entry[] entries;}
  sealed class Pattern {public Entry entry;public Regex regex;}
  static readonly Dictionary<string,Entry> exact=new(StringComparer.Ordinal);
  static readonly Dictionary<string,List<Pattern>> patterns=new(StringComparer.Ordinal);
  static readonly Dictionary<string,string> cache=new(StringComparer.Ordinal);
  static readonly Dictionary<string,Font> fonts=new(StringComparer.Ordinal);
  static Entry[] fragments=Array.Empty<Entry>();
  static bool initialized;static GameLanguage current;
  public static event Action Changed;
  public static GameLanguage Current {get{Ensure();return current;}}
  public static bool English=>Current==GameLanguage.English;
  public static string Code=>Current==GameLanguage.SimplifiedChinese?"zh-Hans":Current==GameLanguage.Malay?"ms":"en";
  public static string NativeName=>Current==GameLanguage.SimplifiedChinese?"简体中文":Current==GameLanguage.Malay?"Bahasa Melayu":"English";
  static string PreferenceKey {
   get{var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-localizationProfile");return i>=0&&i+1<args.Length?"ui.language.review."+args[i+1]:"ui.language";}
  }
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] static void Boot(){
   Ensure();SceneManager.sceneLoaded-=SceneLoaded;SceneManager.sceneLoaded+=SceneLoaded;
  }
  static void Ensure(){
   if(initialized)return;initialized=true;
   current=Parse(PlayerPrefs.GetString(PreferenceKey,"en"));
   var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-language");if(index>=0&&index+1<args.Length)current=Parse(args[index+1]);
   var data=Resources.Load<TextAsset>("Localization/Catalog");if(!data)return;
   var catalog=JsonUtility.FromJson<Catalog>(data.text);
   foreach(var entry in catalog.entries){
    exact[entry.en]=entry;
    if(!Regex.IsMatch(entry.en,@"\{\d+\}"))continue;
    string key=Bucket(entry.en),expression="\\A";int start=0;
    foreach(Match token in Regex.Matches(entry.en,@"\{(\d+)\}")){
     string capture=entry.en=="{0}s"||entry.en=="{0} m"?@"[0-9]+(?:[.,][0-9]+)?":".*?";
     expression+=Regex.Escape(entry.en.Substring(start,token.Index-start))+"(?<p"+token.Groups[1].Value+">"+capture+")";start=token.Index+token.Length;
    }
    expression+=Regex.Escape(entry.en.Substring(start))+"\\z";
    if(!patterns.TryGetValue(key,out var list))patterns[key]=list=new List<Pattern>();
    list.Add(new Pattern{entry=entry,regex=new Regex(expression,RegexOptions.Singleline|RegexOptions.CultureInvariant)});
   }
   foreach(var list in patterns.Values)list.Sort((a,b)=>b.entry.en.Length.CompareTo(a.entry.en.Length));
   fragments=exact.Values.Where(e=>!e.en.Contains("{")&&e.en.Length>=3&&e.en.Length<220).OrderByDescending(e=>e.en.Length).ToArray();
  }
  static GameLanguage Parse(string value)=>value=="zh-Hans"||value=="zh"||value=="简体中文"?GameLanguage.SimplifiedChinese:value=="ms"||value=="Malay"?GameLanguage.Malay:GameLanguage.English;
  public static void Select(GameLanguage language,bool save=true){
   Ensure();bool changed=current!=language;current=language;cache.Clear();
   if(save){PlayerPrefs.SetString(PreferenceKey,Code);PlayerPrefs.Save();}
   if(changed)Changed?.Invoke();
  }
  public static string Translate(string source){
   Ensure();if(current==GameLanguage.English||string.IsNullOrEmpty(source))return source??"";
   if(cache.TryGetValue(source,out var hit))return hit;
   string result=TranslateCore(source);
   if(cache.Count>4096)cache.Clear();cache[source]=result;return result;
  }
  static string Value(Entry entry)=>current==GameLanguage.SimplifiedChinese?entry.zh:entry.ms;
  static string Bucket(string value){var word=Regex.Match(value,@"\A[\p{L}]+");return word.Success?word.Value:"";}
  static string TranslateCore(string source){
   if(exact.TryGetValue(source,out var entry))return Value(entry);
   if(Regex.IsMatch(source,@"</?(?:b|i|size|color)(?:=[^>]+)?>"))return string.Concat(Regex.Split(source,@"(</?(?:b|i|size|color)(?:=[^>]+)?>)").Select(p=>p.StartsWith("<",StringComparison.Ordinal)?p:Translate(p)));
   foreach(string key in new[]{Bucket(source),""}.Distinct())if(patterns.TryGetValue(key,out var list))foreach(var pattern in list){
    var match=pattern.regex.Match(source);if(!match.Success)continue;
    string translated=Value(pattern.entry);
    return Regex.Replace(translated,@"\{(\d+)\}",token=>{
     int id=int.Parse(token.Groups[1].Value);string argument=match.Groups["p"+id].Value;
     return pattern.entry.terms!=null&&Array.IndexOf(pattern.entry.terms,id)>=0?Translate(argument):argument;
    });
   }
   // Preserve paragraph/scoreboard structure while translating independently authored fragments.
   if(source.IndexOf('\n')>=0)return string.Join("\n",source.Split('\n').Select(Translate));
   foreach(string separator in new[]{"  •  "," • "," · ","  |  ","\t"})if(source.Contains(separator))return string.Join(separator,source.Split(new[]{separator},StringSplitOptions.None).Select(Translate));
   string trimmed=source.Trim();if(trimmed!=source&&exact.TryGetValue(trimmed,out entry))return source.Substring(0,source.IndexOf(trimmed,StringComparison.Ordinal))+Value(entry)+source.Substring(source.IndexOf(trimmed,StringComparison.Ordinal)+trimmed.Length);
   var symbol=Regex.Match(source,@"\A[^\p{L}\p{N}<{}]+(?=\p{L})");if(symbol.Success)return symbol.Value+Translate(source.Substring(symbol.Length));
   // Legacy labels concatenate captions and numbers instead of using format strings.
   // Match only complete boundary fragments, never replace substrings inside words.
   foreach(var part in fragments){
    if(source.Length<=part.en.Length)continue;
    if(source.StartsWith(part.en,StringComparison.Ordinal)&&Boundary(part.en[part.en.Length-1],source[part.en.Length]))return Value(part)+Translate(source.Substring(part.en.Length));
    int offset=source.Length-part.en.Length;
    if(source.EndsWith(part.en,StringComparison.Ordinal)&&Boundary(source[offset-1],part.en[0]))return Translate(source.Substring(0,offset))+Value(part);
   }
   return source;
  }
  static bool Boundary(char a,char b)=>!char.IsLetter(a)||!char.IsLetter(b);
  public static bool HasTranslation(string source){Ensure();return exact.ContainsKey(source);}
  public static string Source(Text label)=>label is LocalizedText localized?localized.Source:label?label.text:"";
  public static void UserContent(Text label,bool raw=true){
   if(label is LocalizedText localized)localized.UserContent=raw;
   else if(label){var binding=label.GetComponent<LocalizedSceneText>()??label.gameObject.AddComponent<LocalizedSceneText>();binding.UserContent=raw;}
  }
  public static Font FontFor(Font original,string value){
   if(string.IsNullOrEmpty(value))return original;
   bool needsFallback=false;for(int i=0;i<value.Length;i++)if(value[i]>=0x2e80&&value[i]<=0xffef){needsFallback=true;break;}
   if(!needsFallback)return original;
   string name=original?original.name:"",role=name.Contains("Barlow")||name.Contains("MenuHeading")?"MenuHeading":name.Contains("Sport")||name=="Display"||name=="Text"?(name.Contains("Display")?"SportDisplay":"SportText"):(name.Contains("Display")?"MenuDisplay":"MenuText");
   if(!fonts.TryGetValue(role,out var font))fonts[role]=font=Resources.Load<Font>("Localization/Fonts/"+role);
   return font?font:original;
  }
  static void SceneLoaded(Scene scene,LoadSceneMode mode){
   foreach(var root in scene.GetRootGameObjects()){
    foreach(var text in root.GetComponentsInChildren<Text>(true))if(!(text is LocalizedText)&&!text.GetComponent<LocalizedSceneText>())text.gameObject.AddComponent<LocalizedSceneText>();
    foreach(var text in root.GetComponentsInChildren<TextMesh>(true))if(!text.GetComponent<LocalizedWorldText>())text.gameObject.AddComponent<LocalizedWorldText>();
   }
  }
 }

 public sealed class LocalizedText:Text {
  bool artwork;
  public bool Artwork {get=>artwork;set{artwork=value;Refresh();}}
  readonly UIVertex[] sharpQuad=new UIVertex[4];float lastRasterScale;
  InputField inputField;
  string fitValue;Vector2 fitBox;int fitMax;GameLanguage fitLanguage;float fitScale;
  public bool ManualFit;
  bool InputGeometry=>inputField&&inputField.textComponent==this;
  // Legacy Text only accounts for Canvas.scaleFactor, not the scaled 800px
  // sports panels. Rasterize at the displayed size, with 2x edge sampling.
  float RasterScale(){
   if(InputGeometry)return pixelsPerUnit;
   float local=canvas?canvas.rootCanvas.transform.InverseTransformVector(rectTransform.TransformVector(Vector3.right)).magnitude:1;
   return Mathf.Clamp(pixelsPerUnit*Mathf.Max(1,local)*2,1,Mathf.Max(1,512f/Mathf.Max(1,fontSize)));
  }
  protected override void OnPopulateMesh(VertexHelper mesh){
   if(InputGeometry){lastRasterScale=pixelsPerUnit;base.OnPopulateMesh(mesh);return;}
   if(!font){mesh.Clear();return;}m_DisableFontTextureRebuiltCallback=true;
   try{
    var settings=GetGenerationSettings(rectTransform.rect.size);settings.scaleFactor=lastRasterScale=RasterScale();
    cachedTextGenerator.PopulateWithErrors(text,settings,gameObject);var vertices=cachedTextGenerator.verts;
    // Font atlas growth can rebuild other labels using Unity UI's shared
    // VertexHelper. Clear after Populate, as Text does, to avoid mixing labels.
    mesh.Clear();
    float units=1/settings.scaleFactor;
    for(int i=0;i<vertices.Count;i++){int n=i&3;sharpQuad[n]=vertices[i];sharpQuad[n].position*=units;if(n==3)mesh.AddUIVertexQuad(sharpQuad);}
   }finally{m_DisableFontTextureRebuiltCallback=false;}
  }
  public string Source {get;private set;}="";
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  public bool HasUnclippedText(){
   var settings=GetGenerationSettings(rectTransform.rect.size);settings.scaleFactor=RasterScale();
   settings.fontSize=resizeTextForBestFit?cachedTextGenerator.fontSizeUsedForBestFit:fontSize;settings.resizeTextForBestFit=false;settings.verticalOverflow=VerticalWrapMode.Overflow;
   using(var full=new TextGenerator()){full.Populate(text,settings);return full.verts.Count<=cachedTextGenerator.verts.Count;}
  }
#endif
  bool basketball;
  public bool Basketball {get=>basketball;set{basketball=value;Refresh();}}
  bool userContent;Font originalFont,appliedFont;
  bool fitting,oldFit;int oldMin,oldMax;
  public bool UserContent {get=>userContent;set{userContent=value;Refresh();}}
  // An unchanged source still needs immediate font reassignment, and this
  // legacy caption derives its translation from the currently selected sport.
  public override string text {get=>base.text;set{if(value==base.text&&Source!=value||value==Source&&font==appliedFont&&value!="Aim and shoot")return;Source=value??"";Refresh();}}
  protected override void OnEnable(){inputField=GetComponentInParent<InputField>();base.OnEnable();GameLocalization.Changed+=Refresh;Refresh();}
  protected override void OnDisable(){GameLocalization.Changed-=Refresh;base.OnDisable();}
  void LateUpdate(){
   if(!Mathf.Approximately(lastRasterScale,RasterScale()))SetVerticesDirty();
   if(font!=appliedFont){originalFont=font;ApplyFont();}
   if(!GameLocalization.English&&!userContent&&!ManualFit&&!InputGeometry){
    if(!fitting&&resizeTextForBestFit)return;
    if(!fitting){oldFit=resizeTextForBestFit;oldMin=resizeTextMinSize;oldMax=resizeTextMaxSize;fitting=true;}
    resizeTextForBestFit=true;resizeTextMinSize=oldFit?oldMin:Mathf.Max(8,Mathf.RoundToInt(fontSize*.68f));resizeTextMaxSize=oldFit?oldMax:fontSize;
   }else if(fitting){resizeTextForBestFit=oldFit;resizeTextMinSize=oldMin;resizeTextMaxSize=oldMax;fitting=false;}
  }
  void ApplyFont(){if(font!=appliedFont||!originalFont)originalFont=font;var next=GameLocalization.FontFor(originalFont,base.text);appliedFont=next;if(font!=next)font=next;}
  // Measure the displayed translation at the same density as its mesh without
  // overwriting the generator used by InputField/caret or the rendered label.
  public Vector2 MeasureInk(int size){var r=MeasureGlyphBounds(size);return new Vector2(r.xMin,r.xMax);}
  // Scrollable documents need the same wrapping density used by OnPopulateMesh.
  // Text.preferredHeight alone uses a different raster scale and can miss a line.
  public float MeasureWrappedHeight(){
   ApplyFont();var settings=GetGenerationSettings(new Vector2(rectTransform.rect.width,100000));
   settings.resizeTextForBestFit=false;settings.scaleFactor=RasterScale();
   settings.horizontalOverflow=HorizontalWrapMode.Wrap;settings.verticalOverflow=VerticalWrapMode.Overflow;
   settings.textAnchor=TextAnchor.UpperLeft;settings.pivot=new Vector2(0,1);
   using(var generator=new TextGenerator()){
    float height=generator.GetPreferredHeight(text,settings)/settings.scaleFactor;
    generator.Populate(text,settings);
    foreach(var vertex in generator.verts)height=Mathf.Max(height,-vertex.position.y/settings.scaleFactor);
    return Mathf.Max(preferredHeight,height);
   }
  }
  Rect MeasureGlyphBounds(int size){
   var settings=GetGenerationSettings(rectTransform.rect.size);settings.fontSize=size;settings.resizeTextForBestFit=false;settings.scaleFactor=RasterScale();settings.horizontalOverflow=HorizontalWrapMode.Overflow;settings.verticalOverflow=VerticalWrapMode.Overflow;
   using(var generator=new TextGenerator()){
    generator.Populate(text,settings);Vector2 low=Vector2.positiveInfinity,high=Vector2.negativeInfinity;var vertices=generator.verts;
    for(int i=0;i+3<vertices.Count;i+=4){Vector2 a=vertices[i].position,b=vertices[i+2].position;if(Mathf.Abs(b.x-a.x)<.001f)continue;low=Vector2.Min(low,Vector2.Min(a,b));high=Vector2.Max(high,Vector2.Max(a,b));}
    return float.IsInfinity(low.x)?new Rect():Rect.MinMaxRect(low.x/settings.scaleFactor,low.y/settings.scaleFactor,high.x/settings.scaleFactor,high.y/settings.scaleFactor);
   }
  }
  public void FitSingleLine(string value,int maxSize){
   ManualFit=true;var box=rectTransform.rect.size;float scale=RasterScale();
   if(fitValue==value&&fitBox==box&&fitMax==maxSize&&fitLanguage==GameLocalization.Current&&Mathf.Approximately(fitScale,scale))return;
   fitValue=value;fitBox=box;fitMax=maxSize;fitLanguage=GameLocalization.Current;fitScale=scale;
   text=value;resizeTextForBestFit=false;horizontalOverflow=HorizontalWrapMode.Overflow;
   Vector2 padding=Vector2.one*2;foreach(var shadow in GetComponents<Shadow>())if(shadow.enabled)padding=Vector2.Max(padding,new Vector2(Mathf.Abs(shadow.effectDistance.x),Mathf.Abs(shadow.effectDistance.y))+Vector2.one);
   var safe=rectTransform.rect;safe.min+=padding;safe.max-=padding;
   for(int size=maxSize;size>=8;size--){fontSize=size;var ink=MeasureGlyphBounds(size);if(ink.xMin>=safe.xMin&&ink.xMax<=safe.xMax&&ink.yMin>=safe.yMin&&ink.yMax<=safe.yMax)break;}
  }
  void Refresh(){
   string caption=Source;
   if(!GameLocalization.English){
    if(Basketball&&(caption=="Shoot"||caption=="SHOOT"))caption="Shoot basket";
    if(caption=="Aim and shoot"&&AppRoot.Instance&&AppRoot.Instance.SelectedSport==SportId.Football)caption="Aim and shoot at goal";
   }
   string translated=userContent?Source:GameLocalization.Translate(caption);
   base.text=artwork&&!GameLocalization.English?ArtworkWords(caption,translated):translated;ApplyFont();
  }
  static string ArtworkWords(string source,string value){
   bool chinese=GameLocalization.Current==GameLanguage.SimplifiedChinese;
   string first=null,second=null;bool goldFirst=false,twoLines=false;
   switch(source){
    case "TROPICAL WIND":first=chinese?"热带":"ANGIN";second=chinese?"强风":"TROPIKA";goldFirst=!chinese;twoLines=true;break;
    case "STICKY GREEN":first=chinese?"黏性":"GREEN";second=chinese?"果岭":"MELEKIT";goldFirst=!chinese;twoLines=!chinese;break;
    case "BOUNCE PARTY":first=chinese?"弹跳":"PARTI";second=chinese?"派对":"LANTUNAN";goldFirst=chinese;twoLines=true;break;
    case "ISLAND RUSH":first=chinese?"海岛":"PECUTAN";second=chinese?"冲刺":"PULAU";goldFirst=!chinese;twoLines=!chinese;break;
    case "CRAB CROSSING":first=chinese?"螃蟹":"LINTASAN";second=chinese?"横行":"KETAM";goldFirst=!chinese;twoLines=true;break;
   }
   if(first==null)return value;
   string a=goldFirst?"FFCC28":"FFF9E3",b=goldFirst?"FFF9E3":"FFCC28";
   return "<color=#"+a+">"+first+"</color>"+(twoLines?"\n":chinese?"":" ")+"<color=#"+b+">"+second+"</color>";
  }
 }

 // Authored world-space labels predate the runtime UI factories.
 public sealed class LocalizedSceneText:MonoBehaviour {
  public bool UserContent;
  Text target;string source,last;Font original,applied;
  void Awake(){target=GetComponent<Text>();source=target.text;original=target.font;}
  void OnEnable(){GameLocalization.Changed+=Refresh;Refresh();}
  void OnDisable(){GameLocalization.Changed-=Refresh;}
  void LateUpdate(){if(target.text!=last){source=target.text;Refresh();}else if(target.font!=applied){original=target.font;Refresh();}}
  void Refresh(){if(!target)return;last=UserContent?source:GameLocalization.Translate(source);target.text=last;target.font=applied=GameLocalization.FontFor(original,last);
   // The authored tee plaque has two intentional lines. Never let a longer
   // translated header wrap onto the course-name line.
   if(!UserContent&&target.name=="Hole and par"){
    var lines=last.Split('\n');if(lines.Length!=2)return;
    string footer=System.Text.RegularExpressions.Regex.Replace(lines[1],"</?size[^>]*>","");
    int Fit(string value,int maximum){var settings=target.GetGenerationSettings(target.rectTransform.rect.size);settings.resizeTextForBestFit=false;settings.horizontalOverflow=HorizontalWrapMode.Overflow;settings.verticalOverflow=VerticalWrapMode.Overflow;settings.fontSize=maximum;float width=target.cachedTextGeneratorForLayout.GetPreferredWidth(value,settings)/target.pixelsPerUnit;return Mathf.Clamp(Mathf.FloorToInt(maximum*(target.rectTransform.rect.width-12)/Mathf.Max(target.rectTransform.rect.width-12,width)),12,maximum);}
    target.horizontalOverflow=HorizontalWrapMode.Overflow;target.resizeTextForBestFit=false;
    last="<size="+Fit(lines[0],44)+">"+lines[0]+"</size>\n<size="+Fit(footer,26)+">"+footer+"</size>";target.text=last;
   }
  }
 }
 public sealed class LocalizedWorldText:MonoBehaviour {
  TextMesh target;string source,last;Font original;
  void Awake(){target=GetComponent<TextMesh>();source=target.text;original=target.font;}
  void OnEnable(){GameLocalization.Changed+=Refresh;Refresh();}
  void OnDisable(){GameLocalization.Changed-=Refresh;}
  void LateUpdate(){if(target.text!=last){source=target.text;Refresh();}}
  void Refresh(){if(!target)return;last=GameLocalization.Translate(source);target.text=last;target.font=GameLocalization.FontFor(original,last);if(target.font)target.GetComponent<MeshRenderer>().sharedMaterial=target.font.material;}
 }
}
