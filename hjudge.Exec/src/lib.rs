#![allow(non_snake_case)]
use std::ffi::CStr;
use std::ffi::CString;
use std::os::raw::c_char;
use std::slice;

#[cfg(not(windows))]
mod judge_linux;
#[cfg(windows)]
mod judge_windows;

mod exec_options;

fn parse_json(json_str: &str) -> Result<exec_options::ExecOptions, String> {
    let options: Result<exec_options::ExecOptions, serde_json::error::Error>= serde_json::from_str(json_str);
    return match options {
        Err(err) => Err(err.to_string()),
        Ok(value) => Ok(value)
    };
}

#[no_mangle]
pub unsafe extern "C" fn execute(param: *const c_char, ret: *mut c_char) -> bool {
    let input = CStr::from_ptr(param).to_string_lossy().into_owned();
    let options = match parse_json(&input) {
        Err(message) => {
            write_result(&message, ret);
            return false;
        }
        Ok(o) => o,
    };
    #[cfg(windows)]
    let result = judge_windows::judge(options);
    #[cfg(not(windows))]
    let result = judge_linux::judge(options);
    return result.1;
}

unsafe fn write_result(message: &str, ret: *mut c_char) {
    let buffer = CString::new(message).unwrap().into_bytes_with_nul();
    let slice = slice::from_raw_parts_mut(ret, buffer.len());
    for index in 0..(buffer.len()) {
        slice[index] = buffer[index] as i8;
    }
}
