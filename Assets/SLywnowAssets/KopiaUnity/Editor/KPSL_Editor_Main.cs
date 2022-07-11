using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEditor;
using UnityEngine;
using SLywnow;
using System;
using System.Threading.Tasks;

public class KPSL_Editor_Main : EditorWindow
{
	KPSL_Editor_MainConfig config = new KPSL_Editor_MainConfig();
	KPSL_Editor_MainConfig preconfig = new KPSL_Editor_MainConfig();
	bool enabled = false;
	List<KPSL_Editor_Snapshots> snapshots;
	string datapath;

	Texture2D refreshIcon;
	Texture2D settingsIcon;

	void OnEnable()
    {

		//loadcfg

		config.Load();
		serverdir = config.serverdir;

		if (!config.snaponlyassets)
			datapath = Application.dataPath.Replace("/Assets", "");
		else
			datapath = Application.dataPath;


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

		if (FilesSet.CheckFile(Application.dataPath + "/SLywnowAssets/KopiaUnity/Textures/refresh.png"))
		{
			refreshIcon = FilesSet.LoadSprite(Application.dataPath + "/SLywnowAssets/KopiaUnity/Textures/refresh.png", false).texture;
		}
		else
			refreshIcon = null;

		if (FilesSet.CheckFile(Application.dataPath + "/SLywnowAssets/KopiaUnity/Textures/settings.png"))
		{
			settingsIcon = FilesSet.LoadSprite(Application.dataPath + "/SLywnowAssets/KopiaUnity/Textures/settings.png", false).texture;
		}
		else
			refreshIcon = null;

	}

	string serverdir = "";
	Vector2 snappos;
	Vector2 filepos;
	bool working;
	string workingStatus;

	enum oM { basic, files, settings };
	oM openMode = oM.basic;
	int filePos;
	List<string> fileList;
	List<KPSL_Editor_File> files;

	void OnGUI()
	{
		GUIStyle style = new GUIStyle();
		style.richText = true;

		if (openMode != oM.settings)
		{
			EditorGUILayout.BeginHorizontal();
			if (!enabled)
			{
				GUILayout.Label("Path to exe:", GUILayout.Width(70));
				serverdir = EditorGUILayout.TextField("", serverdir);
				if (GUILayout.Button("Save & Check", GUILayout.Width(110)))
				{
					if (!string.IsNullOrEmpty(serverdir))
					{
						if (serverdir[serverdir.Length - 1] == '/' || serverdir[serverdir.Length - 1] == '\\')
							serverdir.Remove(serverdir.Length - 1);
						config.serverdir = serverdir;
						config.Save();

						if (FilesSet.CheckFile(config.serverdir + "/kopia.exe"))
						{
							enabled = true;
							CheckAndLoad();
						}
						else
							enabled = false;
					}
					else
						serverdir = config.serverdir;
				}
			}
			else
			{
				GUILayout.Label("");
			}

			if (settingsIcon == null)
			{
				if (GUILayout.Button("Settings", GUILayout.Width(70)))
				{
					preconfig = config.Copy();
					openMode = oM.settings;
				}
			}
			else
			{
				if (GUILayout.Button(new GUIContent(settingsIcon), GUILayout.Width(19), GUILayout.Height(19)))
				{
					preconfig = config.Copy();
					openMode = oM.settings;
				}
			}

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();
			EditorGUILayout.Space();
		}

		if (enabled)
		{
			if (!working)
			{
				if (openMode==oM.basic)
				{
					EditorGUILayout.BeginHorizontal();
					if (GUILayout.Button("Create new snapshot"))
					{
						NewSnapshotAsync();
					}

					if (refreshIcon == null)
					{
						if (GUILayout.Button("Refresh list"))
						{
							CheckAndLoad();
						}
					}
					else
					{
						if (GUILayout.Button(new GUIContent(refreshIcon), GUILayout.Width(19), GUILayout.Height(19)))
						{
							CheckAndLoad();
						}
					}
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.Space();

					snappos = GUILayout.BeginScrollView(snappos, style);

					if (snapshots.Count == 0)
						GUILayout.Label("There's no snapshots, create one!");

					for (int i = snapshots.Count - 1; i >= 0; i--)
					{
						int id = i;
						KPSL_Editor_Snapshots s = snapshots[id];
						s.show = EditorGUILayout.BeginFoldoutHeaderGroup(s.show, s.data.ToString());
						if (s.show)
						{
							GUILayout.Label(s.ind);
							GUILayout.Label("Size: " + s.size);
							GUILayout.Label("Files: " + s.files);
							GUILayout.Label("Directories: " + s.dirs);

							if (GUILayout.Button("Show files"))
							{
								openMode = oM.files;
								fileList = new List<string>();
								fileList.Add(s.ind);
							}

							EditorGUILayout.BeginHorizontal();
							if (GUILayout.Button("Restore this"))
							{
								RestoreSnapshot(s);
							}
							if (GUILayout.Button("Delete"))
							{
								DeleteSnapshot(s);
							}
							EditorGUILayout.EndHorizontal();
						}
						EditorGUILayout.EndFoldoutHeaderGroup();
					}
					GUILayout.EndScrollView();
				}
				else if (openMode==oM.files)
				{
					if (fileList.Count == 1)
					{
						if (GUILayout.Button("Close"))
						{
							openMode = oM.basic;
							fileList = new List<string>();
						}
					}
					else
					{
						if (GUILayout.Button("To the top"))
						{
							fileList.RemoveAt(fileList.Count - 1);
						}
					}

					if (filePos != fileList.Count)
					{
						OpenFiles();
						filePos = fileList.Count;
					}
					else
					{
						filepos = GUILayout.BeginScrollView(filepos, style);
						foreach (KPSL_Editor_File file in files)
						{
							EditorGUILayout.BeginHorizontal();

							if (file.folder)
							{
								if (GUILayout.Button(file.name + " " + file.size))
								{
									fileList.Add(file.name);
								}
							}
							else
								GUILayout.Label(file.name + " " + file.size);

							if (GUILayout.Button("Restore", GUILayout.Width(100)))
							{
								string path = "";
								foreach (string s in fileList)
									path += s + "/";
								path += file.name;

								string realpath = path.Replace(fileList[0] + "/", "");

								RestoreSnapshot(path, realpath);
							}

							EditorGUILayout.EndHorizontal();
						}
						GUILayout.EndScrollView();
					}
				}
				else if (openMode==oM.settings)
				{
					if (GUILayout.Button("Close"))
					{
						openMode = oM.basic;
						config = preconfig.Copy();
					}

					GUILayout.BeginHorizontal();
					GUILayout.Label("Path to exe:", GUILayout.Width(200));
					config.serverdir = EditorGUILayout.TextField("", config.serverdir);
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Label("Show .meta files:", GUILayout.Width(200));
					config.showmeta = EditorGUILayout.Toggle(config.showmeta);
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Label("Snapshot only Asset folder:", GUILayout.Width(200));
					config.snaponlyassets = EditorGUILayout.Toggle(config.snaponlyassets);
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Label("Dont rewrite files when restore:", GUILayout.Width(200));
					config.dontrewrite = EditorGUILayout.Toggle(config.dontrewrite);
					GUILayout.EndHorizontal();

					if (GUILayout.Button("Save"))
					{
						if (!string.IsNullOrEmpty(config.serverdir))
						{
							if (config.serverdir[config.serverdir.Length - 1] == '/' || config.serverdir[config.serverdir.Length - 1] == '\\')
								config.serverdir.Remove(config.serverdir.Length - 1);

							if (FilesSet.CheckFile(config.serverdir + "/kopia.exe"))
							{
								enabled = true;
								CheckAndLoad();
							}
							else
								enabled = false;
						}
						else
							config.serverdir = preconfig.serverdir;

						if (!config.snaponlyassets)
							datapath = Application.dataPath.Replace("/Assets", "");
						else
							datapath = Application.dataPath;

						config.Save();
						openMode = oM.basic;
					}
				}
			}
			else
			{
				EditorGUI.ProgressBar(new Rect(3, 45, position.width - 6, 20), 100f, workingStatus);
				EditorGUI.LabelField(new Rect(3, 70, position.width - 6, 20), "<color=red><b>DONT CLOSE UNITY!</b></color>", style);
			}
		}
		else
			GUILayout.Label("Please input path to folder with kopia.exe");
	}

	async void OpenFiles()
	{
		files = new List<KPSL_Editor_File>();

		string id = "";
		foreach (string s in fileList)
			id += s + "/";
		string output = "";
		await Task.Run(() => output = runProc("list -l " + id));
		foreach (string s in output.Split('\n').ToList())
		{
			if (!string.IsNullOrEmpty(s) && (!s.Contains(".meta") || config.showmeta))
			{
				List<string> str = s.Split(" ").ToList();

				//check is that directory or file
				bool file = true;
				if (str[0][0] == 'd')
				{
					file = false;
				}

				//look for data
				for (int i = 0; i < str.Count; i++)
				{
					if (int.TryParse(str[i], out int r))
					{
						files.Add(new KPSL_Editor_File());
						files[files.Count - 1].folder = !file;
						files[files.Count - 1].size = r;
						files[files.Count - 1].ind = str[i + 4];
						if (file)
						{
							for (int n = i + 7; n < str.Count; n++)
								if (n != str.Count - 1)
									files[files.Count - 1].name += str[n] + " ";
								else
									files[files.Count - 1].name += str[n];
						}
						else
							for (int n = i + 6; n < str.Count; n++)
								if (n != str.Count - 1)
									files[files.Count - 1].name += str[n] + " ";
								else
									files[files.Count - 1].name += str[n];

						break;
					}
				}
			}
		}

		files.Sort((x, y) =>
		{
			if (x.name == null && y.name == null) return 0;
			else if (x.name == null) return -1;
			else if (y.name == null) return 1;
			else return x.name.CompareTo(y.name);
		});
	}

	async void NewSnapshotAsync()
	{
		if (!working)
		{
			working = true;
			workingStatus = "Creating new snapshot...";
			await Task.Run(() => runProc("snapshot create " + "\"" + datapath + "\""));
			working = false;
			CheckAndLoad();
		}
	}

	async void DeleteSnapshot(KPSL_Editor_Snapshots s)
	{
		if (!working)
		{
			working = true;
			workingStatus = "Deleting snapshot " + s.data + " " + s.ind + "...";
			await Task.Run(() => runProc("snapshot delete " + s.ind + " --delete"));
			working = false;
			CheckAndLoad();
		}
	}

	async void RestoreSnapshot(KPSL_Editor_Snapshots s)
	{
		if (!working)
		{
			working = true;
			workingStatus = "Restoring snapshot " + s.data + " " + s.ind + "...";
			await Task.Run(() => runProc("snapshot restore " + s.ind + " \"" + datapath + "\"" + (config.dontrewrite? "" : " --overwrite-directories --overwrite-files")));
			working = false;
			CheckAndLoad();
		}
	}

	async void RestoreSnapshot(string filepath, string realfilepath)
	{
		if (!working)
		{
			working = true;
			workingStatus = "Restoring snapshot file " + filepath + "...";
			//UnityEngine.Debug.Log("snapshot restore " + "\"" + filepath + "\" \"" + datapath + "/" + realfilepath + "\" --overwrite-directories --overwrite-files");
			await Task.Run(() => runProc("snapshot restore " + "\"" + filepath + "\" \"" + datapath+"/"+ realfilepath + "\"" + (config.dontrewrite? "" : " --overwrite-directories --overwrite-files")));
			working = false;
			CheckAndLoad();
		}
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
		
		string output = runProc("snapshot list " + "\"" + datapath + "\"");

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
public class KPSL_Editor_File
{
	public string name;
	public int size;
	public string ind;
	public bool folder;
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
	public bool showmeta;
	public bool snaponlyassets;
	public bool dontrewrite;

	public void Load()
	{
		if (FilesSet.CheckFile(Application.dataPath + "/SLywnowAssets/KopiaUnity/config.cfg"))
		{
			KPSL_Editor_MainConfig c = JsonUtility.FromJson<KPSL_Editor_MainConfig>(FilesSet.LoadStream(Application.dataPath + "/SLywnowAssets/KopiaUnity/config.cfg", false, false));
			serverdir = c.serverdir;
			showmeta = c.showmeta;
			snaponlyassets = c.snaponlyassets;
			dontrewrite = c.dontrewrite;
		}
	}

	public void Save()
	{
		FilesSet.SaveStream(Application.dataPath + "/SLywnowAssets/KopiaUnity/config.cfg", JsonUtility.ToJson(this, true));
	}

	public KPSL_Editor_MainConfig Copy()
	{
		KPSL_Editor_MainConfig ret = new KPSL_Editor_MainConfig();

		ret.serverdir = serverdir;
		ret.showmeta = showmeta;
		ret.snaponlyassets = snaponlyassets;
		ret.dontrewrite = dontrewrite;

		return ret;
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
