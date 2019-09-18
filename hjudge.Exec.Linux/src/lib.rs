#![allow(non_snake_case)]
#![no_mangle]
use std::os::raw::c_char;
use std::ffi::CString;
static mut STRING_POINTER: *mut c_char = 0 as *mut c_char;

pub extern fn execute(param: &'static str, ret: &'static mut str) -> bool {
    let pntr = CString::new(param).unwrap().into_raw();
    return true;
}