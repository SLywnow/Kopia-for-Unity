using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class KUSL_Main : MonoBehaviour
{
	private void Start()
	{

		var proc = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				FileName = "D:\\Usr\\Desktop\\раб стол\\KopiaUI\\resources\\server\\kopia.exe",
				Arguments = "snapshot list",
				UseShellExecute = false,
				RedirectStandardOutput = true,
				CreateNoWindow = true,
				Verb = "runas"
			}
		};

		proc.Start();

		UnityEngine.Debug.Log(proc.StandardOutput.ReadToEnd());
	}
}
