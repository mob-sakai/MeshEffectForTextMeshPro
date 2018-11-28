using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace Coffee.UIExtensions
{
	/// <summary>
	/// Base class for effects that modify the generated Mesh.
	/// It works well not only for standard Graphic components (Image, RawImage, Text, etc.) but also for TextMeshPro and TextMeshProUGUI.
	/// </summary>
	[ExecuteInEditMode]
	public abstract class BaseMeshEffect : UIBehaviour, IMeshModifier
	{
		//################################
		// Constant or Static Members.
		//################################
		static readonly List<Vector2> s_Uv0 = new List<Vector2> ();
		static readonly List<Vector2> s_Uv1 = new List<Vector2> ();
		static readonly List<Vector3> s_Vertices = new List<Vector3> ();
		static readonly List<int> s_Indices = new List<int> ();
		static readonly List<Vector3> s_Normals = new List<Vector3> ();
		static readonly List<Vector4> s_Tangents = new List<Vector4> ();
		static readonly List<Color32> s_Colors = new List<Color32> ();
		static readonly VertexHelper s_VertexHelper = new VertexHelper ();
		static readonly List<TMP_SubMeshUI> s_SubMeshUIs = new List<TMP_SubMeshUI> ();
		static readonly List<Mesh> s_Meshes = new List<Mesh> ();


		//################################
		// Public Members.
		//################################
		/// <summary>
		/// The Graphic attached to this GameObject.
		/// </summary>
		public Graphic graphic { get { return _graphic ?? (_graphic = GetComponent<Graphic> ()); } }

		/// <summary>
		/// The CanvasRenderer attached to this GameObject.
		/// </summary>
		public CanvasRenderer canvasRenderer { get { return _canvasRenderer ?? (_canvasRenderer = GetComponent<CanvasRenderer> ()); } }

		/// <summary>
		/// The TMP_Text attached to this GameObject.
		/// </summary>
		public TMP_Text textMeshPro { get { return _textMeshPro ?? (_textMeshPro = GetComponent<TMP_Text> ()); } }

		/// <summary>
		/// Call used to modify mesh. (legacy)
		/// </summary>
		/// <param name="mesh">Mesh.</param>
		public virtual void ModifyMesh (Mesh mesh)
		{
		}

		/// <summary>
		/// Call used to modify mesh.
		/// </summary>
		/// <param name="vh">VertexHelper.</param>
		public virtual void ModifyMesh (VertexHelper vh)
		{
		}

		/// <summary>
		/// Mark the vertices as dirty.
		/// </summary>
		public virtual void SetVerticesDirty ()
		{
			if (textMeshPro)
			{
				//Debug.Log ("SetVerticesDirty");
				//_havePropChanged = true;
				//textMeshPro.isRightToLeftText
				foreach (var info in textMeshPro.textInfo.meshInfo)
				{
					var mesh = info.mesh;
					if (mesh)
					{
						mesh.Clear ();
						mesh.vertices = info.vertices;
						mesh.uv = info.uvs0;
						mesh.uv2 = info.uvs2;
						mesh.colors32 = info.colors32;
						mesh.normals = info.normals;
						mesh.tangents = info.tangents;
						mesh.triangles = info.triangles;
					}
				}

				if (canvasRenderer)
				{
					canvasRenderer.SetMesh (_textMeshPro.mesh);

					GetComponentsInChildren (false, s_SubMeshUIs);
					foreach (var sm in s_SubMeshUIs)
					{
						sm.canvasRenderer.SetMesh (sm.mesh);
					}
					s_SubMeshUIs.Clear ();
				}
				textMeshPro.havePropertiesChanged = true;
			}
			else if (graphic)
			{
				graphic.SetVerticesDirty ();
			}
		}


		//################################
		// Protected Members.
		//################################
		/// <summary>
		/// Should the effect modify the mesh directly for TMPro?
		/// </summary>
		protected virtual bool isLegacyMeshModifier { get { return false; } }

		/// <summary>
		/// This function is called when the object becomes enabled and active.
		/// </summary>
		protected override void OnEnable ()
		{
			if (textMeshPro)
			{

				TMPro_EventManager.TEXT_CHANGED_EVENT.Add (OnTextChanged);
				//#if UNITY_EDITOR
				//				TMPro_EventManager.TEXTMESHPRO_PROPERTY_EVENT.Add (OnTextChanged);
				//				TMPro_EventManager.TEXTMESHPRO_UGUI_PROPERTY_EVENT.Add (OnTextChanged);
				//#endif
			}
			SetVerticesDirty ();
		}

		/// <summary>
		/// This function is called when the behaviour becomes disabled () or inactive.
		/// </summary>
		protected override void OnDisable ()
		{
			TMPro_EventManager.TEXT_CHANGED_EVENT.Remove (OnTextChanged);
			//#if UNITY_EDITOR
			//			TMPro_EventManager.TEXTMESHPRO_PROPERTY_EVENT.Remove (OnTextChanged);
			//			TMPro_EventManager.TEXTMESHPRO_UGUI_PROPERTY_EVENT.Remove (OnTextChanged);
			//#endif
			SetVerticesDirty ();
		}


		/// <summary>
		/// LateUpdate is called every frame, if the Behaviour is enabled.
		/// </summary>
		protected virtual void LateUpdate ()
		{
			var t = _textMeshPro;
			if (t)
			{
				if (t.havePropertiesChanged || _isTextMeshProActive != t.isActiveAndEnabled)
				{
					SetVerticesDirty ();
				}
				_isTextMeshProActive = t.isActiveAndEnabled;
			}
		}

		/// <summary>
		/// Callback for when properties have been changed by animation.
		/// </summary>
		protected override void OnDidApplyAnimationProperties ()
		{
			SetVerticesDirty ();
		}

#if UNITY_EDITOR
		/// <summary>
		/// This function is called when the script is loaded or a value is changed in the inspector (Called in the editor only).
		/// </summary>
		protected override void OnValidate ()
		{
			SetVerticesDirty ();
		}
#endif


		//################################
		// Private Members.
		//################################
		TMP_Text _textMeshPro;
		Graphic _graphic;
		CanvasRenderer _canvasRenderer;
		bool _isTextMeshProActive;
		bool _havePropChanged;

		/// <summary>
		/// Called when any TextMeshPro generated the mesh.
		/// </summary>
		/// <param name="obj">TextMeshPro object.</param>
		void OnTextChanged (Object obj)
		{
			//if(_havePropChanged)
			//{
			//	_havePropChanged = false;
			//	Debug.Log ("OnTextChanged _havePropChanged");
			//}
			//else
			//{
			//	Debug.Log ("OnTextChanged _havePropChanged Not!");
			//	SetVerticesDirty ();
			//	_textMeshPro.havePropertiesChanged = false;
			//}


			// Skip if the object is different from the current object or the text is empty.
			var textInfo = _textMeshPro.textInfo;
			if (_textMeshPro != obj || textInfo.characterCount - textInfo.spaceCount <= 0)
			{
				return;
			}

			// Collect the meshes.
			s_Meshes.Clear ();
			foreach (var info in textInfo.meshInfo)
			{
				s_Meshes.Add (info.mesh);
			}

			// Modify the meshes.
			if (isLegacyMeshModifier)
			{
				// Legacy mode: Modify the meshes directly.
				foreach (var m in s_Meshes)
				{
					ModifyMesh (m);
				}
			}
			else
			{
				// Convert meshes to VertexHelpers and modify them.
				foreach (var m in s_Meshes)
				{
					FillVertexHelper (s_VertexHelper, m);
					ModifyMesh (s_VertexHelper);
					s_VertexHelper.FillMesh (m);
				}
			}

			// Set the modified meshes to the CanvasRenderers (for UI only).
			if (canvasRenderer)
			{
				canvasRenderer.SetMesh (_textMeshPro.mesh);
				GetComponentsInChildren (false, s_SubMeshUIs);
				foreach (var sm in s_SubMeshUIs)
				{
					sm.canvasRenderer.SetMesh (sm.mesh);
				}
				s_SubMeshUIs.Clear ();
			}

			// Clear.
			s_Meshes.Clear ();
			//s_SubMeshes.Clear ();
			//s_SubMeshUIs.Clear ();
		}

		void FillVertexHelper (VertexHelper vh, Mesh mesh)
		{
			vh.Clear ();

			mesh.GetVertices (s_Vertices);
			mesh.GetColors (s_Colors);
			mesh.GetUVs (0, s_Uv0);
			mesh.GetUVs (1, s_Uv1);
			mesh.GetNormals (s_Normals);
			mesh.GetTangents (s_Tangents);
			mesh.GetIndices (s_Indices, 0);

			for (int i = 0; i < s_Vertices.Count; i++)
			{
				s_VertexHelper.AddVert (s_Vertices [i], s_Colors [i], s_Uv0 [i], s_Uv1 [i], s_Normals [i], s_Tangents [i]);
			}

			for (int i = 0; i < s_Indices.Count; i += 3)
			{
				vh.AddTriangle (s_Indices [i], s_Indices [i + 1], s_Indices [i + 2]);
			}
		}
	}
}