using IMFINE.Utils.ConfigManager;
using UnityEngine;

public class ConfigManagerExample : MonoBehaviour
{
	private void Awake()
	{
		ConfigManager.instance.Prepared += OnConfigDataPrepared;
		if (ConfigManager.instance.isPrepared) OnConfigDataPrepared();
	}


	private void OnDestroy()
	{
		if (ConfigManager.instance) ConfigManager.instance.Prepared -= OnConfigDataPrepared;
	}


	private void OnConfigDataPrepared()
	{
		TraceBox.Log("ConfigManagerExample / OnConfigDataPrepared");
		Debug.Log("ConfigManagerExample / OnConfigDataPrepared");
	}
}
