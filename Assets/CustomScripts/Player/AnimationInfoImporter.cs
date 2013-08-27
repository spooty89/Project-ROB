using UnityEngine; 
using System.Collections; 
using System.Collections.Generic; 
using System.Linq;
using System.Xml; 
using System.Xml.Serialization; 
using System.IO; 
using System.Text; 
 
public class AnimationInfoImporter: MonoBehaviour { 
 
   // An example where the encoding can be found is at 
   // http://www.eggheadcafe.com/articles/system.xml.xmlserialization.asp 
   // We will just use the KISS method and cheat a little and use 
   // the examples from the web page since they are fully described 
 
   // This is our local private members 
   static string _FileLocation,_FileName; 
   public static GameObject _Player;
	static Dictionary<string, AnimationClass> myData;
   string _PlayerName; 
   static string _data; 
 
   Vector3 VPosition; 
 
   // When the EGO is instansiated the Start will trigger 
   // so we setup our initial values for our local members 
   void Start () { 
      // Where we want to save and load to and from 
      _FileLocation="\\";
      _FileName="AnimationInfo.xml"; 
 
      // for now, lets just set the name to Joe Schmoe 
      //_PlayerName = "Joe Schmoe"; 
		
		_Player = GameObject.Find( "pav" );
		
		if(_Player.GetComponent<CustomThirdPersonController>().animations != null)
			Debug.Log("correct");
		
		myData = _Player.GetComponent<CustomThirdPersonController>().animations;
	} 
 
   void Update () {} 
	
    [UnityEditor.MenuItem( "Pav/Load Animation Info" )]
	public static void Load()
	{
		LoadXML();
		myData = (Dictionary<string, AnimationClass>)DeserializeObject(_data);
		_Player.GetComponent<CustomThirdPersonController>().animations = myData;
	}
	
    [UnityEditor.MenuItem( "Pav/Save Animation Info" )]
	public static void SaveAnimationInfo()
	{
		myData = _Player.GetComponent<CustomThirdPersonController>().animations;
		// Time to creat our XML! 
		_data = SerializeObject(myData); 
		// This is the final resulting XML from the serialization process 
		CreateXML(); 
		Debug.Log(_data); 
	}
 
   /* The following metods came from the referenced URL */ 
   static string UTF8ByteArrayToString(byte[] characters) 
   {      
      UTF8Encoding encoding = new UTF8Encoding(); 
      string constructedString = encoding.GetString(characters); 
      return (constructedString); 
   } 
 
   static byte[] StringToUTF8ByteArray(string pXmlString) 
   { 
      UTF8Encoding encoding = new UTF8Encoding(); 
      byte[] byteArray = encoding.GetBytes(pXmlString); 
      return byteArray; 
   } 
 
   // Here we serialize our UserData object of myData 
   static string SerializeObject(Dictionary<string, AnimationClass> pObject) 
   { 
      string XmlizedString = null; 
      MemoryStream memoryStream = new MemoryStream(); 
      XmlSerializer xs = new XmlSerializer(typeof(item[]), new XmlRootAttribute() { ElementName = "items" }); 
      XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8); 
      xs.Serialize(xmlTextWriter, pObject.Select(i => new item(){id = i.Key, name = i.Value.name, speed = i.Value.speed, wrap = i.Value.wrap, cross = i.Value.crossfade}).ToArray()); 
      memoryStream = (MemoryStream)xmlTextWriter.BaseStream; 
      XmlizedString = UTF8ByteArrayToString(memoryStream.ToArray()); 
      return XmlizedString; 
   } 
 
   // Here we deserialize it back into its original form 
   static object DeserializeObject(string pXmlizedString) 
   { 
      XmlSerializer xs = new XmlSerializer(typeof(item[]), new XmlRootAttribute() { ElementName = "items" }); 
      MemoryStream memoryStream = new MemoryStream(StringToUTF8ByteArray(pXmlizedString)); 
      XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8); 
		item[] tempItem = (item[])xs.Deserialize(memoryStream);
		Dictionary<string, AnimationClass> tempAC = new Dictionary<string, AnimationClass>();
		foreach(item i in tempItem)
		{
			tempAC.Add(i.id, new AnimationClass(i.name, i.speed, i.wrap, i.cross));
		}
      return tempAC; 
   } 
 
   // Finally our save and load methods for the file itself 
   static void CreateXML() 
   { 
      StreamWriter writer; 
      FileInfo t = new FileInfo(_FileName); 
      if(!t.Exists) 
      { 
         writer = t.CreateText(); 
      } 
      else 
      { 
         t.Delete(); 
         writer = t.CreateText(); 
      } 
      writer.Write(_data); 
      writer.Close(); 
      Debug.Log("File written."); 
   } 
 
   static void LoadXML() 
   { 
      StreamReader r = File.OpenText(_FileName); 
      string _info = r.ReadToEnd(); 
      r.Close(); 
      _data=_info; 
      Debug.Log("File Read"); 
   } 
} 

public class item
{
    [XmlAttribute]
    public string id;
    [XmlAttribute]
    public string name;
    [XmlAttribute]
    public float speed;
    [XmlAttribute]
    public WrapMode wrap;
    [XmlAttribute]
    public float cross;
}