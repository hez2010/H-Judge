extern crate winapi;
use winapi::um::*;
use winapi::ctypes::*;

unsafe fn init(param: super::exec_options::ExecOptions) {
    errhandlingapi::SetErrorMode(
        winbase::SEM_FAILCRITICALERRORS
            | winbase::SEM_NOALIGNMENTFAULTEXCEPT
            | winbase::SEM_NOGPFAULTERRORBOX
            | winbase::SEM_NOOPENFILEERRORBOX,
    );
    errhandlingapi::SetThreadErrorMode(
        winbase::SEM_FAILCRITICALERRORS
            | winbase::SEM_NOGPFAULTERRORBOX
            | winbase::SEM_NOOPENFILEERRORBOX,
        std::ptr::null_mut(),
    );
    timeapi::timeBeginPeriod(1);

    let hJob = jobapi2::CreateJobObjectW(std::ptr::null_mut(), std::ptr::null_mut());
    let mut jobLimit: winnt::JOBOBJECT_EXTENDED_LIMIT_INFORMATION = std::mem::zeroed();
    jobLimit.BasicLimitInformation.LimitFlags = winnt::JOB_OBJECT_LIMIT_PROCESS_TIME
        | winnt::JOB_OBJECT_LIMIT_JOB_TIME
        | winnt::JOB_OBJECT_LIMIT_PRIORITY_CLASS
        | winnt::JOB_OBJECT_LIMIT_PROCESS_MEMORY
        | winnt::JOB_OBJECT_LIMIT_JOB_MEMORY
        | winnt::JOB_OBJECT_LIMIT_ACTIVE_PROCESS
        | winnt::JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE
        | winnt::JOB_OBJECT_LIMIT_DIE_ON_UNHANDLED_EXCEPTION;
    *jobLimit.BasicLimitInformation.PerJobUserTimeLimit.QuadPart_mut() = param.time_limit * 10000;
    *jobLimit.BasicLimitInformation.PerProcessUserTimeLimit.QuadPart_mut() = param.time_limit * 10000;
    jobLimit.ProcessMemoryLimit = (param.memory_limit << 10) as usize;
    jobLimit.JobMemoryLimit = (param.memory_limit << 10) as usize;
    jobLimit.BasicLimitInformation.ActiveProcessLimit = param.active_process_limit;
    jobLimit.BasicLimitInformation.PriorityClass = winbase::NORMAL_PRIORITY_CLASS;

    jobapi2::SetInformationJobObject(hJob, 
        winnt::JobObjectExtendedLimitInformation, 
        &mut jobLimit as winnt::PJOBOBJECT_EXTENDED_LIMIT_INFORMATION as *mut c_void, 
        std::mem::size_of::<winnt::JOBOBJECT_EXTENDED_LIMIT_INFORMATION>() as u32
    );

    let mut jobUIRestrictions: winnt::JOBOBJECT_BASIC_UI_RESTRICTIONS = std::mem::zeroed();
    jobUIRestrictions.UIRestrictionsClass = winnt::JOB_OBJECT_UILIMIT_EXITWINDOWS |
        winnt::JOB_OBJECT_UILIMIT_WRITECLIPBOARD |
        winnt::JOB_OBJECT_UILIMIT_READCLIPBOARD |
        winnt::JOB_OBJECT_UILIMIT_DISPLAYSETTINGS |
        winnt::JOB_OBJECT_UILIMIT_GLOBALATOMS |
        winnt::JOB_OBJECT_UILIMIT_DESKTOP |
        winnt::JOB_OBJECT_UILIMIT_HANDLES |
        winnt::JOB_OBJECT_UILIMIT_SYSTEMPARAMETERS;

    jobapi2::SetInformationJobObject(hJob, 
        winnt::JobObjectBasicUIRestrictions,
        &mut jobUIRestrictions as winnt::PJOBOBJECT_BASIC_UI_RESTRICTIONS as *mut c_void,
        std::mem::size_of::<winnt::JOBOBJECT_BASIC_UI_RESTRICTIONS>() as u32);
}

pub unsafe fn judge(param: super::exec_options::ExecOptions) -> (&'static str, bool) {
    init(param);
    return ("result", true);
}
