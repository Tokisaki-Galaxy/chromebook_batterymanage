# chromebook_batterymanage

A battery charge threshold tool for Chromebooks (running Windows). It stops charging once a set limit is reached to prolong battery life.

The core utility, `ectool.exe`, which handles the battery charging state, is a part of the **coreboot project**. This program simply provides a convenient wrapper and automation service for it.

**Requirement:** Both `ectool.exe` and the main application (`chromebook_batterymanage.exe`) must be in the same directory to function correctly.

## How to Use

### GUI (Recommended)

<img width="273" height="166" alt="image" src="https://github.com/user-attachments/assets/bbc123cf-aaab-40d2-8862-de82129e4941" />

The GUI provides a user-friendly way to manage the `chromebook_batterymanage` service. It allows you to:
*   Install and uninstall the service.
*   Start and stop the service.
*   Run the service temporarily without installation.
*   Open the project's GitHub page.

The default installation path is `C:\batteryManage`.

### Manual Mode (Not recommended)

#### Manual Start

Ensure both `chromebook_batterymanage.exe` and `ectool.exe` are in the same directory, then run `chromebook_batterymanage.exe`.

#### Automatic Start (via Task Scheduler)

1.  Place both files in a permanent directory.
2.  Open **Task Scheduler** in Windows.
3.  Create a new task that triggers at user logon.
4.  Set the action to start a program and point it to `chromebook_batterymanage.exe`.

The application checks the battery level every 2 minutes. If the charge is greater than or equal to the threshold (80% by default), it commands the embedded controller to stop charging (idle mode). The program is very lightweight and consumes minimal system resources while idle.

## Customizing the Charge Threshold

You can set a custom charge threshold by passing it as a command-line argument.

For example, to set the limit to 60%, you would run the program like this:
`chromebook_batterymanage.exe 60`

This is particularly useful when setting up the task in Task Scheduler; you can add the argument in the "Add arguments (optional)" field.

If no argument is provided, the default threshold of **80%** will be used.
