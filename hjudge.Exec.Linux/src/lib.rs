#![allow(non_snake_case)]
use std::ffi::CStr;
use std::ffi::CString;
use std::os::raw::c_char;
use std::slice;

#[no_mangle]
pub unsafe extern "C" fn execute(param: *const c_char, ret: *mut c_char) -> bool {
    let input = CStr::from_ptr(param).to_string_lossy().into_owned();
    let result = judge(&input);
    let buffer = CString::new(result.0).unwrap().into_bytes_with_nul();
    let slice = slice::from_raw_parts_mut(ret, result.0.len());
    for index in 0..(result.0.len()) {
        slice[index] = buffer[index] as i8;
    }
    return result.1;
}

fn judge(param: &str) -> (&str, bool) {
    let jsonResult = json::parse(param);
    if !jsonResult.is_ok() {
        return ("Failed to parse JSON.", false);
    }
    let json = jsonResult.unwrap();
    return ("result", true);
}