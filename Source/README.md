# POSH-HellBot Source Readme

## The code can be found in the following folder:
Source\scbot.git\POSH-StarCraftBot\behaviours

## To run the Bot simply run the "POSH-HellBot.bat" file located at:
Source\scbot.git\POSH-Launcher\bin\Release
### Note: You will Need to install StarCraft: BroodWar with patch 1.16.1 
### plus ChaosLauncher which can be found [here](http://www.teamliquid.net/forum/brood-war/65196-chaoslauncher-for-1161)
### Note: The Bot must be run in admin mode otherwise it will not connect to the game.
The batch file should ask for admin. if it does not then it will not work, to circumven this you must run the POSH-Launcher.exe
in the command line while running in admin, with the following argument after "-a=POSH-HellBot.dll POSH-HellBot".


## To run the code "From Source" please follow this [link.](https://github.com/suegy/bwapi-mono-bridge2/wiki/StarCraft-Setup-BWAPI)

Once there follow the steps until you have closed ChaosLauncher, then follow this [link](https://github.com/suegy/bwapi-mono-bridge2/wiki/MonoBridge-Setup) and follow the instructions.

Do not complete part b of this page, instead follow this [link](https://github.com/suegy/bwapi-mono-bridge2/wiki/CsharpAI).

The once there follow the instructions until it asked you to create a "scbot.git" folder as you will be using the "scbot.git" folder located within this project instead.

Located within this folder should be two other folders called:
#### POSH-Launcher
#### POSH-StarCraftBot

Open the bwapi-clr-client Solution in \bwapi-mono-bridge2.git\Source\bwapi-clr-client

NOTE: currently all bots in Client Mode require Visual Studio to be run as Administrator. 

So, if you want to test your agent you need to start VS with admin rights. 

Otherwise your client is not able to inject the information into the game.

If you do not want to, or forgot to setup the POSHBots you will get an error message that two projects could > not be found: POSH-Launcher and POSH-StarCraftBot

If the POSH projects cannot be found within Visual Studio remove both projects but then add the two with the correct path by right clicking on the Solution in 

Visual Studio -> "Add..." -> "Existing Project" and then select the directory where you put each project.

now simply start the ChaosLauncher in admin mode and select the "Release" injector.

compile first bwapii-native then bwapi-clr and then right click on POSH-Launcher and select "Debug" and run in debug mode.


## The link to the GitHub Repository:
[link](https://github.com/James120393/Hellbot)


## Then select open and navigate to:
"Source\scbot.git\POSH-StarCraftBot\library\plans"
and select any of the plans within this folder.
