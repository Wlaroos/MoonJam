using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticCoroutines
{
	public static IEnumerator Fade(float fadeDuration, SpriteRenderer sr)
	{
		Color initialColor = sr.color;
		Color targetColor = new Color(initialColor.r, initialColor.g, initialColor.b, 0f);
        
		float elapsedTime = 0f;
        
		while (elapsedTime < fadeDuration)
		{
			elapsedTime += Time.deltaTime;
			sr.color = Color.Lerp(initialColor, targetColor, elapsedTime / fadeDuration);
			yield return null;
		}
	}
	
	public static IEnumerator Fade(float fadeDuration, SpriteRenderer sr, Action callback)
	{
		Color initialColor = sr.color;
		Color targetColor = new Color(initialColor.r, initialColor.g, initialColor.b, 0f);
        
		float elapsedTime = 0f;
        
		while (elapsedTime < fadeDuration)
		{
			elapsedTime += Time.deltaTime;
			sr.color = Color.Lerp(initialColor, targetColor, elapsedTime / fadeDuration);
			yield return null;
		}
		
		callback.Invoke();
	}
	
	public static IEnumerator Fade(float fadeDuration, SpriteRenderer sr, Action callbackEnd, Action callbackDuring)
	{
		Color initialColor = sr.color;
		Color targetColor = new Color(initialColor.r, initialColor.g, initialColor.b, 0f);
		
		float elapsedTime = 0f;
        
		while (elapsedTime < fadeDuration)
		{
			elapsedTime += Time.deltaTime;
			sr.color = Color.Lerp(initialColor, targetColor, elapsedTime / fadeDuration);
			callbackDuring.Invoke();
			yield return null;
		}
		
		callbackEnd.Invoke();
	}
}
