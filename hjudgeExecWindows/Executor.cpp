#define _CRT_SECURE_NO_WARNINGS
#include <json.h>
#include <string>
#include <cstring>
#include <sstream>
#include <cstdio>
#include "Executor.h"

ExitType exitInfo = UnknownError;

//Convert a Json::Value object to Json string
std::string getJsonString(Json::Value value)
{
	Json::StreamWriterBuilder wbuilder;
	Json::StreamWriter *writer(wbuilder.newStreamWriter());
	std::ostringstream os;
	writer->write(value, &os);
	delete writer;

	return os.str();
}

bool execute(const char* param, char* ret) {
	//Set error mode, prevent windows error report utils window
	SetErrorMode(SEM_FAILCRITICALERRORS | SEM_NOALIGNMENTFAULTEXCEPT | SEM_NOGPFAULTERRORBOX | SEM_NOOPENFILEERRORBOX);
	SetThreadErrorMode(SEM_FAILCRITICALERRORS | SEM_NOGPFAULTERRORBOX | SEM_NOOPENFILEERRORBOX, NULL);

	//Initialize some varibles
	Json::CharReaderBuilder rbuilder;
	Json::CharReader *reader(rbuilder.newCharReader());
	Json::Value root;
	JSONCPP_STRING errs;

	std::string exec, args, inputFile, outputFile, workingdir;
	long long timeLimit, memoryLimit;
	bool isStdIO;

	Json::Value result;
	result["TimeCost"] = 0;
	result["MemoryCost"] = 0;
	result["Exitcode"] = 0;
	result["ResultType"] = 11;
	result["ExtraInfo"] = "";

	//Parse parameters
	if (reader->parse(param, param + std::strlen(param), &root, &errs) && errs.empty()) {
		exec = root["Exec"].asString(); //Executable path
		args = root["Args"].asString(); //Execute arguments
		workingdir = root["WorkingDir"].asString(); //Environment directory
		inputFile = root["InputFile"].asString(); //Input file path
		outputFile = root["OutputFile"].asString(); //Output file path
		timeLimit = root["TimeLimit"].asInt64(); //Time limit
		memoryLimit = root["MemoryLimit"].asInt64(); //Memory limit
		isStdIO = root["IsStdIO"].asBool(); //Whether to use standard IO
	}
	else { //Parse failed
		delete reader;
		result["ExtraInfo"] = "Failed to parse parameters";
		auto resultToReturn = getJsonString(result);
		std::strcpy(ret, resultToReturn.c_str());
		return false;
	}

	delete reader;

	//Improve time counter accuracy
	timeBeginPeriod(1);

	//Create job object
	HANDLE hJob = CreateJobObject(nullptr, nullptr);

	//Set time, memory, process instance count limits
	JOBOBJECT_EXTENDED_LIMIT_INFORMATION jobLimit = { 0 };
	jobLimit.BasicLimitInformation.LimitFlags =
		JOB_OBJECT_LIMIT_PROCESS_TIME |
		JOB_OBJECT_LIMIT_PRIORITY_CLASS |
		JOB_OBJECT_LIMIT_PROCESS_MEMORY |
		JOB_OBJECT_LIMIT_ACTIVE_PROCESS |
		JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE |
		JOB_OBJECT_LIMIT_DIE_ON_UNHANDLED_EXCEPTION;
	jobLimit.BasicLimitInformation.PerProcessUserTimeLimit.QuadPart = timeLimit * 10000;
	jobLimit.ProcessMemoryLimit = static_cast<SIZE_T>(memoryLimit) << 10;
	jobLimit.BasicLimitInformation.ActiveProcessLimit = 1;
	jobLimit.BasicLimitInformation.PriorityClass = NORMAL_PRIORITY_CLASS;
	SetInformationJobObject(hJob, JobObjectExtendedLimitInformation, &jobLimit, sizeof jobLimit);

	//Set UI restrictions
	JOBOBJECT_BASIC_UI_RESTRICTIONS jobUIRestrictions = { 0 };
	jobUIRestrictions.UIRestrictionsClass =
		JOB_OBJECT_UILIMIT_EXITWINDOWS |
		JOB_OBJECT_UILIMIT_WRITECLIPBOARD |
		JOB_OBJECT_UILIMIT_READCLIPBOARD |
		JOB_OBJECT_UILIMIT_DISPLAYSETTINGS |
		JOB_OBJECT_UILIMIT_GLOBALATOMS |
		JOB_OBJECT_UILIMIT_DESKTOP |
		JOB_OBJECT_UILIMIT_HANDLES |
		JOB_OBJECT_UILIMIT_SYSTEMPARAMETERS;
	SetInformationJobObject(hJob, JobObjectBasicUIRestrictions, &jobUIRestrictions, sizeof jobUIRestrictions);

	//Create Io completion port and thread in order to get notification when limits exceeded
	HANDLE hIOCP = CreateIoCompletionPort(INVALID_HANDLE_VALUE, nullptr, NULL, 1);
	JOBOBJECT_ASSOCIATE_COMPLETION_PORT jobIOCP = { 0 };
	jobIOCP.CompletionKey = hJob;
	jobIOCP.CompletionPort = hIOCP;
	SetInformationJobObject(hJob, JobObjectAssociateCompletionPortInformation, &jobIOCP, sizeof jobIOCP);

	HANDLE hIOCPThread = CreateThread(nullptr, 0, static_cast<LPTHREAD_START_ROUTINE>(IOCPThread), static_cast<LPVOID>(hIOCP), 0, nullptr);

	//Execute command of target program
	std::string commandline = exec.insert(0, "\"").append("\" ").append(args);

	//Initilize process info varibles
	STARTUPINFO si = { sizeof STARTUPINFO };
	SECURITY_ATTRIBUTES sa = { sizeof SECURITY_ATTRIBUTES, nullptr, TRUE };
	PROCESS_INFORMATION pi;

	//Redirect standard input/output
	HANDLE cmdInput = nullptr, cmdOutput = nullptr;

	if (isStdIO) {
		si.dwFlags = STARTF_USESTDHANDLES;
		cmdInput = CreateFile(inputFile.c_str(), GENERIC_READ,
			FILE_SHARE_READ |
			FILE_SHARE_WRITE |
			FILE_SHARE_DELETE,
			&sa, OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, nullptr);

		if (cmdInput == INVALID_HANDLE_VALUE) goto Exit;
		si.hStdInput = cmdInput;

		cmdOutput = CreateFile(outputFile.c_str(), GENERIC_WRITE | GENERIC_READ,
			FILE_SHARE_READ |
			FILE_SHARE_WRITE |
			FILE_SHARE_DELETE,
			&sa, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, nullptr);
		if (cmdOutput == INVALID_HANDLE_VALUE) goto Exit;
		si.hStdOutput = cmdOutput;
	}

	//Create process, and suspend the process at the very beginning
	if (CreateProcess(nullptr, (LPSTR)commandline.c_str(), nullptr, nullptr, TRUE, CREATE_SUSPENDED, nullptr, workingdir.c_str(), &si, &pi)) {
		if (!isStdIO) {
			if (si.hStdInput != nullptr)
				CloseHandle(si.hStdInput);
			if (si.hStdOutput != nullptr)
				CloseHandle(si.hStdOutput);
		}

		//Attach the process to the job object, then resume it
		AssignProcessToJobObject(hJob, pi.hProcess);
		ResumeThread(pi.hThread);
		CloseHandle(pi.hThread);

		//Wait for a while
		DWORD wait = WaitForSingleObject(pi.hProcess, static_cast<DWORD>(timeLimit) * 10);
		JOBOBJECT_EXTENDED_LIMIT_INFORMATION lpJobExtendedInfo;
		JOBOBJECT_BASIC_ACCOUNTING_INFORMATION lpJobBasicInfo;

		DWORD lpReturnLength;

		//Post notification indicated that the process time limit has exceeded
		PostQueuedCompletionStatus(hIOCP, 0, reinterpret_cast<ULONG_PTR>(hJob), nullptr);
		WaitForSingleObject(hIOCPThread, INFINITE);

		//Query process info: time/memory cost, exitcode
		if (QueryInformationJobObject(hJob, JobObjectExtendedLimitInformation, &lpJobExtendedInfo, sizeof lpJobExtendedInfo, &lpReturnLength)
			&& QueryInformationJobObject(hJob, JobObjectBasicAccountingInformation, &lpJobBasicInfo, sizeof lpJobBasicInfo, &lpReturnLength)) {

			auto TimeCost = static_cast<long long>(lpJobBasicInfo.TotalUserTime.QuadPart) / 10000;
			auto MemoryCost = static_cast<long long>(lpJobExtendedInfo.PeakProcessMemoryUsed) >> 10;
			result["TimeCost"] = TimeCost;
			result["MemoryCost"] = MemoryCost;
			result["Exitcode"] = 0;

			if (wait != WAIT_TIMEOUT) {
				result["TimeCost"] = TimeCost;
				result["MemoryCost"] = MemoryCost;

				DWORD Exitcode = 0;
				GetExitCodeProcess(pi.hProcess, &Exitcode);
				result["Exitcode"] = static_cast<int>(Exitcode);

				switch (exitInfo) {
				case Normal:
					result["ResultType"] = 1;
					break;
				case TimeLimitExceeded:
					result["ResultType"] = 4;
					break;
				case MemoryLimitExceeded:
					result["ResultType"] = 5;
					break;
				case RuntimeError:
					result["ResultType"] = 7;
					break;
				case UnknownError:
					result["ResultType"] = 11;
					break;
				}
				if (TimeCost > timeLimit)
					result["ResultType"] = 4;
				if (MemoryCost > MemoryCost)
					result["ResultType"] = 5;
			}
			else if (wait != WAIT_FAILED) {
				result["ResultType"] = 4;
			}
			else {
				result["ResultType"] = 11;
				result["ExtraInfo"] = "Failed to wait job object";
			}
		}
		else {
			result["TimeCost"] = 0;
			result["MemoryCost"] = 0;
			result["Exitcode"] = 0;
			result["ResultType"] = 11;
			result["ExtraInfo"] = "Failed to query job object information";
		}

		//Terminate all and clean up
		TerminateProcess(pi.hProcess, 0);
		TerminateJobObject(hJob, 0);
		TerminateThread(hIOCPThread, 0);

		CloseHandle(pi.hProcess);
		CloseHandle(hIOCPThread);
		CloseHandle(hIOCP);
		CloseHandle(hJob);

		if (isStdIO && cmdInput != nullptr && cmdInput != INVALID_HANDLE_VALUE)
			CloseHandle(cmdInput);
		if (isStdIO && cmdOutput != nullptr && cmdOutput != INVALID_HANDLE_VALUE)
			CloseHandle(cmdOutput);

		//Copy memory from stack to heap in order to pass value to parent function
		auto resultToReturn = getJsonString(result);
		std::strcpy(ret, resultToReturn.c_str());
		return true;
	}

	timeEndPeriod(1);

Exit: //If any operation failed then clean up and exit directly
	result["ExtraInfo"] = "Failed to start progress";
	if (isStdIO && cmdInput != nullptr && cmdInput != INVALID_HANDLE_VALUE)
		CloseHandle(cmdInput);
	if (isStdIO && cmdOutput != nullptr && cmdOutput != INVALID_HANDLE_VALUE)
		CloseHandle(cmdOutput);
	if (pi.hProcess != nullptr && pi.hProcess != INVALID_HANDLE_VALUE)
		CloseHandle(pi.hProcess);
	if (hIOCPThread != nullptr && hIOCPThread != INVALID_HANDLE_VALUE)
		CloseHandle(hIOCPThread);
	if (hIOCP != nullptr && hIOCP != INVALID_HANDLE_VALUE)
		CloseHandle(hIOCP);
	if (hJob != nullptr && hJob != INVALID_HANDLE_VALUE)
		CloseHandle(hJob);

	//Copy memory from stack to heap in order to pass value to parent function
	auto resultToReturn = getJsonString(result);
	std::strcpy(ret, resultToReturn.c_str());
	return false;
}

//Thread for querying notification
DWORD WINAPI IOCPThread(LPVOID lpParam) {
	ULONG_PTR hJob = NULL;
	auto hIocp = static_cast<HANDLE>(lpParam);
	OVERLAPPED* lpOverlapped = nullptr;
	DWORD dwReasonID = 0;

	//Wait for next notification
	while (GetQueuedCompletionStatus(hIocp, &dwReasonID, static_cast<PULONG_PTR>(&hJob), &lpOverlapped, INFINITE))
	{
		switch (dwReasonID)
		{
		case 0:
			return 0;

		case JOB_OBJECT_MSG_ACTIVE_PROCESS_LIMIT: //Process instance count limit exceeded
			exitInfo = RuntimeError;
			return 0;

		case JOB_OBJECT_MSG_END_OF_JOB_TIME: //Process time limit exceeded
			exitInfo = TimeLimitExceeded;
			return 0;

		case JOB_OBJECT_MSG_JOB_MEMORY_LIMIT: //Process memory limit exceeded
			exitInfo = MemoryLimitExceeded;
			return 0;

		case JOB_OBJECT_MSG_EXIT_PROCESS: //Process exited normally
			exitInfo = Normal;
			return 0;

		case JOB_OBJECT_MSG_ABNORMAL_EXIT_PROCESS: //Process exited abnormally
			exitInfo = RuntimeError;
			return 0;

		case JOB_OBJECT_MSG_END_OF_PROCESS_TIME: //Process time limit exceeded
			exitInfo = TimeLimitExceeded;
			return 0;

		case JOB_OBJECT_MSG_PROCESS_MEMORY_LIMIT: //Process memory limit exceeded
			exitInfo = MemoryLimitExceeded;
			return 0;

		default:
			break;
		}

	}

	return 0;
}
