// #define CUSTOM_FONT

using System;
using System.Globalization;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using UnityEngine.UI;

namespace Parabox.RegexConstructor
{
	public class RegexBuilder : EditorWindow
	{
		struct ProcessedText
		{
			public string text;
			public string error;
			public int matchCount;

			public ProcessedText(string text, string error, int matchCount)
			{
				this.text = text;
				this.error = error;
				this.matchCount = matchCount;
			}
		}

		object[] m_SplitterArgs;
		MethodInfo m_BeginVerticalSplit;
		MethodInfo m_EndVerticalSplit;
		[SerializeField] object m_SplitterState;
		[SerializeField] string m_RegexPattern = "(?i)lorem";
		[SerializeField] string m_SampleText = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec pulvinar, diam ut mattis dictum, risus elit accumsan velit, a efficitur velit mauris id massa. Interdum et malesuada fames ac ante ipsum primis in faucibus. Pellentesque lobortis odio quis turpis bibendum malesuada. Suspendisse tincidunt molestie tortor non bibendum. Donec in lorem quis nunc pellentesque elementum sed ut arcu. Quisque ac tortor dolor. Nunc sed aliquet massa. Etiam sed orci et orci imperdiet scelerisque non ut neque. Quisque congue risus diam, quis tempor ligula pulvinar nec. Quisque blandit, tellus ut volutpat malesuada, purus dui porttitor nisi, sed egestas libero ipsum eget risus.";
		GUIStyle m_PatternSearchBox;
		GUIStyle m_VerticalWrapperStyle;

#if CUSTOM_FONT
		Font font;
		const string k_FontPath = "Assets/Debug/Font/ProggyClean.ttf";
#endif
		GUIStyle m_MatchesStyle;
		GUIStyle m_SampleTextStyle;
		GUIStyle m_PatternStyle;
		bool m_IsGuiInitialized;

		[SerializeField] Vector2 m_Scroll;
		[SerializeField] Vector2 m_ContentsScroll;
		[SerializeField] ProcessedText m_ProcessedText;
		[SerializeField] RegexOptions m_RegexOptions = RegexOptions.None;

		[MenuItem("Window/Regex Builder Window")]
		static void Init()
		{
			GetWindow<RegexBuilder>(true, "Parabox Regex", true).Show();
		}

		void OnEnable()
		{
			m_IsGuiInitialized = false;
			m_ProcessedText = DoRegex(m_SampleText, m_RegexPattern, m_RegexOptions);

			Type splitterStateType = Type.GetType("UnityEditor.SplitterState, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

			m_SplitterState = Activator.CreateInstance(
				splitterStateType,
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
				null,
				new object[] { new float[] { .7f, .3f }, new int[] { 120, 22 }, null },
				CultureInfo.CurrentCulture,
				null);

			Type splitterGuiLayoutType = Type.GetType("UnityEditor.SplitterGUILayout, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

			m_BeginVerticalSplit = splitterGuiLayoutType.GetMethod(
				"BeginVerticalSplit",
				BindingFlags.Static | BindingFlags.Public,
				null,
				CallingConventions.Any,
				new Type[] { splitterStateType, typeof(GUILayoutOption[]) },
				null);

			m_EndVerticalSplit = splitterGuiLayoutType.GetMethod(
				"EndVerticalSplit",
				BindingFlags.Static | BindingFlags.Public);

			m_SplitterArgs = new object[]
			{
				m_SplitterState,
				new GUILayoutOption[]
				{
					GUILayout.ExpandWidth(true),
					GUILayout.ExpandHeight(true)
				}
			};
		}

		void OnGUI()
		{
			if(!m_IsGuiInitialized)
			{
				m_IsGuiInitialized = true;

				m_MatchesStyle = new GUIStyle();
#if CUSTOM_FONT
				font = AssetDatabase.LoadAssetAtPath<Font>(k_FontPath);
				m_MatchesStyle.font = font;
#endif
				m_MatchesStyle.richText = true;
				m_MatchesStyle.margin = new RectOffset(4,4,4,4);
				m_MatchesStyle.padding = new RectOffset(2,2,2,2);
				m_MatchesStyle.normal.textColor = new Color(1f, 1f, 1f, .7f);
				m_MatchesStyle.wordWrap = true;

				m_PatternStyle = new GUIStyle(EditorStyles.textArea);
				m_PatternStyle.padding = new RectOffset(8,8,8,8);

				m_PatternSearchBox = new GUIStyle(EditorStyles.textField);
				m_PatternSearchBox.fixedHeight = 20;
				m_PatternSearchBox.margin.top += 2;
				m_PatternSearchBox.alignment = TextAnchor.MiddleLeft;

				m_SampleTextStyle = new GUIStyle(EditorStyles.textField);
				m_SampleTextStyle.wordWrap = true;
				m_SampleTextStyle.padding = new RectOffset(4, 4, 6, 6);

				m_VerticalWrapperStyle = new GUIStyle("CN Box");
			}

#if CUSTOM_FONT
			Font old = GUI.skin.font;
			GUI.skin.font = font;
			EditorStyles.boldLabel.font = font;
#endif
			m_BeginVerticalSplit.Invoke(null, m_SplitterArgs);

			GUILayout.BeginVertical(m_VerticalWrapperStyle);
				DoSearchAndResultsGui();
			GUILayout.EndVertical();

			GUILayout.BeginVertical(m_VerticalWrapperStyle);
				DoContentsAndSettingsGui();
			GUILayout.EndVertical();

#if CUSTOM_FONT
			EditorStyles.boldLabel.font = old;
			GUI.skin.font = old;
#endif
			m_EndVerticalSplit.Invoke(null, null);
		}

		void DoSearchAndResultsGui()
		{
			GUILayout.BeginHorizontal();

			GUILayout.Label("Pattern", EditorStyles.boldLabel);
			GUILayout.FlexibleSpace();
			if(GUILayout.Button("cheatsheet"))
				Application.OpenURL("https://msdn.microsoft.com/en-us/library/az24scfc(v=vs.110).aspx");
			if(GUILayout.Button("copy escaped pattern"))
				GUIUtility.systemCopyBuffer = m_RegexPattern.Replace("\\", "\\\\").Replace("\"", "\\\"");
			GUILayout.EndHorizontal();

			EditorGUI.BeginChangeCheck();
			m_RegexPattern = EditorGUILayout.TextArea(m_RegexPattern, m_PatternSearchBox);
			if(EditorGUI.EndChangeCheck())
				m_ProcessedText = DoRegex(m_SampleText, m_RegexPattern, m_RegexOptions);

			GUILayout.Label("Matches (" + m_ProcessedText.matchCount + ")", EditorStyles.boldLabel);

			if( !string.IsNullOrEmpty(m_ProcessedText.error) )
				EditorGUILayout.HelpBox(m_ProcessedText.error, MessageType.Error);

			m_Scroll = GUILayout.BeginScrollView(m_Scroll);

			RenderText(m_ProcessedText.text, m_MatchesStyle);

			GUILayout.EndScrollView();
		}

		void DoContentsAndSettingsGui()
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("Sample Text", EditorStyles.boldLabel);
			GUILayout.FlexibleSpace();
			bool multiLine = (m_RegexOptions & RegexOptions.Multiline) > 0;
			EditorGUI.BeginChangeCheck();
			multiLine = EditorGUILayout.Toggle("MultiLine", multiLine);
			if (EditorGUI.EndChangeCheck())
			{
				m_RegexOptions = m_RegexOptions ^ RegexOptions.Multiline;
				m_ProcessedText = DoRegex(m_SampleText, m_RegexPattern, m_RegexOptions);
			}
			GUILayout.EndHorizontal();

			EditorGUI.BeginChangeCheck();
			m_ContentsScroll = GUILayout.BeginScrollView(m_ContentsScroll);
			m_SampleText = EditorGUILayout.TextArea(m_SampleText, m_SampleTextStyle, GUILayout.ExpandHeight(true));
			if (EditorGUI.EndChangeCheck())
				m_ProcessedText = DoRegex(m_SampleText, m_RegexPattern, m_RegexOptions);
			GUILayout.EndScrollView();

		}

		static void RenderText(string contents, GUIStyle style)
		{
			// need to account for matching html tags
			 const int MAX_LENGTH = ushort.MaxValue / 8;
			 int splits = (int) (contents.Length / MAX_LENGTH);
			 for(int i = 0; i < splits + 1; i++)
			 {
				int index = (int) (i * MAX_LENGTH);
				string sub = contents.Substring(index, Math.Min(MAX_LENGTH, contents.Length - index));
				GUILayout.Label(sub, style);
			 }
		}

		static ProcessedText DoRegex(string source, string pattern, RegexOptions options)
		{
			ProcessedText processed = new ProcessedText(source, null, 0);

			if (string.IsNullOrEmpty(pattern) || string.IsNullOrEmpty(source))
				return processed;

			try
			{
				int inc = 0;

				StringBuilder sb = new StringBuilder(source);
				bool open = false;

				MatchCollection matches = Regex.Matches(source, pattern, options);
				processed.matchCount = matches.Count;

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
