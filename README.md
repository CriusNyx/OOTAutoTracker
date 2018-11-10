# OOT Randomizer Auto Tracker

This software is used to track which chests, and collectibles have been visited in ocarina of time.
It was developed raplidly, using resources available on the web, to auto track Ocarina of Time.

This is an Alpha release, intended for testing, and further development.

It's been designed with flexability and readability in mind.

# Limitations

Currently, this software only works to track retroarch saves file for OOT.
Currently, it only has a command line interface.

# Usage

Open the OOT auto tracker, and then open retroarch.<br />
Start OOT, and either<br />
	&nbsp;&nbsp;&nbsp;&nbsp;-Create a new file<br />
	&nbsp;&nbsp;&nbsp;&nbsp;-Load an existing file, and save once.<br />
In the auto tracker command line, enter the command autotrack<br />
The software will begin auto tracking the oot save.<br />

*If auto tracking does not appear to work correctly, make sure "SRAM AutoSave Interval" is set to a low number in retroarch, such as 2 seconds<br />This may degrade preformance on slow machines*

*If retroarch is installed in a non standard directory, use the command setdir [directory] to set the auto tracker to the new directory*

# Other software commands
	-exit: exit the program
	-quit: exit the program
	-q: exit the program
	-print [sceneIndex]: Print the data at the specified scene index
	-check [sceneIndex]: Prints if the specified scene is complete or not
	-checkAll: Prints the status of all chests
	-printMap: prints the memory map for the save file
	-printFilepath: print the filepath to the current active file
	-printFile: print the filepath to the current active file
	-printActive: print the filepath to the current active file
	-find [search pattern]: prints the scene index of all scenes that match the pattern
	-find . will print all scenes
	-autotrack: begin auto tracking, and auto updating
	-cls: clear the console screen
	-events: print a list of events
	-setdir [DirectoryPath]: sets the location of the retro arch saves path for non standard retroarch installations
	-resetdir: resets the directory to the standard retroarch installation
	-help: print the help string, with a list of commands