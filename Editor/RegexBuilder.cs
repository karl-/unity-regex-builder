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
		// private Font font;
		private string escaped_pattern = "";
		public Vector2 scroll = Vector2.zero;

		void OnEnable()
		{
			escaped_pattern = pattern;
			// font = Resources.Load<Font>("monkey");
		}

		void OnGUI()
		{
			// Font old = GUI.skin.font;
			// GUI.skin.font = font;
			// EditorStyles.boldLabel.font = font;

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

			GUI.color = Color.green;
			try {
				foreach(Match match in Regex.Matches(sample, escaped_pattern))
					GUILayout.Label(match.Value);

				Regex.Match("garbage string", escaped_pattern);
			} catch(System.Exception e) {
				valid = false;
				error = e.Message;
			}
			GUI.color = color;

			if(!valid)
				EditorGUILayout.HelpBox(error, MessageType.Error);

			GUILayout.EndScrollView();

			GUILayout.FlexibleSpace();

			GUILayout.Label("Sample Text", EditorStyles.boldLabel);

			sample = EditorGUILayout.TextArea(sample, GUILayout.MinHeight(128), GUILayout.MaxHeight(128));

			GUI.color = color;
			// EditorStyles.boldLabel.font = old;
			// GUI.skin.font = old;
		}
	}
}
