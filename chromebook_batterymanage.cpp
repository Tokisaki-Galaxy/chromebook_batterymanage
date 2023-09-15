// chromebook_batterymanage.cpp : 此文件包含 "main" 函数。程序执行将在此处开始并结束。
//

#include <iostream>
#include <Windows.h>
#include <chrono>
#pragma comment(linker, "/subsystem:\"windows\" /entry:\"mainCRTStartup\"")//运行时不显示窗口

using namespace std::chrono;

int battery_monitor() {
    LPSYSTEM_POWER_STATUS power = new _SYSTEM_POWER_STATUS();
    int ret = GetSystemPowerStatus(power);
    int percent = (int)power->BatteryLifePercent;
    delete power;
    if (ret == 0) //表示获取失败
        return 0;
    return percent;
}

int main()
{

    while (true) {
        auto start = high_resolution_clock::now();

        // 执行任务
        if (battery_monitor() >= 80)
            ShellExecute(NULL, L"open", L"ectool.exe", L"chargecontrol idle", NULL, SW_HIDE);
        else
            ShellExecute(NULL, L"open", L"ectool.exe", L"chargecontrol normal", NULL, SW_HIDE);
        std::cout << battery_monitor() << std::endl;

        std::this_thread::sleep_for(minutes(3));
    }
    return 0;
}

// 运行程序: Ctrl + F5 或调试 >“开始执行(不调试)”菜单
// 调试程序: F5 或调试 >“开始调试”菜单

// 入门使用技巧: 
//   1. 使用解决方案资源管理器窗口添加/管理文件
//   2. 使用团队资源管理器窗口连接到源代码管理
//   3. 使用输出窗口查看生成输出和其他消息
//   4. 使用错误列表窗口查看错误
//   5. 转到“项目”>“添加新项”以创建新的代码文件，或转到“项目”>“添加现有项”以将现有代码文件添加到项目
//   6. 将来，若要再次打开此项目，请转到“文件”>“打开”>“项目”并选择 .sln 文件
