using UnityEditor;
using System.IO;

public class AnimationInfoPath : EditorWindow
{
    const string LastPathKey = "Pav-AnimationInfoImporter-LastPath";
	public static string path;
	
    //[UnityEditor.MenuItem( "Pav/Animation Info Path" )]
    public static void AnimationImport()
    {
        path = Path.GetFullPath( EditorPrefs.GetString( LastPathKey, "Animation Info" ) );
        path = EditorUtility.OpenFolderPanel( "Choose data folder", Path.GetDirectoryName( path ), "" );
        if( !string.IsNullOrEmpty( path ) )
        {
            EditorPrefs.SetString( LastPathKey, path );
        }
    }
}

