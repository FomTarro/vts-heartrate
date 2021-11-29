using UnityEngine;
using System.Collections;

public class AnimatedUVs : MonoBehaviour {

	public Material _mat; 
	public Vector2 uvAnimationRate = new Vector2( 0.25f, 0.25f ); 
	public string textureName = "_MainTex";

	Vector2 uvOffset = Vector2.zero;
	
	void LateUpdate() 
	{
        uvOffset += (uvAnimationRate * Time.deltaTime);
        //uvOffset = new Vector2(0.25f/2 * Jukebox.Progression, 0);
		if(this._mat != null)
		{
			this._mat.SetTextureOffset(textureName, uvOffset);
		}
	}
}
