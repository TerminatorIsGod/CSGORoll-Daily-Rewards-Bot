# CSGORoll Daily Rewards Bot / Collector

Developed By: TerminatorIsGod  
Official Github URL: [https://github.com/TerminatorIsGod/CSGORoll-Daily-Rewards-Bot](https://github.com/TerminatorIsGod/CSGORoll-Daily-Rewards-Bot)  
Tutorial: [CSGORoll Daily Rewards Bot Tutorial](https://www.youtube.com/watch?v=B2sp25ok0VI)

For support either create an [issue](https://github.com/TerminatorIsGod/CSGORoll-Daily-Rewards-Bot/issues) or message me on discord: TerminatorIsGod

**If you find this program helpful feel free to [leave a vouch](https://www.reddit.com/r/CSGORollServices/comments/1be07ic/csgoroll_daily_rewards_bot/)**

# Table of Contents

1. [What is this program?](#what-is-this-program)
2. [What's the catch/Why is it free?](#whats-the-catchwhy-is-it-free)
3. [Does CSGORoll allow this?](#does-csgoroll-allow-this)
4. [How to Use](#how-to-use)
5. [Common Issues](#common-issues)
6. [How It Works](#how-it-works)
7. [Commands/Console](#commandsconsole)
8. [Safety/Other](#safetyother)

## What is this program?

This program automates the collection and sells your free daily cases on CSGORoll. It does this by using Microsoft's WebView2 library to simulate a web browser. We then run some simple javascript to do stuff such as verify you are signed in, check how much longer is left to claim the cases, and to open the cases themselves. One useful feature to enhance its automation capabilities is having it so if you get signed out of CSGORoll, it will sign you back in as long as you are still signed into Steam. At no point do the developers have access to anything, it works as if you are using your regular web browser.

## Does CSGORoll allow this?

While CSGORoll's terms and conditions do not expressly prohibit the use of scripts for automating the collection of free daily cases, it's important to understand that the program operates within the existing framework of the website. We want to emphasize that we assume no responsibility for any actions CSGORoll may take regarding your account. Users are encouraged to use the program responsibly and adhere to the terms and conditions of CSGORoll.

## How to Use

1. Run the program
2. Read the initalization page and press 'Confirm'
3. Sign into CSGORoll using Steam
4. Watch your daily cases get claimed

**Note:** Ensure your computer is not turned off during the daily case collection time. Sleep mode is acceptable. If your computer is turned off, the program won't run, preventing the collection of your free daily cases.

## Common Issues

### The program isn't closing itself
Sometimes when CSGORoll has special events it will break the program and it won't close automatically.

### My daily cases are no longer being collected!
If you have moved the exe file then you need to rerun the program.

### The program isn't automatically launching itself when im not at my computer!
Your computer must be either on or in sleep mode. If your computer is off then the program won't work. Make sure you have also signed in at least once since turning the PC on.

### The program isn't automatically closing itself!
If the program isn't closing itself after 3 minutes then check to see if you are signed into CSGORoll. If you are signed in then try swapping pages and then going back to the rewards page.

### My internet is really, really slow so the program doesn't work.
Download the 'slow internet' version of the program.

## How It Works

This program leverages Microsoft's WebView2 library to simulate a Microsoft Edge browser. By injecting JavaScript, it automates the process of opening daily cases on CSGORoll. Coupled with Windows Task Scheduler, it runs daily even during sleep mode.

## Commands/Console

There's little need for manual intervention as everything is automated. Using commands unnecessarily may lead to issues. We take no responsibility or provide support in such cases.

### Available Commands:

- `goTo <url>`: Navigates to the specified URL.
- `autoQuit [bool]`: Toggles automatic program closure.

## What's the catch/Why is it free?

The 'catch' lies in the automatic setting of your affiliate code. This strategic decision not only enables us to provide the program for free but also comes at no cost to you. By setting your affiliate code, you contribute to supporting the developer to continue making programs just like this one.

## Safety/Other

- Do not trust any downloads to this software that isn't from the official github link.
- This program does not require any elevated privileges and shouldn't trigger anti-virus programs.
- Anything that the program does automatically is safe and verified, but it's always better to be safe than sorry and not trust some random free program. The correct URL should always be shown right above the browser but to be safe and verify that it is correct you can always, right-click on a page and select 'inspect' to view the website's URL.
- The program does not directly save or send any user data. Microsoft manages any data storage or transmission.

