/*using UnityEngine;
using UnityEditor;
using System.Collections; 
using System.Collections.Generic; 
using System.Linq;
using System.Xml; 
using System.Xml.Serialization; 
using System.IO; 
using System.Text; 
 
public class AnimationInfoImporter { 
 
	// An example where the encoding can be found is at 
   // http://www.eggheadcafe.com/articles/system.xml.xmlserialization.asp 
   // We will just use the KISS method and cheat a little and use 
   // the examples from the web page since they are fully described 
 
	// This is our local private members 
	static string _FileLocation, _FileName; 
	public static GameObject _Player;
	static Dictionary<string, LesserAnimationClass> myData;
	string _PlayerName; 
	static string _data;
 
   // When the EGO is instansiated the Start will trigger 
   // so we setup our initial values for our local members 
	static void Setup () { 
      	// Where we want to save and load to and from 
      	_FileLocation = EditorPrefs.GetString("Pav-AnimationInfoImporter-LastPath");
      	_FileName="AnimationInfo.xml"; 
		_Player = GameObject.Find( "Pav" );
		if (_Player == null)
			_Player = GameObject.Find( "pav" );
	} 
	
    //[UnityEditor.MenuItem( "Pav/Load Animation Info" )]
	public static void Load()
	{
      	//_FileName="AnimationInfo.xml"; 
		Setup();
		LoadXML();
		myData = (Dictionary<string, LesserAnimationClass>)DeserializeObject(_data);
		_Player.GetComponent<CustomThirdPersonController>().animations = myData;
	}
	
    //[UnityEditor.MenuItem( "Pav/Save Animation Info" )]
	public static void SaveAnimationInfo()
	{
		Setup();
		myData = _Player.GetComponent<CustomThirdPersonController>().animations;
		// Time to creat our XML! 
		_data = SerializeObject(myData); 
		// This is the final resulting XML from the serialization process 
		CreateXML();
	}
 
	/* The following metods came from the referenced URL */ 
/*	static string UTF8ByteArrayToString(byte[] characters) 
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
	static string SerializeObject(Dictionary<string, LesserAnimationClass> pObject) 
	{ 
		string XmlizedString = null; 
		MemoryStream memoryStream = new MemoryStream(); 
		XmlSerializer xs = new XmlSerializer(typeof(item[]), new XmlRootAttribute() { ElementName = "items" }); 
		XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8); 
		xmlTextWriter.Settings.Indent = true;
		xmlTextWriter.Settings.NewLineOnAttributes = true;
		xs.Serialize(xmlTextWriter, pObject.Select(i => new item(){
																		id = i.Key,
																		name = i.Value.name,
																		speed = i.Value.speed,
																		wrap = i.Value.wrap,
																		cross = i.Value.crossfade,
																		state = i.Value.state
																	}).ToArray()); 
		memoryStream = (MemoryStream)xmlTextWriter.BaseStream; 
		XmlizedString = UTF8ByteArrayToString(memoryStream.ToArray()); 
		return XmlizedString; 
	} 
 
	// Here we deserialize it back into its original form 
	static object DeserializeObject(string pXmlizedString) 
	{ 
		XmlSerializer xs = new XmlSerializer(typeof(item[]), new XmlRootAttribute() { ElementName = "items" }); 
		MemoryStream memoryStream = new MemoryStream(StringToUTF8ByteArray(pXmlizedString)); 
		//XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8); 
		item[] tempItem = (item[])xs.Deserialize(memoryStream);
		Dictionary<string, LesserAnimationClass> tempAC = new Dictionary<string, LesserAnimationClass>();
		foreach(item i in tempItem)
		{
			tempAC.Add(i.id, new LesserAnimationClass(i.name, i.speed, i.wrap, i.cross, i.state));
		}
		return tempAC; 
	} 
 
	// Finally our save and load methods for the file itself 
	static void CreateXML() 
	{ 
		StreamWriter writer; 
		FileInfo t = new FileInfo( _FileLocation + "/" + _FileName ); 
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
	} 
 
	static void LoadXML() 
	{ 
		StreamReader r = File.OpenText( _FileLocation + "/" + _FileName ); 
		string _info = r.ReadToEnd(); 
		r.Close(); 
		_data=_info; 
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
    [XmlAttribute]
    public string state;
}*/