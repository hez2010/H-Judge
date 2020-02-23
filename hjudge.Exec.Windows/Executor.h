#pragma once
#pragma comment(lib, "winmm")

#include <Windows.h>
#include <Psapi.h>
#include <sdkddkver.h>
#include <winnt.h>

#ifdef __cplusplus
extern "C" {
#endif
    _declspec(dllexport)
        bool execute(const char*, char*);
#ifdef __cplusplus
}
#endif

DWORD WINAPI IOCPThread(LPVOID);
std::string getJsonString(Json::Value);

enum class ExitType {
	Normal, TimeLimitExceeded, MemoryLimitExceeded, RuntimeError, UnknownError
};
