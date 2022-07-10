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
Warning, that may freeze Unity, especially at first time, just eait a few minutes (or seconds)

### **Refresh list**
Refreshes list of all snapshots

### **Snapshot**
You can open additional info about snapshot, also, you can remove or restore this snapshot (warning, recovery does not delete new files, only overwrites existing ones)


# **TO DO**
- Create system to view snapshot's files
- Create system to autorun server
- Move working with files to another thread to fix unity freezing
- Fix bugs
