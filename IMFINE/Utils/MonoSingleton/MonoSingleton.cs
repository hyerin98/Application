namespace IMFINE.Utils
{
	using UnityEngine;

	public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
	{
		private static T _instance;

		public static T instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = FindObjectOfType<T>();
				}
				return _instance;
			}
		}
	}
}