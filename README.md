# chromebook_batterymanage
适用于chromebook的电池阈值，充电80%停止充电

核心用于调整电池充电状态的`ectool.exe`是我从芳灵工具箱那里弄来的，只是套了个壳。

https://github.com/Tokisaki-Galaxy/chromebook_batterymanage

需要extool.exe和主程序在同一目录下。

## 教程

### 手动启动

确保两个文件放在同一目录，点击`chromebook_batterymanage.exe`。

### 自动启动

确保两个文件放在同一目录，然后在"任务计划程序"里面创建计划，开机启动，执行`chromebook_batterymanage.exe`

预期情况，每隔3分钟检测一次电池电量，若大于等于80%，则停止向电池供电。不探测的时候后台休眠，占用极低。
