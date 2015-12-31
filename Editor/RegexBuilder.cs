using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using System.Linq;

namespace Parabox.RegexConstructor
{
	public class RegexBuilder : EditorWindow
	{
		[MenuItem("Window/Regex Builder Window")]
		static void Init()
		{
			EditorWindow.GetWindow<RegexBuilder>().Show();
		}

		public string pattern = "_TEMP_UV3_CHANNEL\\s\\(\"TEMP_UV3_CHANNEL\", Vector\\)\\s=\\s\\([\\d*\\.?\\d*,?]*\\)";
		public string sample = "Shader \"Polybrush/TextureBlender\" {\n Properties {\n _MainTex (\"Base Color\", 2D) = \"white\" {}\n _TextureG (\"TextureG\", 2D) = \"white\" {}\n _TextureB (\"TextureB\", 2D) = \"white\" {}\n _TextureA (\"TextureA\", 2D) = \"white\" {}\n _Metallic (\"Metallic\", Range(0, 1)) = 0\n _Gloss (\"Gloss\", Range(0, 1)) = 0.8\n _TextureU (\"TextureU\", 2D) = \"white\" {}\n _TextureV (\"TextureV\", 2D) = \"white\" {}\n _TextureS (\"TextureS\", 2D) = \"white\" {}\n _TextureT (\"TextureT\", 2D) = \"white\" {}\n _TEMP_UV3_CHANNEL (\"TEMP_UV3_CHANNEL\", Vector) = (1,0,0,0)\n [HideInInspector]_Cutoff (\"Alpha cutoff\", Range(0,1)) = 0.5\n }\n SubShader {";
		private Font font;
		private string escaped_pattern = "";
		public Vector2 scroll = Vector2.zero;
		private GUIStyle matches_style = new GUIStyle();

		void OnEnable()
		{
			escaped_pattern = pattern;
			font = Resources.Load<Font>("monkey");

			matches_style.font = font;
			matches_style.richText = true;
			matches_style.normal.textColor = new Color(1f, 1f, 1f, .7f);
		}

		void OnGUI()
		{
			Font old = GUI.skin.font;
			GUI.skin.font = font;
			EditorStyles.boldLabel.font = font;

			GUILayout.BeginHorizontal();
				GUILayout.Label("Pattern", EditorStyles.boldLabel);
				GUILayout.FlexibleSpace();
				if(GUILayout.Button("cheatsheet"))	
					Application.OpenURL("https://msdn.microsoft.com/en-us/library/az24scfc(v=vs.110).aspx");
				if(GUILayout.Button("Copy Escaped Pattern"))
					GUIUtility.systemCopyBuffer = escaped_pattern.Replace("\\", "\\\\").Replace("\"", "\\\"");
			GUILayout.EndHorizontal();

			EditorGUI.BeginChangeCheck();
			pattern = EditorGUILayout.TextArea(pattern, GUILayout.MinHeight(36));
			if(EditorGUI.EndChangeCheck())
				escaped_pattern = pattern;

			Color color = GUI.color;
			string error = string.Empty;
			bool valid = true;

			GUILayout.Label("Matches", EditorStyles.boldLabel);
			
			scroll = GUILayout.BeginScrollView(scroll);

			try
			{
				int inc = 0;

				System.Text.StringBuilder sb = new System.Text.StringBuilder(sample);

				foreach(Match match in Regex.Matches(sample, escaped_pattern))
				{
					sb.Insert(inc + match.Index, "<color=#00FF00FF>");
					inc += 17;
					sb.Insert(inc + match.Index + match.Length, "</color>");
					inc += 8;
				}

				GUILayout.Label(sb.ToString(), matches_style);
			}
			catch(System.Exception e)
			{
				valid = false;
				error = e.Message;
			}


			if(!valid)
				EditorGUILayout.HelpBox(error, MessageType.Error);

			GUILayout.EndScrollView();

			GUILayout.FlexibleSpace();

			GUILayout.Label("Sample Text", EditorStyles.boldLabel);

			sample = EditorGUILayout.TextArea(sample, GUILayout.MinHeight(128), GUILayout.MaxHeight(128));

			GUI.color = color;
			EditorStyles.boldLabel.font = old;
			GUI.skin.font = old;
		}
	}
}
