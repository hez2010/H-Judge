#pragma once
#pragma comment(lib, "winmm")

#include <Windows.h>
#include <Psapi.h>
#include <sdkddkver.h>
#include <winnt.h>

_declspec(dllexport)
bool execute(const char*, char*);

DWORD WINAPI IOCPThread(LPVOID);
std::string getJsonString(Json::Value);

enum ExitType {
	Normal, TimeLimitExceeded, MemoryLimitExceeded, RuntimeError, UnknownError
};
