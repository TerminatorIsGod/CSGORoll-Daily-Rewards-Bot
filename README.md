# CSGORoll Daily Rewards Bot / Collector

Developed By: TerminatorIsGod  
Official Github URL: [https://github.com/TerminatorIsGod/CSGORoll-Daily-Rewards-Bot](https://github.com/TerminatorIsGod/CSGORoll-Daily-Rewards-Bot)  
Tutorial (OUTDATED): [CSGORoll Daily Rewards Bot Tutorial](https://www.youtube.com/watch?v=B2sp25ok0VI) <br>
Discord Server: [https://discord.gg/fy5ARFSYHV](https://discord.gg/fy5ARFSYHV)

For support create an [issue](https://github.com/TerminatorIsGod/CSGORoll-Daily-Rewards-Bot/issues) or join the [discord server](https://discord.gg/fy5ARFSYHV)

<br>

### Cheap Hosting Available
Interested in having the collector hosted for you? Join the [discord server](https://discord.gg/fy5ARFSYHV) for more details and to purchase.
<br><br>

**If you find this program helpful feel free to [leave a vouch](https://www.reddit.com/r/CSGORollServices/comments/1be07ic/csgoroll_daily_rewards_bot/)**

**CSGORoll has recently changed how affiliate/deposit codes are applied resulting in forcing codes. If you would like a version that doesn't force the affiliate code you can request it in our [discord server](https://discord.gg/fy5ARFSYHV).**

# Table of Contents

1. [What is this program?](#what-is-this-program)
2. [Current Features](#features)
3. [What's the catch/Why is it free?](#whats-the-catchwhy-is-it-free)
4. [Does CSGORoll allow this?](#does-csgoroll-allow-this)
5. [How to Use](#how-to-use)
6. [Config.json](#config) <br>
   6.1. [Auto Update](#auto-update) <br>
   6.2. [Multiple Instances](#multiple-instances) <br>
   6.3. [Proxy](#proxy) <br>
   6.4  [Unbox Cases](#unbox-cases) <br>
   6.5. [Daily Rewards Battle](#daily-rewards-battle) <br>
        &nbsp;&nbsp;&nbsp;&nbsp;6.5.1 [Cases to Battle](#cases-to-battle) <br>
        &nbsp;&nbsp;&nbsp;&nbsp;6.5.2 [Gamemode](#gamemode) <br>
        &nbsp;&nbsp;&nbsp;&nbsp;6.5.3 [Numnber of Players](#number-of-players) <br>
        &nbsp;&nbsp;&nbsp;&nbsp;6.5.4 [Number of Teams](#number-of-teams) <br>
        &nbsp;&nbsp;&nbsp;&nbsp;6.5.5 [PvP Mode](#pvp-mode) <br>
   6.6. [Trigger Time](#trigger-time) <br>
   6.7. [Discord Webhook Integration](#discord-webhook-integration) <br>
   6.8  [Default Config File](#default-config-file)
9. [Common Issues](#common-issues)
10. [How It Works](#how-it-works)
11. [Commands/Console](#commandsconsole)
12. [Safety/Other](#safetyother)

## What is this program?

This program automates the collection and sells your free daily cases on CSGORoll. It does this by using Microsoft's WebView2 library to simulate a web browser. We then run some simple javascript to do stuff such as verify you are signed in, check how much longer is left to claim the cases, and to open the cases themselves. One useful feature to enhance its automation capabilities is having it so if you get signed out of CSGORoll, it will sign you back in as long as you are still signed into Steam. At no point do the developers have access to anything, it works as if you are using your regular web browser.

## Features

1. Automatically collects your CSGOROll daily rewards as long as your PC is on/asleep
2. Supports daily case battles
3. Discord webhook integration
4. HTTP and HTTPS proxy support
5. Auto updates
6. Multiple instances
7. Select which cases you want to be automatically collected

## Does CSGORoll allow this?

While CSGORoll's terms and conditions do not expressly prohibit the use of scripts for automating the collection of free daily cases, it's important to understand that the program operates within the existing framework of the website. We want to emphasize that we assume no responsibility for any actions CSGORoll may take regarding your account. Users are encouraged to use the program responsibly and adhere to the terms and conditions of CSGORoll.

## How to Use

1. (optional) Modify config.txt to your liking, (more info about config.json)[#config]
2. Run the program
3. Sign into CSGORoll using Steam
4. Let the program do the rest

**Note:** Ensure your computer is not turned off during the daily case collection time. Sleep mode is acceptable. If your computer is turned off, the program won't run, preventing the collection of your free daily cases.
<br><br>

## Config

Any line in the config that has ```info``` in the variable name is there to provide information about the variable below and does nothing.
<br><br>

### Auto Update

The auto update feature can be disabled by changing ```true``` to ```false```

```
"autoUpdateProgram": true,
```
<br><br>

### Multiple Instances
To have multiple instances of the program and to prevent them from overwriting each other, first download the program again in another folder so you have multiple exe files and config files in different places. In each of the config files set a unique value for
```
"taskSchedulerTaskName": "CSGORoll Daily Collector",
```
Run the exe, and now you have multiple instances of it.
<br><br>

### Proxy
The proxy can be configured with these four lines, as long as ```proxyAddress``` is empty then it will be disabled. The program only supports ```http``` or ```https```, sockets won't work. <br>
```proxyAddress```   the proxy IP <br>
```proxyType```      the proxy type, either ```http``` or ```https``` <br>
```proxyUsername```  username for authentication if required <br>
```proxyPassword```  password for authentication if required <br>
<br>
```
  "proxyAddress": "",
  "proxyTypeInfo": "Proxy type options: 'http' or 'https'",
  "proxyType": "",
  "proxyUsername": "",
  "proxyPassword": "",
```
<br><br>



### Unbox Cases 
<br>
Configure what cases, the risk and the order you want to unbox them. It should be pretty self explainatory, ```level``` is the case level, ```risk``` is the risk percent you want. <br> <br>

Default List: <br>
```
"casesToOpen": [
    {
      "level": "2",
      "risk": "50"
    },
    {
      "level": "10",
      "risk": "50"
    },
    {
      "level": "20",
      "risk": "50"
    },
    {
      "level": "30",
      "risk": "50"
    },
    {
      "level": "40",
      "risk": "50"
    },
    {
      "level": "50",
      "risk": "50"
    },
    {
      "level": "60",
      "risk": "50"
    },
    {
      "level": "70",
      "risk": "50"
    },
    {
      "level": "80",
      "risk": "50"
    },
    {
      "level": "90",
      "risk": "50"
    },
    {
      "level": "100",
      "risk": "50"
    }
  ],
```
<br><br>

### Daily Rewards Battle 
<br>
Leave as is if you don't care to setup daily case battles.


#### Cases to Battle
```
"casesToPvpbattle": [],
```
<br>

The ```casesToOpen``` list works the same as the [unbox cases](#unbox-cases) feature above. It is highly recommended to have the cases in both the battle list and the regular case opening list so If something goes wrong for example a case was already opened in the list. Then it will fallback to just opening the cases as normal. <br>

Example List:
```
"casesToPvpbattle": [
    {
      "level": "2",
      "risk": "50"
    },
    {
      "level": "10",
      "risk": "50"
    },
    {
      "level": "20",
      "risk": "50"
    }
  ],
```

<br><br>

#### Gamemode: 
<br>
```"pvpStrategy": "HIGHEST_SUM",``` <br><br>

Options: <br>
Regular:          ```HIGHEST_SUM``` <br>
Crazy:            ```LOWEST_SUM``` <br>
Clutch Regular:   ```HIGHEST_BET_PAYOUT``` <br>
Clutch Crazy:     ```LOWEST_BET_PAYOUT``` <br>
Termianl Regular: ```HIGHEST_LAST_BET_PAYOUT``` <br>
Terminal Crazy:   ```LOWEST_LAST_BET_PAYOUT``` <br>

<br>

#### Number of Players: 
<br>
```"pvpNumOfPlayers": 4,``` <br><br>

Options: <br>
```2``` number of teams must be either ```1``` or ```2``` <br>
```3``` number of teams must be either ```1``` or ```3``` <br>
```4``` number of teams must be either ```1``` or ```2``` <br>
```6``` number of teams must be either ```2``` or ```3``` <br>

<br><br>

#### Number of Teams: 
<br>
```"pvpNumOfTeams": 2,```<br><br>

For shared mode set this to ```1``` <br><br>

Options: <br>
```1``` number of players can either be ```2```, ```3```, ```4``` <br>
```2``` number of players can either be ```2```, ```4```, ```6``` <br>
```3``` number of players can either be ```3```, ```6``` <br>
```4``` number of players can either be ```4``` <br>

#### PvP Mode
<br>
```"pvpMode": "TEAM",```
<br>
This will automatically be set based on the above settings. <br> <br>

Options: <br>
```TEAM```   for team battles
```SINGLE``` for no teams

<br><br>

### Trigger Time 
<br>
By default the program will trigger when your cases become available but sometimes you might want it to run at a different time. <br><br>

The format is ```HH:MM:SS```, it uses 24hr clock and is in local time. ```00:00:00``` sets it to automatic. 

```
  "triggerTime": "00:00:00",
```

### Discord Webhook Integration
Be careful of people bruteforcing discord webhook urls! They can spam your discord channel with whatever they want. <br><br>

To setup the discord webhook first make sure you have created a webhook inside and have the URL. After you have the URL change ```enableDiscordWebhook``` from ```false``` to ```true``` then replace the value of ```discordWebhookURL``` with your URL. <br>
For exmaple ```"discordWebhookURL": "https://discord.com/api/webhooks/1364332891156512898/cEHRS5i46LHI-U0uZUW3Mfa1QzUx60zDl1CZw1jsW8l0wPjM_COPgwrW7An-Oua62lFa",``` <br>
```
  "enableDiscordWebhook": false,
  "discordWebhookURL": "be careful people can brute force discord webhooks and spam messages",
```
<br><br>

### Default Config File
```
{
  "taskSchedulerTaskName": "CSGORoll Daily Collector",
  "autoUpdateProgram": true,
  "proxyAddress": "",
  "proxyTypeInfo": "Proxy type options: 'http' or 'https'",
  "proxyType": "",
  "proxyUsername": "",
  "proxyPassword": "",
  "infoString": "You can remove/add/reorder the list below based on how/which cases you want to open. PvP battles will run first, incase that fails I recommend keeping them here too",
  "casesToOpen": [
    {
      "level": "2",
      "risk": "50"
    },
    {
      "level": "10",
      "risk": "50"
    },
    {
      "level": "20",
      "risk": "50"
    },
    {
      "level": "30",
      "risk": "50"
    },
    {
      "level": "40",
      "risk": "50"
    },
    {
      "level": "50",
      "risk": "50"
    },
    {
      "level": "60",
      "risk": "50"
    },
    {
      "level": "70",
      "risk": "50"
    },
    {
      "level": "80",
      "risk": "50"
    },
    {
      "level": "90",
      "risk": "50"
    },
    {
      "level": "100",
      "risk": "50"
    }
  ],
  "infoStringpvp": "This list behaves the same as above but allows you to battle it. Providing invalid settings will cause it to try and correct it!",
  "casesToPvpbattle": [],
  "infoStringPvpStrategyInfo1": "Strategy Options:",
  "infoStringPvpStrategyInfo2": "'HIGHEST_SUM' (Regular)  'LOWEST_SUM' (Crazy)",
  "infoStringPvpStrategyInfo3": "'HIGHEST_BET_PAYOUT' (Clutch Regular)  'LOWEST_BET_PAYOUT' (Clutch Crazy)",
  "infoStringPvpStrategyInfo4": "'HIGHEST_LAST_BET_PAYOUT' (Terminal Regular)  'LOWEST_LAST_BET_PAYOUT' (Terminal Crazy)",
  "pvpStrategy": "HIGHEST_SUM",
  "infoStringPvpNumOfPlayers": "Options: 2, 3, 4, 6",
  "pvpNumOfPlayers": 4,
  "infoStringPvpNumOfTeams1": "Options: 1, 2, 3, 4",
  "infoStringPvpNumOfTeams2": "if 1 number of players can be either 2, 3, 4",
  "infoStringPvpNumOfTeams3": "if 2 number of players can be either 2, 4, 6",
  "infoStringPvpNumOfTeams4": "if 3 number of players can be either 3, 6",
  "infoStringPvpNumOfTeams5": "if 4 number of players can only be either 4",
  "pvpNumOfTeams": 2,
  "infoStringPvpMode": "Options: 'TEAM' or 'SINGLE'",
  "pvpMode": "TEAM",
  "triggerTime": "00:00:00",
  "disableAffiliate": "why would you want to disbale this?",
  "enableDiscordWebhook": false,
  "discordWebhookURL": "be careful people can brute force discord webhooks and spam messages",
  "identifier": "for special hosting feature that can be found in the discord server",
  "commport": 0,
  "configVersion": "2.0.0"
}
```





## Common Issues

### The program isn't closing itself
Sometimes when CSGORoll has special events it will break the program and it won't close automatically. Create an issue or contact me on Discord: TerminatorIsGod

### My daily cases are no longer being collected!
If you have moved the exe file then you need to rerun the program.

### The program isn't automatically launching itself when im not at my computer!
Your computer must be either on or in sleep mode. If your computer is off then the program won't work. Make sure you have also signed in at least once since turning the PC on.

### The program isn't automatically closing itself!
If the program isn't closing itself after 3 minutes then check to see if you are signed into CSGORoll. If you are signed in then try swapping pages and then going back to the rewards page.

### My internet is really, really slow so the program doesn't work.
The program should still work no matter how bad your internet since it searches for specific elements on the page and waits for them. Create an issue or join our [discord server]()

## How It Works

This program leverages Microsoft's WebView2 library to simulate a Microsoft Edge browser. By injecting JavaScript, it automates the process of opening daily cases on CSGORoll. Coupled with Windows Task Scheduler, it runs daily even during sleep mode.

## Commands/Console

There's little need for manual intervention as everything is automated. Using commands unnecessarily may lead to issues. We take no responsibility or provide support in such cases.

### Available Commands:

- `goTo <url>`: Navigates to the specified URL.
- `autoQuit [bool]`: Toggles automatic program closure.

## What's the catch/Why is it free?

The 'catch' lies in the automatic setting of your affiliate code. This decision not only enables us to provide the program for free but also comes at no cost to you. By setting your affiliate code, you contribute to supporting the developer to continue making updates and other programs just like this one.

## Safety/Other

- Do not trust any downloads to this software that isn't from the official github link.
- This program does not require any elevated privileges and shouldn't trigger anti-virus programs.
- Do not enter commands unless you accept the risks.
- The program does not directly save or send any user data. Microsoft manages any data storage or transmission.

