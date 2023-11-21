# chromebook_batterymanage
适用于chromebook的电池阈值，充电到一定程度停止充电

核心用于调整电池充电状态的`ectool.exe`是coreboot项目组的，本程序只是套了个壳方便使用。

https://github.com/Tokisaki-Galaxy/chromebook_batterymanage

需要extool.exe和主程序在同一目录下。

## 教程

### 手动启动

确保两个文件放在同一目录，点击`chromebook_batterymanage.exe`。

### 自动启动

确保两个文件放在同一目录，然后在"任务计划程序"里面创建计划，开机启动，执行`chromebook_batterymanage.exe`

预期情况，每隔3分钟检测一次电池电量，若大于等于80%(默认设置)，则停止向电池供电。不探测的时候后台休眠，占用极低。

### 自定义电池充电阈值

命令行选项（可选）

在添加任务计划程序里面，`chromebook_batterymanage.exe 60`可以设置阈值为60%，以此类推。不设置默认80%。
