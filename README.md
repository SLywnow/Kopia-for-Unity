# Kopia for Unity asset
![Icon](https://i.imgur.com/7dzbZPl.png)
Asset for convenient use of [Kopia](https://github.com/kopia/kopia "Kopia") in Unity

# **How to install**
1. Launch Kopia GUI or launch Kopia server
2. Install Asset: merge the Assets folder of your project with the Assets folder of this repository and install [SLywnow basic](https://github.com/SLywnow/slywnow_basic)
3. Open the SLywnow/Kopia UI window
4. Set path to kopia.exe file, example: C:\KopiaUI\resources\server (only folder, not file) and press Save & Check
5. If you set right path and server started, then on top you'll see "Connected" text. Now you're ready to use asset


# **How to use**
### **Create new snapshot**
Creates new snapshot, it'll copy full project folder, except Temp

### **Refresh list**
Refreshes list of all snapshots

### **Snapshot**
You can open additional info about snapshot, also, you can remove or restore this snapshot. Also, you can see files in snapshot, see [File browsing](https://github.com/SLywnow/Kopia-for-Unity/blob/main/README.md#file-browsing) for more info.

### **File browsing**
You can see all files and directories in snapshot and their size. You can restore any file or directory by pressing "Restore" button on right. To move back press "To the top" or "Close" on top.
Also, on top you can see current path.

### **While working**
When asset working all GUI vanishes and you see status of current task

### **Settings**
To open settings click on settings icon on top right

- Path to exe: path to kopia.exe file, example: C:\KopiaUI\resources\server (only folder, not file) 
- Show .meta files: shows .meta files in the file browser
- Snapshot only Asset folder: create snapshot only for Assets folder instead of whole project
- Don't rewrite files when restore: Only new or deleted files will be writen when you restore data

Save button will save new settings, Cancel will return data back to old config

All data stores in config.cfg file in KopiaUnity's folder

### **Console**
To open console click on console icon on top right

You can enter any kopia commands here, returns will be showed in Unity's console. But I recommends to use classical console for that.

# **TO DO**
- Create system to autorun server
- Fix bugs
- Add more console commands in help
- Add confirm before delete/restore and settings to show it