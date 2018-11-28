using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using TMPro;

namespace Coffee.UIExtensions
{
	/// <summary>
	/// Dissolve effect for uGUI.
	/// </summary>
	[AddComponentMenu("UI/UIEffect/UIDissolve", 3)]
	public class UIDissolve : UIEffectBase
	{
		//################################
		// Constant or Static Members.
		//################################
		public const string shaderName = "UI/Hidden/UI-Effect-Dissolve";
		static readonly ParameterTexture _ptex = new ParameterTexture(8, 128, "_ParamTex");


		//################################
		// Serialize Members.
		//################################
		[Tooltip("Current location[0-1] for dissolve effect. 0 is not dissolved, 1 is completely dissolved.")]
		[FormerlySerializedAs("m_Location")]
		[SerializeField] [Range(0, 1)] float m_EffectFactor = 0.5f;

		[Tooltip("Edge width.")]
		[SerializeField] [Range(0, 1)] float m_Width = 0.5f;

		[Tooltip("Edge softness.")]
		[SerializeField] [Range(0, 1)] float m_Softness = 0.5f;

		[Tooltip("Edge color.")]
		[SerializeField] [ColorUsage(false)] Color m_Color = new Color(0.0f, 0.25f, 1.0f);

		[Tooltip("Edge color effect mode.")]
		[SerializeField] ColorMode m_ColorMode = ColorMode.Add;

		[Tooltip("Noise texture for dissolving (single channel texture).")]
		[SerializeField] Texture m_NoiseTexture;

		[Tooltip("The area for effect.")]
		[SerializeField] protected EffectArea m_EffectArea;

		[Tooltip("Keep effect aspect ratio.")]
		[SerializeField] bool m_KeepAspectRatio;

		[Header("Effect Player")]
		[SerializeField] EffectPlayer m_Player;

		[Obsolete][HideInInspector]
		[SerializeField][Range(0.1f, 10)] float m_Duration = 1;
		[Obsolete][HideInInspector]
		[SerializeField] AnimatorUpdateMode m_UpdateMode = AnimatorUpdateMode.Normal;


		//################################
		// Public Members.
		//################################

		/// <summary>
		/// Effect factor between 0(start) and 1(end).
		/// </summary>
		[System.Obsolete("Use effectFactor instead (UnityUpgradable) -> effectFactor")]
		public float location
		{
			get { return m_EffectFactor; }
			set
			{
				value = Mathf.Clamp(value, 0, 1);
				if (!Mathf.Approximately(m_EffectFactor, value))
				{
					m_EffectFactor = value;
					SetDirty();
				}
			}
		}

		/// <summary>
		/// Effect factor between 0(start) and 1(end).
		/// </summary>
		public float effectFactor
		{
			get { return m_EffectFactor; }
			set
			{ 
				value = Mathf.Clamp(value, 0, 1);
				if (!Mathf.Approximately(m_EffectFactor, value))
				{
					m_EffectFactor = value;
					SetDirty();
				}
			}
		}

		/// <summary>
		/// Edge width.
		/// </summary>
		public float width
		{
			get { return m_Width; }
			set
			{
				value = Mathf.Clamp(value, 0, 1);
				if (!Mathf.Approximately(m_Width, value))
				{
					m_Width = value;
					SetDirty();
				}
			}
		}

		/// <summary>
		/// Edge softness.
		/// </summary>
		public float softness
		{
			get { return m_Softness; }
			set
			{
				value = Mathf.Clamp(value, 0, 1);
				if (!Mathf.Approximately(m_Softness, value))
				{
					m_Softness = value;
					SetDirty();
				}
			}
		}

		/// <summary>
		/// Edge color.
		/// </summary>
		public Color color
		{
			get { return m_Color; }
			set
			{
				if (m_Color != value)
				{
					m_Color = value;
					SetDirty();
				}
			}
		}

		/// <summary>
		/// Noise texture.
		/// </summary>
		public Texture noiseTexture
		{
			get { return m_NoiseTexture ?? material.GetTexture("_NoiseTex"); }
			set
			{
				if (m_NoiseTexture != value)
				{
					m_NoiseTexture = value;
					if (graphic)
					{
						ModifyMaterial();
					}
				}
			}
		}

		/// <summary>
		/// The area for effect.
		/// </summary>
		public EffectArea effectArea
		{
			get { return m_EffectArea; }
			set
			{
				if (m_EffectArea != value)
				{
					m_EffectArea = value;
					SetDirty();
				}
			}
		}

		/// <summary>
		/// Keep aspect ratio.
		/// </summary>
		public bool keepAspectRatio
		{
			get { return m_KeepAspectRatio; }
			set
			{
				if (m_KeepAspectRatio != value)
				{
					m_KeepAspectRatio = value;
					SetDirty();
				}
			}
		}

		/// <summary>
		/// Color effect mode.
		/// </summary>
		public ColorMode colorMode { get { return m_ColorMode; } }

		/// <summary>
		/// Play effect on enable.
		/// </summary>
		[System.Obsolete("Use Show/Hide method instead")]
		public bool play { get { return _player.play; } set { _player.play = value; } }

		/// <summary>
		/// Play effect loop.
		/// </summary>
		[System.Obsolete]
		public bool loop { get { return _player.loop; } set { _player.loop = value; } }

		/// <summary>
		/// The duration for playing effect.
		/// </summary>
		public float duration { get { return _player.duration; } set { _player.duration = Mathf.Max(value, 0.1f); } }

		/// <summary>
		/// Delay on loop effect.
		/// </summary>
		[System.Obsolete]
		public float loopDelay { get { return _player.loopDelay; } set { _player.loopDelay = Mathf.Max(value, 0); } }

		/// <summary>
		/// Update mode for playing effect.
		/// </summary>
		public AnimatorUpdateMode updateMode { get { return _player.updateMode; } set { _player.updateMode = value; } }

		/// <summary>
		/// Gets the parameter texture.
		/// </summary>
		public override ParameterTexture ptex { get { return _ptex; } }

		public Material material
		{
			get
			{
				var g = graphic;
				var t = g as TMPro.TextMeshProUGUI;
				return t ? t.fontMaterial : g.material;
			}
			set
			{
				var g = graphic;
				var t = g as TMPro.TextMeshProUGUI;
				if(t && t.fontMaterial != value)
				{
					t.fontMaterial = value;
				}
				else if(!t && g.material != value)
				{
					g.material = value;
				}
			}
		}

		public Material GetModifiedMaterial (Material baseMaterial)
		{
			//if (graphic is TMPro.TextMeshProUGUI && baseMaterial.HasProperty("_NoiseTex"))
			//{
			//	Debug.Log (m_NoiseTexture);
			//	baseMaterial.SetTexture ("_NoiseTex", m_NoiseTexture);
			//}
			return baseMaterial;
		}

		/// <summary>
		/// Modifies the material.
		/// </summary>
		public override void ModifyMaterial()
		{
			if (graphic is TMPro.TextMeshProUGUI)
			{
				TMPro.TextMeshProUGUI t = graphic as TMPro.TextMeshProUGUI;
				//baseMaterial.SetTexture ("_NoiseTex", m_NoiseTexture);
				Debug.LogFormat ("ModifyMaterial {0}, {1}, {2}, {3}, {4}", t.material, t.fontMaterial, t.fontMaterial.GetInstanceID(), t.fontSharedMaterial, t.fontSharedMaterial.GetInstanceID ());
				return;
			}

			ulong hash = (m_NoiseTexture ? (uint)m_NoiseTexture.GetInstanceID() : 0) + ((ulong)1 << 32) + ((ulong)m_ColorMode << 36);
			if (_materialCache != null && (_materialCache.hash != hash || !isActiveAndEnabled || !m_EffectMaterial))
			{
				MaterialCache.Unregister(_materialCache);
				_materialCache = null;
			}

			if (!isActiveAndEnabled || !m_EffectMaterial)
			{
				material = null;
			}
			else if (!m_NoiseTexture)
			{
				material = m_EffectMaterial;
			}
			else if (_materialCache != null && _materialCache.hash == hash)
			{
				material = _materialCache.material;
			}
			else
			{
				_materialCache = MaterialCache.Register(hash, m_NoiseTexture, () =>
					{
						var mat = new Material(m_EffectMaterial);
						mat.name += "_" + m_NoiseTexture.name;
						mat.SetTexture("_NoiseTex", m_NoiseTexture);
						return mat;
					});
				material = _materialCache.material;
			}
		}

		/// <summary>
		/// Modifies the mesh.
		/// </summary>
		public override void ModifyMesh(VertexHelper vh)
		{
			Debug.Log ("ModifyMesh");

			//return;
			if (!isActiveAndEnabled)
				return;



			float normalizedIndex = ptex.GetNormalizedIndex(this);

			// rect.
			var tex = noiseTexture;
			var aspectRatio = m_KeepAspectRatio && tex ? ((float)tex.width) / tex.height : -1;
			Rect rect = m_EffectArea.GetEffectArea(vh, graphic, aspectRatio);

			// Calculate vertex position.
			UIVertex vertex = default(UIVertex);
			bool effectEachCharacter = graphic is Text && m_EffectArea == EffectArea.Character;
			float x, y;
			/*
			int count = vh.currentVertCount;
			for (int i = 0; i < count; i++)
			{
				vh.PopulateUIVertex(ref vertex, i);

				if (effectEachCharacter)
				{
					x = splitedCharacterPosition[i % 4].x;
					y = splitedCharacterPosition[i % 4].y;
				}
				else
				{
					x = Mathf.Clamp01(vertex.position.x / rect.width + 0.5f);
					y = Mathf.Clamp01(vertex.position.y / rect.height + 0.5f);
				}

				//vertex.uv0 = new Vector2(
				//	Packer.ToFloat(vertex.uv0.x, vertex.uv0.y),
				//	Packer.ToFloat(x, y, normalizedIndex)
				//);

				vertex.uv2 = new Vector2 (
					Packer.ToFloat (x, y, normalizedIndex), 0
				);

				vh.SetUIVertex(vertex, i);
			}
			*/


			var t = graphic as TMPro.TextMeshProUGUI;
			Mesh mesh = t.mesh;
			int count = mesh.vertexCount;
			Vector2[] uvs = mesh.uv;
			for (int i = 0; i < count; i++)
			{
				if (effectEachCharacter)
				{
					x = splitedCharacterPosition [i % 4].x;
					y = splitedCharacterPosition [i % 4].y;
				}
				else
				{
					x = Mathf.Clamp01 (vertex.position.x / rect.width + 0.5f);
					y = Mathf.Clamp01 (vertex.position.y / rect.height + 0.5f);
				}

				uvs [i] = new Vector2(
					Packer.ToFloat(uvs [i].x, uvs [i].y),
					Packer.ToFloat(x, y, normalizedIndex)
				);
			}
			mesh.uv = uvs;

			Debug.Log (count);
		}

		protected override void SetDirty()
		{
			ptex.RegisterMaterial(material);
			ptex.SetData(this, 0, m_EffectFactor);	// param1.x : location
			ptex.SetData(this, 1, m_Width);		// param1.y : width
			ptex.SetData(this, 2, m_Softness);	// param1.z : softness
			ptex.SetData(this, 4, m_Color.r);	// param2.x : red
			ptex.SetData(this, 5, m_Color.g);	// param2.y : green
			ptex.SetData(this, 6, m_Color.b);	// param2.z : blue
		}

		/// <summary>
		/// Play effect.
		/// </summary>
		public void Play()
		{
			_player.Play();
		}

		/// <summary>
		/// Stop effect.
		/// </summary>
		public void Stop()
		{
			_player.Stop();
		}

		//################################
		// Protected Members.
		//################################
		/// <summary>
		/// This function is called when the object becomes enabled and active.
		/// </summary>
		protected override void OnEnable()
		{
			Canvas.willRenderCanvases += test;
			base.OnEnable();
			_player.OnEnable(f => effectFactor = f);
		}

		protected override void OnDisable()
		{
			Canvas.willRenderCanvases -= test;
			MaterialCache.Unregister(_materialCache);
			_materialCache = null;
			_player.OnDisable();
			base.OnDisable();
		}


		void test ()
		{
			TextMeshProUGUI t = GetComponent<TextMeshProUGUI> ();
			if (havePropertiesChanged)
			{
				havePropertiesChanged = false;
				Debug.Log ("test");
				if (!isActiveAndEnabled)
					return;



				float normalizedIndex = ptex.GetNormalizedIndex (this);

				// rect.
				var tex = noiseTexture;
				var aspectRatio = m_KeepAspectRatio && tex ? ((float)tex.width) / tex.height : -1;
				//Rect rect = m_EffectArea.GetEffectArea (vh, graphic, aspectRatio);
				var rect = graphic.rectTransform.rect;

				// Calculate vertex position.
				UIVertex vertex = default (UIVertex);
				bool effectEachCharacter = graphic is Text && m_EffectArea == EffectArea.Character;
				float x, y;

				//var t = graphic as TMPro.TextMeshProUGUI;
				Mesh mesh = t.mesh;
				int count = mesh.vertexCount;
				Vector2 [] uvs = mesh.uv;
				Vector3 [] positions = mesh.vertices;
				for (int i = 0; i < count; i++)
				{
					if (effectEachCharacter)
					{
						x = splitedCharacterPosition [i % 4].x;
						y = splitedCharacterPosition [i % 4].y;
					}
					else
					{
						x = Mathf.Clamp01 (positions[i].x / rect.width + 0.5f);
						y = Mathf.Clamp01 (positions[i].y / rect.height + 0.5f);
					}

					uvs [i] = new Vector2 (
						Packer.ToFloat (uvs [i].x, uvs [i].y),
						Packer.ToFloat (x, y, normalizedIndex)
					);
				}
				mesh.uv = uvs;
				t.canvasRenderer.SetMesh (mesh);

				Debug.Log (count);
			}
		}

		bool havePropertiesChanged;

		void LateUpdate ()
		{
			TextMeshProUGUI t = GetComponent<TextMeshProUGUI> ();
			if (t.havePropertiesChanged)
			{
				havePropertiesChanged = true;
			}
		}

#if UNITY_EDITOR
		/// <summary>
		/// Gets the material.
		/// </summary>
		/// <returns>The material.</returns>
		protected override Material GetMaterial()
		{
			return MaterialResolver.GetOrGenerateMaterialVariant(Shader.Find(shaderName), m_ColorMode);
		}

		#pragma warning disable 0612
		protected override void UpgradeIfNeeded()
		{
			// Upgrade for v3.0.0
			if (IsShouldUpgrade(300))
			{
				_player.play = false;
				_player.duration = m_Duration;
				_player.loop = false;
				_player.loopDelay = 1;
				_player.updateMode = m_UpdateMode;
			}
		}

#pragma warning restore 0612
#endif

		//################################
		// Private Members.
		//################################
		MaterialCache _materialCache = null;

		EffectPlayer _player{ get { return m_Player ?? (m_Player = new EffectPlayer()); } }
	}
}
