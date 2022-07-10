using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEditor;
using UnityEngine;
using SLywnow;
using System;
using System.Globalization;

public class KPSL_Editor_Main : EditorWindow
{
	KPSL_Editor_MainConfig config = new KPSL_Editor_MainConfig();
	bool enabled = false;
	List<KPSL_Editor_Snapshots> snapshots;

	void OnEnable()
    {
		//loadcfg

		config.Load();
		serverdir = config.serverdir;

		if (FilesSet.CheckFile(config.serverdir + "/kopia.exe"))
		{
			enabled = true;

			//runProc("policy set +" + "\"" + Application.dataPath + "/SLywnowAssets/KopiaUnity/policy.json" + "\"");
			runProc("policy set --global --ignore-cache-dirs true");
			runProc("policy set --global --add-ignore Temp");
			runProc("policy set --global --add-dot-ignore AtlasCache,TempArtifacts,Temp,StateCache,ShaderCache,PlayerDataCache,PackageCache");

			CheckAndLoad();
		}

		//UnityEngine.Debug.Log(Application.dataPath);
		//UnityEngine.Debug.Log("snapshot list " + "\"" + Application.dataPath + "\"");
	}

	string serverdir = "";
	Vector2 snappos;

    void OnGUI()
    {
		GUIStyle style = new GUIStyle();
		style.richText = true;

		EditorGUILayout.BeginHorizontal();
		serverdir = EditorGUILayout.TextField("", serverdir);
		if (GUILayout.Button("Save & Check"))
		{
			if (serverdir[serverdir.Length - 1] == '/' || serverdir[serverdir.Length - 1] == '\\')
				serverdir.Remove(serverdir.Length - 1);
			config.serverdir = serverdir;
			config.Save();

			if (FilesSet.CheckFile(config.serverdir + "/kopia.exe"))
				enabled = true;
			CheckAndLoad();
		}
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.Space();
		EditorGUILayout.Space();

		if (enabled)
		{
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Create new snapshot"))
			{
				runProc("snapshot create " + "\"" + Application.dataPath.Replace("/Assets", "") + "\"");
				CheckAndLoad();
			}
			if (GUILayout.Button("Refresh list"))
			{
				CheckAndLoad();
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();

			snappos = GUILayout.BeginScrollView(snappos, style);

			if (snapshots.Count == 0)
				GUILayout.Label("There's no snapshots, create one!");

			for (int i =snapshots.Count-1;i>=0;i--)
			{
				int id = i;
				KPSL_Editor_Snapshots s = snapshots[id];
				s.show = EditorGUILayout.BeginFoldoutHeaderGroup(s.show, s.data.ToString());
				if (s.show)
				{
					GUILayout.Label(s.ind);
					GUILayout.Label("Size: " + s.size);
					GUILayout.Label("Files: " + s.files);
					GUILayout.Label("Directories: "+ s.dirs);

					EditorGUILayout.BeginHorizontal();
					if (GUILayout.Button("Restore this"))
					{
						runProc("snapshot restore " + s.ind);
					}
					if (GUILayout.Button("Delete"))
					{
						runProc("snapshot delete " + s.ind +" --delete");
					}
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.EndFoldoutHeaderGroup();
			}
			GUILayout.EndScrollView();
		}
		else
			GUILayout.Label("Please input path to kopia.exe",style);
	}


	string runProc(string args)
	{
		string ret = "";
		var proc = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				FileName = config.serverdir + "\\kopia.exe",
				//Arguments = "snapshot list " + "\"" + Application.dataPath.Replace("/Assets", "") + "\"",
				Arguments = args,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				CreateNoWindow = true,
				Verb = "runas"
			}
		};

		proc.Start();

		ret = proc.StandardOutput.ReadToEnd();

		return ret;
	}

	void CheckAndLoad()
	{
		
		string output = runProc("snapshot list " + "\"" + Application.dataPath.Replace("/Assets", "") + "\"");

		List<string> outputs = output.Split('\n').ToList();
		snapshots = new List<KPSL_Editor_Snapshots>();

		if (outputs.Count > 1)
			for (int i = 1; i < outputs.Count; i++)
			{
				if (!string.IsNullOrEmpty(outputs[i]))
				{
					string[] data = outputs[i].Split(' ');
					snapshots.Add(new KPSL_Editor_Snapshots());
					snapshots[snapshots.Count - 1].data = DateTime.Parse(data[2] + " " + data[3]);
					snapshots[snapshots.Count - 1].ind = data[5];
					snapshots[snapshots.Count - 1].size = data[6] + data[7];
					snapshots[snapshots.Count - 1].files = data[9].Replace("files:", "");
					snapshots[snapshots.Count - 1].dirs = data[10].Replace("dirs:", "");
				}
			}
	}
}

[System.Serializable]
public class KPSL_Editor_Snapshots
{
	public bool show;
	public DateTime data;
	public string ind;
	public string size;
	public string files;
	public string dirs;
}

	[System.Serializable]
public class KPSL_Editor_MainConfig
{
	public string serverdir;

	public void Load()
	{
		if (FilesSet.CheckFile(Application.dataPath + "/SLywnowAssets/KopiaUnity/config.cfg"))
		{
			KPSL_Editor_MainConfig c = JsonUtility.FromJson<KPSL_Editor_MainConfig>(FilesSet.LoadStream(Application.dataPath + "/SLywnowAssets/KopiaUnity/config.cfg", false, false));
			serverdir = c.serverdir;
		}
	}

	public void Save()
	{
		FilesSet.SaveStream(Application.dataPath + "/SLywnowAssets/KopiaUnity/config.cfg", JsonUtility.ToJson(this, true));
	}
}

public class KPSL_Editor_MainManager : Editor
{
    [MenuItem("SLywnow/Kopia UI")]
    static void SetDirection()
    {
        EditorWindow.GetWindow(typeof(KPSL_Editor_Main), false, "Kopia UI", true);
    }
}
