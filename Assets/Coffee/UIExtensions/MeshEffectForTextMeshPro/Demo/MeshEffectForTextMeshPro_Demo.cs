using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Coffee.UIExtensions
{
	public class MeshEffectForTextMeshPro_Demo : MonoBehaviour
	{
		public void ChangeFontMaterial (Material material)
		{
			foreach (var tmp in GetComponentsInChildren<TMP_Text> ())
			{
				tmp.fontSharedMaterial = material;
			}
		}
	}
}