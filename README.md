Asset for convenient use of [kopia](https://github.com/kopia/kopia "kopia")

# **How to install**
1. Launch kopia GUI or launch kopia server
2. Install Asset: merge the Assets folder of your project with the Assets folder of this repository and install [SLywnow basic](https://github.com/SLywnow/slywnow_basic)
3. Open the SLywnow/Kopia UI window
4. Set path to kopia.exe file, example: C:\KopiaUI\resources\server (only folder, not file) and press Save & Check
5. If you set right path and server started, then button "Create new snapshot" appears


# **How to use**
### **Create new snapshot**
Creates new snapshot, it'll copy full project folder, except Temp

### **Refresh list**
Refreshes list of all snapshots

### **Snapshot**
You can open additional info about snapshot, also, you can remove or restore this snapshot. Also, you can see files in snapshot, see [File browsing](https://github.com/SLywnow/Kopia-for-Unity/blob/main/README.md#file-browsing) for more info.

### **File browsing**
You can see all files and directories in snapshot. You can restore any file or directory by press "Restore" button on right. To move back press "To the top" or "Close" on top.

### **While working**
When asset working all GUI vanish  and you see status of current task


# **TO DO**
- Create system to autorun server
- Fix bugs
- Fix bug "Getting control 4's position in a group with only 4 controls when doing repaint" when open file browser
- Add settings:
	- Show .meta files in file browser
	- Dont rewrite directories and files
	- Create snapshow only for Assets folder
- Add console line
