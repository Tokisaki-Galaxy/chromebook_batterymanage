// chromebook_batterymanage.cpp : 此文件包含 "main" 函数。程序执行将在此处开始并结束。
//

#include <iostream>
#include <string>
#include <vector>
#include <chrono>
#include <thread>
#include <Windows.h>
#include <Shlwapi.h>
#include <stdexcept>

#ifndef _DEBUG
#pragma comment(linker, "/subsystem:\"windows\" /entry:\"mainCRTStartup\"")
#endif

#pragma comment(lib, "Shlwapi.lib")
using namespace std::chrono_literals;
HANDLE hMutex = NULL;
const wchar_t* mutexName = L"Global\\ChromebookBatteryManagerAppMutex_UniqueName{E2A27A6E-7E3B-4C9C-A2E1-765A9B8706B9}";

const wchar_t* CMD_CHARGE_NORMAL = L"chargecontrol normal";
const wchar_t* CMD_CHARGE_IDLE = L"chargecontrol idle";
const wchar_t* CMD_FAN_MAX = L"fanduty 100";
const wchar_t* CMD_FAN_AUTO = L"autofanctrl";

/**
 * @brief 获取当前可执行文件的目录路径
 * @return 包含目录的 wstring，如果失败则为空字符串
 */
std::wstring GetExecutableDirectory() {
    wchar_t path[MAX_PATH] = { 0 };
    if (GetModuleFileName(NULL, path, MAX_PATH) == 0) {
        return L"";
    }
    PathRemoveFileSpec(path);
    return std::wstring(path);
}

/**
 * @brief 执行 ectool.exe 并返回其退出码
 * @param ectoolPath ectool.exe 的完整路径
 * @param args 要传递给 ectool.exe 的参数
 * @return 进程的退出码。如果进程创建失败，则返回 -1 (DWORD(-1))
 */
DWORD ExecuteEcTool(const std::wstring& ectoolPath, const std::wstring& args) {
    // 将路径和参数组合成完整的命令行，并为路径添加引号以处理空格
    std::wstring commandLine = L"\"" + ectoolPath + L"\" " + args;

    // CreateProcessW 的 lpCommandLine 参数需要一个可写的缓冲区
    std::vector<wchar_t> commandLineVec(commandLine.begin(), commandLine.end());
    commandLineVec.push_back(0); // 添加 null 终止符

    STARTUPINFO si;
    PROCESS_INFORMATION pi;
    ZeroMemory(&si, sizeof(si));
    si.cb = sizeof(si);
    ZeroMemory(&pi, sizeof(pi));

    // 创建进程，不显示窗口
    BOOL success = CreateProcessW(
        NULL,
        commandLineVec.data(),
        NULL, NULL, FALSE, CREATE_NO_WINDOW,
        NULL, NULL, &si, &pi
    );

    if (!success) {
        std::wcerr << L"CreateProcess failed for command: " << commandLine << L" Error: " << GetLastError() << std::endl;
        return -1; // 返回一个特殊值表示进程创建失败
    }

    // 等待进程执行完成
    WaitForSingleObject(pi.hProcess, INFINITE);

    // 获取进程的退出码
    DWORD exitCode = -1; // 默认为失败
    GetExitCodeProcess(pi.hProcess, &exitCode);

    // 清理句柄
    CloseHandle(pi.hProcess);
    CloseHandle(pi.hThread);

    std::wcout << L"Command '" << commandLine << L"' finished with exit code: " << exitCode << std::endl;

    return exitCode;
}

/**
 * @brief 获取系统电池电量百分比
 * @return 电量百分比 (0-100)。如果失败或没有电池，则返回 -1。
 */
int GetBatteryPercent() {
    SYSTEM_POWER_STATUS powerStatus;
    if (GetSystemPowerStatus(&powerStatus) == 0) {
        return -1; // API 调用失败
    }

    // BatteryFlag 为 128 表示没有系统电池
    // BatteryLifePercent 为 255 表示未知状态
    if (powerStatus.BatteryFlag == 128 || powerStatus.BatteryLifePercent > 100) {
        return -1; // 没有电池或状态未知
    }

    return static_cast<int>(powerStatus.BatteryLifePercent);
}

int main(int argc, char* argv[]) {
    if (argc > 1 && (strcmp(argv[1], "/?") == 0 || strcmp(argv[1], "/h") == 0 || strcmp(argv[1], "--help") == 0)) {
        MessageBox(NULL,
            L"Usage: program.exe [battery_limit_percent]\n\n"
            L"Example: program.exe 75\n\n"
            L"If no limit is provided, the default value of 80% will be used.",
            L"Help",
            MB_OK | MB_ICONINFORMATION);
        return 0;
    }

    hMutex = CreateMutexW(NULL, TRUE, mutexName);
    if (hMutex == NULL) {
        // 创建互斥体失败，这通常是严重的系统问题
        MessageBox(NULL, L"Failed to create a mutex. The application cannot start.", L"Fatal Error", MB_OK | MB_ICONERROR);
        return 1;
    }

    if (GetLastError() == ERROR_ALREADY_EXISTS) {
        // 互斥体已存在，说明程序已在运行
        MessageBox(NULL, L"The application is already running.", L"Information", MB_OK | MB_ICONINFORMATION);
        // 关闭我们刚刚获取的句柄并退出
        CloseHandle(hMutex);
        return 0; // 正常退出，因为这不是一个错误
    }

    // 获取 ectool.exe 的路径
    std::wstring exeDir = GetExecutableDirectory();
    if (exeDir.empty()) {
        MessageBox(NULL, L"Failed to get the application's path!", L"Fatal Error", MB_OK | MB_ICONERROR);
        CloseHandle(hMutex);
        return 1;
    }
    const std::wstring ectoolPath = exeDir + L"\\ectool.exe";

    // 解析命令行参数以设置电量阈值
    int batteryLimit = 80; // 默认阈值
    if (argc > 1) {
        try {
            int parsedValue = std::stoi(argv[1]);
            if (parsedValue > 0 && parsedValue <= 100) {
                batteryLimit = parsedValue;
            }
        }
        catch (const std::invalid_argument&) {
            MessageBox(NULL, L"The provided battery limit is not a valid number. Using the default value of 80%.", L"Invalid Parameter", MB_OK | MB_ICONWARNING);
        }
        catch (const std::out_of_range&) {
            MessageBox(NULL, L"The provided battery limit is out of the valid range (1-100). Using the default value of 80%.", L"Invalid Parameter", MB_OK | MB_ICONWARNING);
        }
    }

    // 启动时检查与EC的通信是否正常
    if (ExecuteEcTool(ectoolPath, CMD_CHARGE_NORMAL) != 0) {
        MessageBox(NULL,
            L"Failed to communicate with the Embedded Controller (EC).\n"
            L"The application will now exit.\n\n"
            L"Please ensure your device is a Coreboot-based machine (like a Chromebook) and that ectool.exe is located in the same directory.",
            L"Error",
            MB_OK | MB_ICONHAND);
        CloseHandle(hMutex);
        return 1;
    }

    // 启动时执行一次风扇控制序列
    ExecuteEcTool(ectoolPath, CMD_FAN_MAX);
    std::this_thread::sleep_for(10s);
    ExecuteEcTool(ectoolPath, CMD_FAN_AUTO);

    enum class ChargeState { UNKNOWN, NORMAL, IDLE };
    ChargeState currentState = ChargeState::UNKNOWN;
	int continuetime = 0;

    while (true) {
        int percent = GetBatteryPercent();

        if (percent != -1) {
            if (percent >= batteryLimit) {
                // 电量高于或等于阈值
                if (currentState != ChargeState::IDLE) {
                    if (ExecuteEcTool(ectoolPath, CMD_CHARGE_IDLE) == 0) {
                        currentState = ChargeState::IDLE;
                        continuetime = 0; // 状态改变，重置计数器
                    }
                }
            }
            else {
                // 电量低于阈值
                if (currentState != ChargeState::NORMAL) {
                    // 情况1: 状态需要从 IDLE/UNKNOWN 切换到 NORMAL，立即执行
                    if (ExecuteEcTool(ectoolPath, CMD_CHARGE_NORMAL) == 0) {
                        currentState = ChargeState::NORMAL;
                        continuetime = 0; // 状态改变，重置计数器
                    }
                }
                else {
                    // 情况2: 状态已经是 NORMAL，进行周期性再确认
                    continuetime++;
                    if (continuetime >= 5) { // 每 5 个周期 (大约10分钟)
                        std::wcout << L"Periodic re-sync: forcing NORMAL charge mode." << std::endl;
                        ExecuteEcTool(ectoolPath, CMD_CHARGE_NORMAL); // 强制再发一次命令
                        continuetime = 0; // 重置计数器
                    }
                }
            }
        }

        std::this_thread::sleep_for(2min);
    }

    CloseHandle(hMutex);
    return 0;
}