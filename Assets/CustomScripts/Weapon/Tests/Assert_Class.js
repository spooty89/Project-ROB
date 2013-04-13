#pragma strict

function Start () {

}

function Update () {

}

public class Assert{
	public static function assertTrue(e : boolean, s : String) : void{
		if (!e)
			throw UnityException("Assertion Error: " + s);
		}
		
	public static function assertFalse(e : boolean, s : String) : void{
		if (e)
			throw UnityException("Assertion Error: " + s);
		}
}

