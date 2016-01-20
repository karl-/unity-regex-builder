using UnityEngine;
using UnityEditor;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace Parabox.RegexConstructor
{
	public class RegexBuilder : EditorWindow
	{
		struct ProcessedText
		{
			public string text;
			public string error;

			public ProcessedText(string text, string error)
			{
				this.text = text;
				this.error = error;
			}
		}

		[MenuItem("Window/Regex Builder Window")]
		static void Init()
		{
			EditorWindow.GetWindow<RegexBuilder>(true, "Parabox Regex", true).Show();
		}

		public string pattern = "(?i)lorem";
		public string sample = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec pulvinar, diam ut mattis dictum, risus elit accumsan velit, a efficitur velit mauris id massa. Interdum et malesuada fames ac ante ipsum primis in faucibus. Pellentesque lobortis odio quis turpis bibendum malesuada. Suspendisse tincidunt molestie tortor non bibendum. Donec in lorem quis nunc pellentesque elementum sed ut arcu. Quisque ac tortor dolor. Nunc sed aliquet massa. Etiam sed orci et orci imperdiet scelerisque non ut neque. Quisque congue risus diam, quis tempor ligula pulvinar nec. Quisque blandit, tellus ut volutpat malesuada, purus dui porttitor nisi, sed egestas libero ipsum eget risus.";
#if CUSTOM_FONT
		private Font font;
#endif
		public Vector2 scroll = Vector2.zero;
		private GUIStyle matches_style = new GUIStyle();
		private GUIStyle pattern_style = null;
		private ProcessedText processed = new ProcessedText();
		private bool guiInitialized = false;

		void OnEnable()
		{
			processed = DoRegex(sample, pattern);
		}

		void OnGUI()
		{
			if(!guiInitialized)	
			{
				guiInitialized = true;

#if CUSTOM_FONT
				font = Resources.Load<Font>("monkey");
				matches_style.font = font;
#endif
				matches_style.richText = true;
				matches_style.margin = new RectOffset(4,4,4,4);
				matches_style.padding = new RectOffset(2,2,2,2);
				matches_style.normal.textColor = new Color(1f, 1f, 1f, .7f);
				matches_style.wordWrap = true;

				pattern_style = new GUIStyle(EditorStyles.textArea);
				pattern_style.padding = new RectOffset(8,8,8,8);
			}

#if CUSTOM_FONT
			Font old = GUI.skin.font;
			GUI.skin.font = font;
			EditorStyles.boldLabel.font = font;
#endif

			GUILayout.BeginHorizontal();
				GUILayout.Label("Pattern", EditorStyles.boldLabel);
				GUILayout.FlexibleSpace();
				if(GUILayout.Button("cheatsheet"))	
					Application.OpenURL("https://msdn.microsoft.com/en-us/library/az24scfc(v=vs.110).aspx");
				if(GUILayout.Button("copy escaped pattern"))
					GUIUtility.systemCopyBuffer = pattern.Replace("\\", "\\\\").Replace("\"", "\\\"");
			GUILayout.EndHorizontal();

			EditorGUI.BeginChangeCheck();
			pattern = EditorGUILayout.TextArea(pattern, GUILayout.MinHeight(24));
			if(EditorGUI.EndChangeCheck())
				processed = DoRegex(sample, pattern);

			Color color = GUI.color;

			GUILayout.Label("Matches", EditorStyles.boldLabel);

			if( !string.IsNullOrEmpty(processed.error) )
				EditorGUILayout.HelpBox(processed.error, MessageType.Error);

			scroll = GUILayout.BeginScrollView(scroll);

			GUILayout.Label(processed.text, matches_style);

			GUILayout.EndScrollView();

			GUILayout.FlexibleSpace();

			GUILayout.Label("Sample Text", EditorStyles.boldLabel);

			sample = EditorGUILayout.TextArea(sample, GUILayout.MinHeight(64), GUILayout.MaxHeight(64));

			GUI.color = color;

#if CUSTOM_FONT
				EditorStyles.boldLabel.font = old;
				GUI.skin.font = old;
#endif
		}

		static void RenderText(StringBuilder sb, GUIStyle style)
		{
			// need to account for matching html tags
			// const int MAX_LENGTH = ushort.MaxValue / 2;
			// int splits = (int) (sb.Length / MAX_LENGTH);
			// for(int i = 0; i < splits + 1; i++)
			// {
			// 	int index = (int) (i * MAX_LENGTH);				
			// 	GUILayout.Label(sb.ToString(index, System.Math.Min(MAX_LENGTH, sb.Length - index)), style);
			// }
			GUILayout.Label(sb.ToString(), style);
		}

		static ProcessedText DoRegex(string source, string pattern)
		{
			ProcessedText processed = new ProcessedText(source, null);

			try
			{
				int inc = 0;

				StringBuilder sb = new StringBuilder(source);
				bool open = false;

				MatchCollection matches = Regex.Matches(source, pattern);

				for(int i = 0; i < matches.Count; i++)
				{
					Match match = matches[i];

					if(!open)
					{
						sb.Insert(inc + match.Index, "<color=#00FF00FF>");
						inc += 17;	
					}

					open = i < matches.Count - 1 && match.Index + match.Length == matches[i+1].Index;

					for(int n = inc + match.Index; n < inc + match.Index + match.Length; n++)
						if(char.IsSeparator(sb[n]))
							sb[n] = '→';

					if(!open)
					{
						sb.Insert(inc + match.Index + match.Length, "</color>");
						inc += 8;
					}
				}

				processed.text = sb.ToString();
			}
			catch(System.Exception e)
			{
				processed.error = e.Message;
			}

			return processed;
		}
	}
}
