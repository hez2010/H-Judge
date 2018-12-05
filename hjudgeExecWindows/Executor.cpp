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

#define CheckHandle(x) !(x == NULL || x == INVALID_HANDLE_VALUE)

bool execute(const char* param, char* ret) {
	//Set error mode, prevent windows error report utils window
	SetErrorMode(SEM_FAILCRITICALERRORS | SEM_NOALIGNMENTFAULTEXCEPT | SEM_NOGPFAULTERRORBOX | SEM_NOOPENFILEERRORBOX);
	SetThreadErrorMode(SEM_FAILCRITICALERRORS | SEM_NOGPFAULTERRORBOX | SEM_NOOPENFILEERRORBOX, NULL);

	//Initialize some varibles
	Json::CharReaderBuilder rbuilder;
	Json::CharReader *reader(rbuilder.newCharReader());
	Json::Value root;
	JSONCPP_STRING errs;

	std::string exec, args, inputFile, outputFile, workingdir, stdErrFile;
	int activeProcessLimit;
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
		stdErrFile = root["StdErrRedirectFile"].asString(); //Redirect standard error
		inputFile = root["InputFile"].asString(); //Input file path
		outputFile = root["OutputFile"].asString(); //Output file path
		timeLimit = root["TimeLimit"].asInt64(); //Time limit
		memoryLimit = root["MemoryLimit"].asInt64(); //Memory limit
		isStdIO = root["IsStdIO"].asBool(); //Whether to use standard IO
		activeProcessLimit = root["ActiveProcessLimit"].asInt(); //Control active process limit
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

	bool checked = true;

	//Create job object
	HANDLE hJob = CreateJobObject(NULL, NULL);
	checked = CheckHandle(hJob);

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
	jobLimit.BasicLimitInformation.ActiveProcessLimit = activeProcessLimit;
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
	HANDLE hIOCP = CreateIoCompletionPort(INVALID_HANDLE_VALUE, NULL, NULL, 0);
	checked = CheckHandle(hIOCP);

	JOBOBJECT_ASSOCIATE_COMPLETION_PORT jobIOCP = { 0 };
	jobIOCP.CompletionKey = hJob;
	jobIOCP.CompletionPort = hIOCP;
	SetInformationJobObject(hJob, JobObjectAssociateCompletionPortInformation, &jobIOCP, sizeof jobIOCP);

	HANDLE hIOCPThread = CreateThread(NULL, 0, static_cast<LPTHREAD_START_ROUTINE>(IOCPThread), static_cast<LPVOID>(hIOCP), 0, NULL);

	checked = CheckHandle(hIOCPThread);

	//Execute command of target program
	std::string commandline = exec.insert(0, "\"").append("\" ").append(args);

	//Initilize process info varibles
	STARTUPINFO si = { sizeof STARTUPINFO };
	si.dwFlags = STARTF_USESHOWWINDOW | STARTF_USESTDHANDLES;
	si.wShowWindow = SW_HIDE;
	SECURITY_ATTRIBUTES sa = { sizeof SECURITY_ATTRIBUTES, NULL, TRUE };
	PROCESS_INFORMATION pi;

	//Redirect standard input/output
	HANDLE cmdInput = NULL, cmdOutput = NULL, cmdError;

	if (isStdIO) {
		cmdInput = CreateFile(inputFile.c_str(), GENERIC_READ,
			FILE_SHARE_READ |
			FILE_SHARE_WRITE,
			&sa, OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);

		checked = CheckHandle(cmdInput);
		if (checked) si.hStdInput = cmdInput;

		cmdOutput = CreateFile(outputFile.c_str(), GENERIC_WRITE | GENERIC_READ,
			FILE_SHARE_READ |
			FILE_SHARE_WRITE,
			&sa, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);

		checked = CheckHandle(cmdOutput);
		if (checked) si.hStdOutput = cmdOutput;
	}

	cmdError = CreateFile(stdErrFile.c_str(), GENERIC_WRITE | GENERIC_READ,
		FILE_SHARE_READ |
		FILE_SHARE_WRITE,
		&sa, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
	if (checked) si.hStdError = cmdError;

	if (!checked) goto Exit;

	//Create process, and suspend the process at the very beginning
	if (CreateProcess(NULL, (LPSTR)commandline.c_str(), NULL, NULL, TRUE, CREATE_NO_WINDOW | CREATE_SUSPENDED, NULL, workingdir.c_str(), &si, &pi)) {
		if (!isStdIO) {
			if (CheckHandle(si.hStdInput))
				CloseHandle(si.hStdInput);
			if (CheckHandle(si.hStdOutput))
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

		//Post notification to terminate Io completion port thread
		PostQueuedCompletionStatus(hIOCP, 0, reinterpret_cast<ULONG_PTR>(hJob), NULL);
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

		CloseHandle(pi.hProcess);
		CloseHandle(hIOCPThread);
		CloseHandle(hIOCP);
		CloseHandle(hJob);

		if (isStdIO) {
			if (CheckHandle(cmdInput)) CloseHandle(cmdInput);
			if (CheckHandle(cmdError)) {
				FlushFileBuffers(cmdError);
				CloseHandle(cmdError);
			}
			if (CheckHandle(cmdOutput)) {
				FlushFileBuffers(cmdOutput);
				CloseHandle(cmdOutput);
			}
		}

		//Copy memory from stack to heap in order to pass value to parent function
		auto resultToReturn = getJsonString(result);
		std::strcpy(ret, resultToReturn.c_str());
		timeEndPeriod(1);
		return true;
	}
	else PostQueuedCompletionStatus(hIOCP, 0, reinterpret_cast<ULONG_PTR>(hJob), NULL);

	timeEndPeriod(1);

Exit: //If any operation failed then clean up and exit directly
	result["ExtraInfo"] = "Failed to judge";
	if (isStdIO) {
		if (CheckHandle(cmdInput)) CloseHandle(cmdInput);
		if (CheckHandle(cmdError)) {
			FlushFileBuffers(cmdError);
			CloseHandle(cmdError);
		}
		if (CheckHandle(cmdOutput)) {
			FlushFileBuffers(cmdOutput);
			CloseHandle(cmdOutput);
		}
	}
	if (CheckHandle(hIOCPThread))
		CloseHandle(hIOCPThread);
	if (CheckHandle(hIOCP))
		CloseHandle(hIOCP);
	if (CheckHandle(hJob))
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
		case 0: //Stop this thread
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
